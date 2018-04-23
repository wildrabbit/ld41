using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyDecision
{
    Spawn,
    Deploy
}

public class EnemyPlanet : BasePlanet
{
    public List<ResourceRequirement> deploymentCosts;
    public float counterDamage = 10;

    public float damageBonus = 1.1f;

    public EnemyPlanet hq;
    public BaseShip shipPrefab;

    public int nextDecisionBeat = 0;
    public int maxShipsToDeploy = 6;
    public float deployProbability = 0.6f;

    public int minStartDecisionBeats = 2;
    public int maxStartDecisionBeats = 4;
    public int multStartDecisionBeats = 8;

    public int minDecisionBeats = 1;
    public int maxDecisionBeats = 4;
    public int minDecisionMult = 3;
    public int maxDecisionMult = 4;

    public int minSpawnUnits = 1;
    public int maxSpawnUnits = 3;

    public int counter = 0;

    // Decision logic

    protected override void Start()
    {
        base.Start();
        teamID = TeamType.AI;
        nextDecisionBeat = multStartDecisionBeats * UnityEngine.Random.Range(minStartDecisionBeats, maxStartDecisionBeats);
    }

    public void ResetBeatCounter()
    {
        nextDecisionBeat = multStartDecisionBeats * UnityEngine.Random.Range(minStartDecisionBeats, maxStartDecisionBeats);
    }

    public override void OnBeat(bool isRelevant)
    {
        if (manager.TotalBeats == nextDecisionBeat)
        {
            EnemyDecision decision = EnemyDecision.Spawn;
            List<BaseShip> ships = manager.GetShipsForFaction(teamID);
            if (ships.Count > 0)
            {
                if (ships.Count == maxShipsToDeploy || UnityEngine.Random.value >= deployProbability)
                {
                    decision = EnemyDecision.Deploy;
                }
            }

            switch(decision)
            {
                case EnemyDecision.Spawn:
                    {
                        int units = UnityEngine.Random.Range(minSpawnUnits, maxSpawnUnits);
                        Debug.LogFormat("Spawning {0} {1} enemy units", units, shipPrefab.shipName);
                        for (int i = 0; i < units; ++i)
                        {
                            BaseShip s = Instantiate<BaseShip>(shipPrefab);
                            s.name = string.Format("{0}#{1}", s.shipName, counter++);
                            s.SetHQ(hq);
                            manager.RegisterShip(s);
                        }
                        break;
                    }
                case EnemyDecision.Deploy:
                    {
                        List<BasePlanet> planets = manager.GetPlanetsInFaction(TeamType.Player);
                        // Force attacking only the base
                        BasePlanet target = planets.Find(x => x is HQPlanet);
                        if (target != null)
                            manager.LaunchAttack(target, 0.0f);
                        break;
                    }
            }

            nextDecisionBeat += Random.Range(minDecisionMult,maxDecisionMult) * UnityEngine.Random.Range(minDecisionBeats, maxDecisionBeats);
        }
        base.OnBeat(isRelevant);
    }

    public override void OnBeatSuccess(BasePlanet planet, BeatResult result)
    {
        if (planet != this)
        {
            return;
        }

        float resultBonus = result == BeatResult.Awesome ? damageBonus : 1.0f;
        if (CanAttackPlanet())
        {
            manager.LaunchAttack(this, resultBonus);
        }
        base.OnBeatSuccess(planet, result);
    }

    public bool  CanAttackPlanet()
    {
        foreach (ResourceRequirement req in deploymentCosts)
        {
            if (manager.GetResourceType(req.type) < req.amount)
            {
                return false;
            }
        }

        return manager.GetShipsForFaction(TeamType.Player).Count > 0;
    }
    public override void OnBeatFailed(BasePlanet planet)
    {
        if (planet != this && planetState != PlanetState.Colonized)
        {
            return;
        }
        base.OnBeatFailed(planet);

        Debug.LogFormat("attack was countered");
        manager.HitPlayerBase(counterDamage);
    }


    public override bool ActionAvailable()
    {
        return planetState != PlanetState.Destroyed; // maybe we'll allowed repairing directly here
    }

    public override bool ActionAllowed()
    {
        switch (planetState)
        {
            case PlanetState.Colonized:
                {
                    return CanAttackPlanet();
                }
            case PlanetState.Desert:
                {
                    return CanColonize();
                }
            default:
                return false;
        }
    }

    public override List<Sprite> GetMissingResourcesForAction()
    {
        List<Sprite> result = new List<Sprite>();

        if (planetState == PlanetState.Desert && !CanColonize())
        {
            foreach (ResourceRequirement req in requirements)
            {
                if (manager.GetResourceType(req.type) < req.amount)
                {
                    result.Add(manager.GetSmallIconForResourceType(req.type));
                }
            }
        }
        else if (planetState == PlanetState.Colonized && !CanAttackPlanet())
        {
            foreach (ResourceRequirement req in deploymentCosts)
            {
                if (manager.GetResourceType(req.type) < req.amount)
                {
                    result.Add(manager.GetSmallIconForResourceType(req.type));
                }
            }
            result.Add(manager.smallShipIcon);
        }
        return result;
    }

    public override Sprite GetSpriteForAction()
    {
        if (planetState == PlanetState.Desert && CanColonize())
        {
            return manager.colonizeIcon;
        }
        else if (planetState == PlanetState.Colonized && CanAttackPlanet())
        {
            return manager.attackIcon;
        }
        return null;
    }

    protected override void PlanetDestroyed()
    {
        manager.DestroyShipsForFaction(teamID);
    }
}
