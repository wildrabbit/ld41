using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Population,
    Metal,
    Fuel
}


public class ResourcePlanet : BasePlanet
{
    public ResourceType resourceType;
    public int baseBeatRate = 1;
    public int resourcePenalty = 10;

    public override void OnBeatSuccess(BasePlanet planet, BeatResult result)
    {
        if (planet != this)
        {
            return;
        }
        int resultBonus = result == BeatResult.Awesome ? 2 : 1;
        if (planetState == PlanetState.Colonized)
        {
            int units = baseBeatRate * resultBonus;
            Debug.LogFormat("Adding {0} units of {1}", units, PlanetUtils.GetResourceName(resourceType));
            manager.AddResource(resourceType, units);
        }

        base.OnBeatSuccess(planet, result);
    }

    public override void OnBeatFailed(BasePlanet planet)
    {
        if (planet != this && planetState != PlanetState.Colonized)
        {
            return;
        }
        base.OnBeatFailed(planet);

        Debug.LogFormat("Lost {0} units of {1}", resourcePenalty, PlanetUtils.GetResourceName(resourceType));
        manager.DecreaseResource(resourceType, resourcePenalty);
    }
}
