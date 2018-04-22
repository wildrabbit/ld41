using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public ResourceRequirement startingResource;

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

    public override void Colonize()
    {
        base.Colonize();
        if (startingResource.amount > 0)
            manager.AddResource(startingResource.type, startingResource.amount);
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

    public override bool ActionAvailable()
    {
        return planetState != PlanetState.Destroyed; // maybe we'll allowed repairing directly here
    }

    public override bool ActionAllowed()
    {
        switch(planetState)
        {
            case PlanetState.Colonized:
            {
                return true;                
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
        if (planetState == PlanetState.Desert && !CanColonize())
        {
            List<Sprite> result = new List<Sprite>();
            foreach (ResourceRequirement req in requirements)
            {
                if (manager.GetResourceType(req.type) < req.amount)
                {
                    result.Add(manager.GetSmallIconForResourceType(req.type));
                }
            }
            return result;
        }
        else return new List<Sprite>();
    }

    public override Sprite GetSpriteForAction()
    {
        if (planetState == PlanetState.Desert && CanColonize())
        {
            return manager.colonizeIcon;
        }
        else return manager.GetIconForResourceType(resourceType);
    }

    public override bool TryGetBuildingTypeSmallIcon(out Sprite sp)
    {
        sp = manager.GetSmallIconForResourceType(resourceType);
        return planetState == PlanetState.Desert && CanColonize();
    }
}
