﻿using System.Collections;
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

    public int counter = 0;
    // Decision logic

    protected override void Start()
    {
        base.Start();
        teamID = TeamType.AI;
        nextDecisionBeat = 8 * UnityEngine.Random.Range(2, 4);
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
                        int units = UnityEngine.Random.Range(1, 2);
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
                        BasePlanet target = planets[UnityEngine.Random.Range(0, planets.Count - 1)];
                        manager.LaunchAttack(target, 0.0f);
                        break;
                    }
            }

            nextDecisionBeat += Random.Range(3,6) * UnityEngine.Random.Range(1, 4);
        }
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

}
