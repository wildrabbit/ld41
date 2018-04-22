using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public enum PlanetState
{
    Desert,
    Colonized,
    Destroyed
}

[System.Serializable]
public struct ResourceRequirement
{
    public ResourceType type;
    public int amount;
    public ResourceRequirement(ResourceType _type, int _amount)
    {
        type = _type;
        amount = _amount;
    }
}

public class BasePlanet : MonoBehaviour
{
    public int ID = 0;
    public PlanetState defaultState = PlanetState.Desert;

    protected PlanetManager manager;
    protected SpriteRenderer spRenderer;
    protected Collider2D planetCollider;

    protected PlanetState planetState = PlanetState.Desert;

    public bool Colonized
    {
        get { return planetState == PlanetState.Colonized; }
    }
    public bool Destroyed
    {
        get { return planetState == PlanetState.Destroyed; }
    }

    PlanetActionHUD actionHUD = null;

    public Color baseColour = new Color(0,1,1,1);
    public Color desertColour = new Color(0, 1, 1, 0.4f);
    public Color disabledColour = new Color(0.3f, 0.3f, 0.3f, 1);

    public float maxHP = 100;
    protected float hp;

    protected TeamType teamID = TeamType.Player;
    public TeamType TeamID
    {
        get { return teamID; }
    }
    

    const int kColourId1 = 999;
    const int kColourId2 = 666;

    public float defaultScale = 1.0f;

    Sequence seq = null;
    Sequence blinkSeq = null;

    public List<ResourceRequirement> requirements = new List<ResourceRequirement>();

    public int HP
    {
        get
        {
            return Mathf.RoundToInt(hp);
        }
    }

    private void Awake()
    {
        manager = Transform.FindObjectOfType<PlanetManager>();
        spRenderer = GetComponent<SpriteRenderer>();
        planetCollider = GetComponent<Collider2D>();
        defaultScale = transform.localScale.x;
        hp = maxHP;
    }

    public void SetState(PlanetState newState, bool init = false)
    {
        if (newState != planetState || init)
        {
            switch(newState)
            {
                case PlanetState.Colonized:
                    {
                        seq.Complete(false);
                        planetCollider.enabled = true;
                        break;
                    }
                case PlanetState.Desert:
                    {
                        planetCollider.enabled = true;
                        break;
                    }
                case PlanetState.Destroyed:
                    {
                        planetCollider.enabled = false;
                        break;
                    }
            }
            spRenderer.color = GetColourForState(newState);
            planetState = newState;
        }
    }

    public void SetActionHUD(PlanetActionHUD planetHUD)
    {
        actionHUD = planetHUD;
    }

    // Use this for initialization
    virtual protected void Start ()
    {
        SetState(defaultState, true);
    }

  public virtual void Colonize()
    {
        SetState(PlanetState.Colonized);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    public virtual void OnBeat(bool isRelevant)
    {
        Sequence mySequence = DOTween.Sequence();
        // Add a 1 second move tween only on the Y axis
        float scale = isRelevant ? 1.5f : 1.05f;
        mySequence.Append(transform.DOScale(defaultScale * scale, 0.15f));
        mySequence.Append(transform.DOScale(defaultScale, 0.04f));

        if (actionHUD != null)
        {
            Sequence hudSeq = DOTween.Sequence();
            Vector3 hudScale = actionHUD.transform.localScale;
            hudSeq.Append(actionHUD.transform.DOScale(hudScale * 1.05f, 0.15f));
            hudSeq.Append(actionHUD.transform.DOScale(hudScale, 0.04f));
        }
    }

    public bool CanColonize()
    {
        foreach (ResourceRequirement req in requirements)
        {
            if (manager.GetResourceType(req.type) < req.amount)
            {
                return false;
            }
        }
        return true;
    }

    public void BlinkColour()
    {
        Color colour = spRenderer.color;
        seq = DOTween.Sequence();
        seq.Append(spRenderer.DOBlendableColor(Color.yellow, 0.1f));
        seq.Append(spRenderer.DOColor(colour, 0.025f));
    }
    public void CheckColonize(BasePlanet planet)
    {

        if (planetState == PlanetState.Desert && CanColonize())
        {
            foreach (ResourceRequirement req in requirements)
            {
                manager.SpendResource(req.type, req.amount);
            }
            Colonize();
        }
    }
    public virtual void OnBeatSuccess(BasePlanet planet, BeatResult result)
    {
        BlinkColour();
        CheckColonize(planet);

    }

    public virtual void OnBeatFailed(BasePlanet planet)
    {
        Color colour = spRenderer.color;
        seq = DOTween.Sequence();
        seq.Append(spRenderer.DOBlendableColor(Color.red, 0.1f));
        seq.Append(spRenderer.DOColor(colour, 0.025f));
    }


    public void OnMouseDown()
    {
         manager.EvaluateTap(this);
    }

    public void Repair()
    {
        SetState(PlanetState.Colonized);
        hp = maxHP;
    }

    public Color GetColourForState(PlanetState st)
    {
        switch(st)
        {
            case PlanetState.Colonized: return baseColour;
            case PlanetState.Desert: return desertColour;
            case PlanetState.Destroyed: return disabledColour;
        }
        return Color.white;
    }

    public bool Hit(float damage)
    {
        Color oldColour = GetColourForState(planetState);

        if (blinkSeq == null)
        {
            blinkSeq = DOTween.Sequence();

            blinkSeq.Append(spRenderer.DOFade(0.0f, 0.05f));
            blinkSeq.Join(spRenderer.DOBlendableColor(Color.red, 0.05f));
            blinkSeq.Append(spRenderer.DOFade(1.0f, 0.05f));
            blinkSeq.Join(spRenderer.DOBlendableColor(oldColour, 0.05f));
            blinkSeq.AppendCallback(() => { spRenderer.color = oldColour; blinkSeq = null; });
            blinkSeq.SetLoops(4);
        }


        hp = Mathf.Max(0, hp - damage);
        Debug.LogFormat("Planet {0} took {1} damage", transform.name, damage);
        if (Mathf.Approximately(hp, 0))
        {
            SetState(PlanetState.Destroyed);
            return true;
        }
        return false;
    }

    public virtual bool ActionAvailable()
    {
        return false;
    }

    public virtual bool ActionAllowed()
    {
        return true;
    }

    public virtual List<Sprite> GetMissingResourcesForAction()
    {
        return null;
    }

    public virtual Sprite GetSpriteForAction()
    {
        return null;
    }

    public virtual bool TryGetBuildingTypeSmallIcon(out Sprite sp)
    {
        sp = null;
        return false;
    }
}
