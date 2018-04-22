using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    Slider slider;
    public BasePlanet planet;
    Vector3 defaultScale;
    // Use this for initialization
	void Start ()
    {
        slider = GetComponent<Slider>();
        defaultScale = slider.transform.localScale;

        slider.transform.localScale = planet.Colonized ? defaultScale : Vector3.zero;
        slider.value = planet.HP / planet.maxHP;
	}
	
	// Update is called once per frame
	void Update () {
        slider.transform.localScale = planet.Colonized ? defaultScale : Vector3.zero;
        slider.value = planet.HP / planet.maxHP;
    }
}
