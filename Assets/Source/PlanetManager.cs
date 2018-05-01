using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;

public enum BeatResult
{
    Failed,
    Meh,
    Good,
    Awesome
}

public enum TeamType
{
    Player,
    AI
}

public enum LevelResult
{
    Continue,
    Won,
    Lost    
}

[System.Serializable]
public struct ResourceIconMapping
{
    public ResourceType type;
    public Sprite resourceIcon;
    public ResourceIconMapping(ResourceType _type, Sprite _icon)
    {
        type = _type;
        resourceIcon = _icon;
    }
}

public class PlanetManager : MonoBehaviour
{
    public const double kMinMarkerTime = 0.5;
    public const double kFailureThreshold = 0.2;
    public const double kAwesomeThreshold = 0.08;
    const double kGoodThreshold = 0.2;

    public Sprite colonizeIcon;
    public Sprite shipIcon;
    public Sprite attackIcon;
    public Sprite protectIcon;
    public Sprite repairIcon;
    public Sprite smallShipIcon;

    public Dictionary<ResourceType, int> resources;

    public MetronomeTest metronome;
    public BeatTracker simpleMetronome;

    public IBeatTracker activeMetronome;

    public Timeline timeline;

    BasePlanet[] followers;

    event Action<bool> Beat;
    event Action<BasePlanet, BeatResult> BeatSuccess;
    event Action<BasePlanet> BeatFailed;

    public ResourceCounters resourceCounters;

    public ResourceIconMapping[] resourceIcons;
    public ResourceIconMapping[] smallIcons;


    public Song leSong;

    public double delay = 3.0f;
    double start = 0;

    SpriteRenderer marker;
    AudioSource source;

    int totalTicks = 0;
    int currentBeatInfoIdx = 0;

    int currentBeatMarkerIdx = 0;

    List<int> songBeats = null;
    int lastRelevantBeat = 0;
    int totalSongBeats = 0;
    int lastTouchedBeat = 0;    

    public bool cycle = false;

    public GameObject cover;
    public GameObject howto;

    int stage = 0;


    const string victoryText = "YOU WON! Thanks for playing";
    const string defeatText = "YOU LOST! Keep trying";
    const string restartText = "Press any key to restart";
    public UnityEngine.UI.Text resultText;
    public UnityEngine.UI.Text continueText;
    public Transform resultPanel;

    public List<BaseShip> ships = new List<BaseShip>();

    public int TotalBeats 
    {
        get { return totalTicks;  }
    }

    LevelResult levelResult = LevelResult.Continue;
    public bool running = false;
    public HQPlanet playerBase;
    public EnemyPlanet enemyBase;

    TutorialManager tutorialManager;
    bool paused = false;
    public bool Paused { get { return paused; } }
    bool inputEnabled = false;
    bool aiEnabled = false;

    public void Cycle()
    {
        double next = AudioSettings.dspTime;
        activeMetronome.Reset(next/* + metronome.TimeBetweenBeats*/);
        source.Stop();
        source.PlayScheduled(next/* + metronome.TimeBetweenBeats*/);

        totalTicks = 0;
        currentBeatInfoIdx = 0;
        currentBeatMarkerIdx = 0;
        lastTouchedBeat = 0;
        lastRelevantBeat = songBeats.Count > 0 ? songBeats[songBeats.Count - 1] : -1;
        enemyBase.ResetBeatCounter();
        bool offLimits = false;
        while (!offLimits)
        {
            if (songBeats[currentBeatMarkerIdx] < timeline.maxBeats)
            {
                timeline.AddBeat(songBeats[currentBeatMarkerIdx]);
                currentBeatMarkerIdx++;
            }
            else offLimits = true;
        }
    }

    private void Awake()
    {
#if UNITY_WEBGL
        activeMetronome = simpleMetronome;
        metronome.gameObject.SetActive(false);
#else
        activeMetronome = metronome;
        simpleMetronome.gameObject.SetActive(false);
#endif
        Screen.SetResolution(800, 600, false);
        DOTween.Init();
        followers = Transform.FindObjectsOfType<BasePlanet>();

        tutorialManager = Transform.FindObjectOfType<TutorialManager>();
        source = GetComponent<AudioSource>();
        marker = GetComponentInChildren<SpriteRenderer>();
        if (marker != null) marker.gameObject.SetActive(false);

        leSong.LoadSheet();
        source.clip = leSong.songResource;
        activeMetronome.BPM = leSong.bpm;

        resources = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Fuel, 10 },
            {ResourceType.Metal, 10 },
            {ResourceType.Population, 10 }
        };
    }
    void StartGame()
    {
        start = AudioSettings.dspTime;

        totalSongBeats = (int)Math.Round(source.clip.length * activeMetronome.BPM / 60);
        source.PlayScheduled(start + delay);
        activeMetronome.SetStartTime(start + delay);
        timeline.Init();

        foreach (BasePlanet f in followers)
        {
            Beat += f.OnBeat;
            BeatSuccess += f.OnBeatSuccess;
            BeatFailed += f.OnBeatFailed;
        }


        totalTicks = 0;
        currentBeatInfoIdx = 0;
        lastTouchedBeat = 0;

        songBeats = leSong.GetBeats();
        lastRelevantBeat = songBeats.Count > 0 ? songBeats[songBeats.Count - 1] : -1;
        currentBeatMarkerIdx = 0;
        bool offLimits = false;
        while (!offLimits)
        {
            if (songBeats[currentBeatMarkerIdx] < timeline.maxBeats)
            {
                timeline.AddBeat(songBeats[currentBeatMarkerIdx]);
                currentBeatMarkerIdx++;
            }
            else offLimits = true;
        }
        levelResult = LevelResult.Continue;
        running = true;

        if (tutorialManager && tutorialManager.TutorialEnabled)
        {
            tutorialManager.StartTutorial(0, 1);
        }
    }
    // Use this for initialization
    void Start()
    {

        stage = 0;
        cover.SetActive(true);
        howto.SetActive(false);
        
    }

    float overElapsed = -1.0f;
    float overDelay = 1.5f;
    // Update is called once per frame
    void Update()
    {
        if (stage == 0 && Input.anyKey)
        {
            stage = 1;
            cover.SetActive(false);
            howto.SetActive(true);
            overElapsed = 0.0f;
            return;
        }
        else if (stage == 1)
        {
            if (overElapsed >= 0)
            {
                overElapsed += Time.deltaTime;
                if (overElapsed > overDelay)
                {
                    overElapsed = -1.0f;
                }
            }
            else if (Input.anyKey)
            {
                stage = 2;
                StartGame();
                cover.gameObject.SetActive(false);
                howto.gameObject.SetActive(false);
            }
            return;
        }

        if (!running)
        {
            if (overElapsed >= 0)
            {
                overElapsed += Time.deltaTime;
                if (overElapsed >= overDelay)
                {
                    continueText.gameObject.SetActive(true);
                    overElapsed = -1.0f;
                }
            }
            else if (Input.anyKey)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            return;
        }

        if (paused)
        {
            return;
        }

        BeatEntry currentBeat;
        bool validBeat = leSong.GetBeat(currentBeatInfoIdx, out currentBeat);
        if (!validBeat)
        {
            return;
        }

        int pendingBeats = activeMetronome.NumPendingBeats;
        while (pendingBeats > 0)
        {
            pendingBeats--;
            bool relevantBeat = validBeat && currentBeat.beat == totalTicks;
            Beat(relevantBeat);

            if (currentBeatMarkerIdx < songBeats.Count)
            {
                int offset = songBeats[currentBeatMarkerIdx] - totalTicks;
                if (offset > 0 && offset <= timeline.maxBeats)
                {
                    timeline.AddBeat(offset);
                    currentBeatMarkerIdx++;
                }
            }

            totalTicks++;
            if (totalTicks >= totalSongBeats)
            {
                if (cycle)
                {
                    Cycle();
                }
                else
                {
                    if (playerBase.HP >= enemyBase.HP)
                    {
                        levelResult = LevelResult.Won;
                    }
                    else
                    {
                        levelResult = LevelResult.Lost;
                    }
                }
            }
            else
            {
                if (enemyBase.Destroyed)
                {
                    levelResult = LevelResult.Won;
                }
                else if (playerBase.Destroyed)
                {
                    levelResult = LevelResult.Lost;
                }
            }
        }
        activeMetronome.ResetBeatCount();

        // Update active beat
        double delta = activeMetronome.GetRemainingTimeToTick(currentBeat.beat);
        if (totalTicks <= lastRelevantBeat && delta < -kFailureThreshold*1.002)
        {
            if (lastTouchedBeat != currentBeat.beat)
            {
                Debug.Log("FAILED - NO TOUCH");
            }
            if (currentBeatInfoIdx < leSong.beats.Count - 1)
                currentBeatInfoIdx++;
        }

        if (levelResult != LevelResult.Continue)
        {
            activeMetronome.Stop();
            //source.Stop();
            running = false;
            foreach(BaseShip s in ships)
            {
                s.Hit(s.maxHP);
            }
            overElapsed = 0.0f;
            resultPanel.gameObject.SetActive(true);
            resultText.text = (levelResult == LevelResult.Won) ? victoryText : defeatText;
            continueText.gameObject.SetActive(false);
        }

        List<BaseShip> shipsToRemove = ships.FindAll((s) => s.IsDead);

        foreach (BaseShip s in shipsToRemove)
        {
            // TODO: Proper death logic
            GameObject.Destroy(s.gameObject);
            ships.Remove(s);
        }
    }

    public void EvaluateTap(BasePlanet tappedPlanet)
    {
        BeatEntry be;
        if (!leSong.GetBeat(currentBeatInfoIdx, out be))
        {
            return; 
        }

        double delta = activeMetronome.GetRemainingTimeToTick(be.beat);
        Debug.LogFormat("DELTA: {2}, Expected beat #{0}. Current: {1}", be.beat, totalTicks, delta);

        if (delta < -kFailureThreshold || delta > activeMetronome.TimeBetweenBeats)
        {
            BeatFailed(tappedPlanet);
            return;
        }
        
        if (Math.Abs(delta) <= kAwesomeThreshold) // define threshold
        {
            BeatSuccess(tappedPlanet, BeatResult.Awesome);
        }
        else if (delta > kAwesomeThreshold &&  delta <= kGoodThreshold)
        {
            BeatSuccess(tappedPlanet, BeatResult.Good);
        }
        else
        {
            BeatSuccess(tappedPlanet, BeatResult.Meh);
        }
        lastTouchedBeat = be.beat;
    }

    public List<BeatEntry> GetBeatsInRange()
    {
        return leSong.GetBeatsInRange(activeMetronome.GetTotalElapsed(), 5.0f, 1.0f);
    }

    public List<int> GetBeats()
    {
        return leSong.GetBeats();
    }

    public int GetResourceType(ResourceType type)
    {
        return resources[type];
    }

    public void AddResource(ResourceType type, int units)
    {
        resources[type] += units;
        if (resourceCounters != null)
        {
            resourceCounters.UpdateValue(type, resources[type], true);
        }
    }

    public void DecreaseResource(ResourceType type, int units)
    {
        resources[type] -= units;
        if (resources[type] < 0)
        {
            resources[type] = 0;
        }
        resourceCounters.UpdateValue(type, resources[type]);
    }

    public bool SpendResource(ResourceType type, int units)
    {
        if (resources[type] >= units )
        {
            resources[type] -= units;
            resourceCounters.UpdateValue(type, resources[type]);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SpendResourceList(List<ResourceRequirement> reqs)
    {
        bool allOk = true;
        int idx = -1;
        foreach (ResourceRequirement req in reqs)
        {
            if (resources[req.type] >= req.amount)
            {
                resources[req.type] -= req.amount;
                idx++;
            }
            else
            {
                allOk = false;
                break;
            }
        }
        if (!allOk)
        {
            // Revert :S
            for (int i = idx; i >= 0; --i)
            {
                resources[reqs[i].type] += reqs[i].amount;
            }
        }
        else
        {
            // Apply bump
            foreach(ResourceRequirement req in reqs)
            {
                resourceCounters.UpdateValue(req.type, resources[req.type]);
            }
        }
        return allOk;
    }

    public void RegisterShip(BaseShip s)
    {
        ships.Add(s);
    }

    public void DamageShipsOfType(string type, float penalty)
    {
        List<BaseShip> shipsToDamage = ships.FindAll((ship) => { return ship.shipName == type; });
        foreach (BaseShip s in shipsToDamage)
        {
            s.Hit(penalty);
        }

    }

    public void HitPlayerBase(float damage)
    {
        BasePlanet hq = System.Array.Find(followers, (planet) => planet is HQPlanet);
        if (hq)
        {
            hq.Hit(damage);
        }
    }

    public void LaunchAttack(BasePlanet target, float attackBonus)
    {
        foreach (BaseShip s in ships)
        {
            if (target.TeamID == s.TeamID || s.HasTarget) continue;
            s.SetTarget(target);
            s.ApplyDamageBonus(attackBonus);
        }
    }

    public bool HasShips()
    {
        return ships.Count > 0;
    }

    public List<BasePlanet> GetPlanetsInFaction(TeamType faction)
    {
        return new List<BasePlanet>(System.Array.FindAll(followers, (planet) => planet.TeamID == faction && !planet.Destroyed));
    }

    public List<BaseShip> GetShipsForFaction(TeamType faction)
    {
        return ships.FindAll((ship) => ship.TeamID == faction && !ship.IsDead);
    }

    public List<BasePlanet> GetDestroyedPlanets(TeamType faction)
    {
        return new List<BasePlanet>(System.Array.FindAll(followers, (planet) => planet.TeamID == faction && planet.Destroyed));
    }

    public void SetResourceCounter(ResourceCounters resourceHUD)
    {
        resourceCounters = resourceHUD;
    }

    public Sprite GetIconForResourceType(ResourceType type)
    {
        foreach(ResourceIconMapping mapping in resourceIcons)
        {
            if (type == mapping.type)
            {
                return mapping.resourceIcon;
            }
        }
        return null;
    }

    public Sprite GetSmallIconForResourceType(ResourceType type)
    {
        foreach (ResourceIconMapping mapping in smallIcons)
        {
            if (type == mapping.type)
            {
                return mapping.resourceIcon;
            }
        }
        return null;
    }

    public void DestroyShipsForFaction(TeamType id)
    {
        List<BaseShip> ships = GetShipsForFaction(id);
        foreach(BaseShip s in ships)
        {
            s.Hit(s.maxHP);
        }
    }

    public void PauseLogic(bool paused)
    {
        this.paused = paused;
    }
    
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled && !paused;        
    }

    public void setAIEnabled(bool aiEnabled)
    {
        this.aiEnabled = aiEnabled && !paused;
        enemyBase.Pause(aiEnabled);
    }
}