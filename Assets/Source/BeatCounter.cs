using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatCounter : MonoBehaviour
{
    PlanetManager manager;
    Text text;

	// Use this for initialization
	void Start () {
        manager = Transform.FindObjectOfType<PlanetManager>();
        text = GetComponent<Text>();
        text.text = string.Format("B: {0}", manager.TotalBeats);
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        text.text = string.Format("B: {0}", manager.TotalBeats);
    }
}
