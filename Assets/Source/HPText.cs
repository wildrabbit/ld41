using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPText : MonoBehaviour
{
    public BasePlanet planet;
    Text text;
	// Use this for initialization
	void Start ()
    {
        text = GetComponent<Text>();
        text.text = string.Format("{0}HP", planet.HP);
	}
	
	// Update is called once per frame
	void Update () {
        text.text = string.Format("{0}HP", planet.HP);
    }
}
