using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HQPlanet : ResourcePlanet
{
    public ResourceRequirement repairCost = new ResourceRequirement(ResourceType.Population, 10);
    //int level = 1;

    protected override void Start()
    {
        resourceType = ResourceType.Population;
        base.Start();
    }

    public override void OnBeatSuccess(BasePlanet planet, BeatResult result)
    {
        if (planet != this)
        {
            return;
        }
        int resultBonus = result == BeatResult.Awesome ? 2 : 1;
        if (planetState == PlanetState.Colonized)
        {
            List<BasePlanet> destroyed = manager.GetDestroyedPlanets(teamID);
            if (destroyed.Count > 0 && manager.GetResourceType(repairCost.type) >= repairCost.amount)
            {
                int idx = UnityEngine.Random.Range(0, destroyed.Count - 1);
                destroyed[idx].Repair();
                manager.SpendResource(repairCost.type, repairCost.amount);
            }
            else
            {
                int units = baseBeatRate * resultBonus;
                Debug.LogFormat("Adding {0} units of {1}", units, PlanetUtils.GetResourceName(resourceType));
                manager.AddResource(resourceType, units);
            }
        }
        BlinkColour();
        CheckColonize(planet);
    }

    public override bool ActionAvailable()
    {
        return planetState != PlanetState.Destroyed; // maybe we'll allowed repairing directly here
    }

    public override bool ActionAllowed()
    {
        return ActionAvailable();
    }

    public override List<Sprite> GetMissingResourcesForAction()
    {
        return new List<Sprite>();
    }

    public override Sprite GetSpriteForAction()
    {
        return manager.GetIconForResourceType(resourceType);
    }
}
