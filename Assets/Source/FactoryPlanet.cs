using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryPlanet : BasePlanet
{
    public List<ResourceRequirement> shipSpawnRequirements = new List<ResourceRequirement>();
    public BaseShip shipPrefab;
    public BasePlanet hq;
    public int defaultSpawnCount = 1;

    public int counter = 0;

    public override void OnBeatSuccess(BasePlanet planet, BeatResult result)
    {
        if (planet != this)
        {
            return;
        }

        int resultBonus = 
            result == BeatResult.Awesome ? 3 : result == BeatResult.Good ? 2 : 1;
        if (planetState == PlanetState.Colonized && CanSpawnShip())
        {
            int units = defaultSpawnCount * resultBonus;
            Debug.LogFormat("Spawning {0} {1} units", units, shipPrefab.shipName);
            for (int i = 0; i < units; ++i)
            {
                BaseShip s = Instantiate<BaseShip>(shipPrefab);
                s.name = string.Format("{0}#{1}", s.shipName, counter++);
                s.SetHQ(hq);
                manager.SpendResourceList(shipSpawnRequirements);
                manager.RegisterShip(s);
            }
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

        Debug.LogFormat("{0} ships took damage", shipPrefab.shipName);
        manager.DamageShipsOfType(shipPrefab.shipName, 10.0f);
    }


    public bool CanSpawnShip()
    {
        foreach (ResourceRequirement req in shipSpawnRequirements)
        {
            if (manager.GetResourceType(req.type) < req.amount)
            {
                return false;
            }
        }
        return true;
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
                    return CanSpawnShip();
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
        else if (planetState == PlanetState.Colonized && !CanSpawnShip())
        {
            foreach (ResourceRequirement req in shipSpawnRequirements)
            {
                if (manager.GetResourceType(req.type) < req.amount)
                {
                    result.Add(manager.GetSmallIconForResourceType(req.type));
                }
            }
        }

        return result;
    }

    public override Sprite GetSpriteForAction()
    {
        if (planetState == PlanetState.Desert && CanColonize())
        {
            return manager.colonizeIcon;
        }
        else return manager.shipIcon;
    }

    public override bool TryGetBuildingTypeSmallIcon(out Sprite sp)
    {
        sp = manager.smallShipIcon;
        return planetState == PlanetState.Desert && CanColonize();
    }
}
