using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCounters : MonoBehaviour
{
    public Text mineralsText;
    public Text fuelText;
    public Text populationText;

    PlanetManager planetManager;

    // Use this for initialization
    void Start () {
        planetManager = Transform.FindObjectOfType<PlanetManager>();
        mineralsText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Metal));
        fuelText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Fuel));
        populationText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Population));
    }
	
	// Update is called once per frame
	void Update () {
        mineralsText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Metal));
        fuelText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Fuel));
        populationText.text = string.Format("{0:00000}", planetManager.GetResourceType(ResourceType.Population));
    }
}
