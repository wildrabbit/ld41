using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline : MonoBehaviour
{
    public SpriteRenderer defaultBeatPrefab;
    public Transform beatsRoot;
    public int maxBeats = 10;

    public float offset = 1.0f;
    public float purgeOffset = -0.8f;

    public Transform thresholdArea;

    float speed;

    PlanetManager manager;
    List<SpriteRenderer> beats;

    bool paused = false;


    private void Awake()
    {
        manager = Transform.FindObjectOfType<PlanetManager>();
    }

    // Use this for initialization
    public void Init ()
    {
        beats = new List<SpriteRenderer>();
        float timeToCoverBeat = (float)manager.activeMetronome.TimeBetweenBeats;
        speed = offset / timeToCoverBeat;
        float scaleX = timeToCoverBeat + (float)PlanetManager.kFailureThreshold * offset;
        thresholdArea.localScale = new Vector2(scaleX, thresholdArea.localScale.y);
        float offsetX = scaleX * 0.5f - (float)PlanetManager.kFailureThreshold;
        thresholdArea.localPosition = new Vector2(offsetX, thresholdArea.localPosition.y);

    }

    public void AddBeat()
    {
        AddBeat(maxBeats);
    }

    public void AddBeat(int index)
    {
        SpriteRenderer spawn = Instantiate<SpriteRenderer>(defaultBeatPrefab);
        spawn.transform.SetParent(beatsRoot);
        spawn.transform.localPosition = new Vector2(index * offset, 0);
        beats.Add(spawn);
    }

    // Update is called once per frame
    void Update ()
    {
        if (!manager.running)
        {
            return;
        }

        if (manager.Paused)
        {
            return;
        }

        List<SpriteRenderer> removals = new List<SpriteRenderer>();
        foreach (SpriteRenderer beat in beats)
        {
            if (beat.transform.localPosition.x < purgeOffset)
            {
                GameObject.Destroy(beat.gameObject);
                removals.Add(beat);
            }
            else
            {
                beat.transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
            }
        }

        foreach(SpriteRenderer beat in removals)
        {
            beats.Remove(beat);
        }
	}
}
