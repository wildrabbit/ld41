using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public struct ResourceHUDEntry
{
    public ResourceType type;
    public Transform t;
    public Text text;
    public Image img;

    public ResourceHUDEntry(ResourceType _type, Transform _t, Text _text, Image _img)
    {
        type = _type;
        t = _t;
        text = _text;
        img = _img;
    }
}

public class ResourceCounters : MonoBehaviour
{
    public List<ResourceHUDEntry> hudEntries;

    PlanetManager planetManager;

    // Use this for initialization
    void Start () {
        planetManager = Transform.FindObjectOfType<PlanetManager>();
        planetManager.SetResourceCounter(this);

        for(int i = 0; i < hudEntries.Count; ++i)
        {
            ResourceHUDEntry entry = hudEntries[i];
            entry.text.text = string.Format("{0:00000}", planetManager.GetResourceType(entry.type));
            entry.img.sprite = planetManager.GetIconForResourceType(entry.type);
        }        
    }
	
	// Update is called once per frame
	void Update () {
    }

    public void UpdateValue(ResourceType type, int value, bool bump = false)
    {
        int entryIdx = hudEntries.FindIndex ((e) => e.type == type);
        if (entryIdx >= 0)
        {
            ResourceHUDEntry entryVal = hudEntries[entryIdx];
            entryVal.text.text = string.Format("{0:00000}", value);
            if (bump)
            {
                Vector3 imgScale = entryVal.img.transform.localScale;
                entryVal.img.transform.DOPunchScale(new Vector3(imgScale.x * 1.05f, imgScale.y * 1.05f, imgScale.z), 0.2f);
            }
        }

        
    }
}
