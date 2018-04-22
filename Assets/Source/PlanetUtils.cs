using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlanetUtils
{
    public static string GetResourceName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Population:
                return "Population";                
            case ResourceType.Metal:
                return "Metal";
            case ResourceType.Fuel:
                return "Fuel";
            default:
                return "Invalid";
        }
    }
}
