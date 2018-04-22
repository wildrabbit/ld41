using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlanet : BasePlanet
{
    public List<ResourceRequirement> deploymentCosts;
    public float counterDamage = 10;

    public float damageBonus = 1.1f;

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

        return manager.HasShips();
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
