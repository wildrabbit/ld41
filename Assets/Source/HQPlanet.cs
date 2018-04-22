using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HQPlanet : ResourcePlanet
{
    //int level = 1;

    protected override void Start ()
    {
        resourceType = ResourceType.Population;
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
