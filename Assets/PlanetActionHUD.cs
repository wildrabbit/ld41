using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetActionHUD : MonoBehaviour
{
    public Sprite availableSpr;
    public Sprite unavailableSpr;

    public BasePlanet planet;
    PlanetManager manager;
    public Image background;
    public Image actionImg;
    public Image buildingImg;

    public Transform missingNode;

    public Image missingSlot1;
    public Image missingSlot2;
    public Image missingSlot3;


    // Use this for initialization
    void Start ()
    {
        manager = Transform.FindObjectOfType<PlanetManager>();
        planet.SetActionHUD(this);
        Refresh();	
	}
	
	// Update is called once per frame
	void Update ()
    {
        Refresh();
	    
	}

    void Refresh()
    {
        bool available = planet.ActionAvailable();
        if (!available)
        {
            background.gameObject.SetActive(false);
            actionImg.gameObject.SetActive(false);
            missingNode.gameObject.SetActive(false);
            buildingImg.gameObject.SetActive(false);
            return;
        }
        else
        {
            Sprite buildingSprite;
            bool requiresBuilding = planet.TryGetBuildingTypeSmallIcon(out buildingSprite);
            buildingImg.gameObject.SetActive(requiresBuilding);
            buildingImg.sprite = buildingSprite;
            background.gameObject.SetActive(true);
            bool allowedAction = planet.ActionAllowed();
            actionImg.gameObject.SetActive(allowedAction);
            missingNode.gameObject.SetActive(!allowedAction);
            if (allowedAction)
            {
                background.sprite = availableSpr;
                actionImg.sprite = planet.GetSpriteForAction();
            }
            else
            {
                background.sprite = unavailableSpr;
                List<Sprite> resources = planet.GetMissingResourcesForAction();
                if (resources.Count == 1)
                {
                    missingSlot2.enabled = false;
                    missingSlot3.enabled = false;
                    missingSlot1.enabled = true;
                    missingSlot1.sprite = resources[0];
                }
                else if (resources.Count == 2)
                {
                    missingSlot1.enabled = false;
                    missingSlot2.enabled = true;
                    missingSlot2.sprite = resources[0];
                    missingSlot3.enabled = true;
                    missingSlot3.sprite = resources[1];
                }
                else if (resources.Count == 3)
                {
                    missingSlot1.enabled = true;
                    missingSlot1.sprite = resources[0];
                    missingSlot2.enabled = true;
                    missingSlot2.sprite = resources[1];
                    missingSlot3.enabled = true;
                    missingSlot3.sprite = resources[2];
                }
            }
        }
    }
}
