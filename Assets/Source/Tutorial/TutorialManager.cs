using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TutorialStep
{
    public string id;
    public string eventKey;
    public bool showOverlay = true;    
    public Transform highlightPositionNode;
    public string tutorialText;
    public float duration;
    public bool playNextAutomatically = false;
    public bool skipAllowed = true;
    public bool gameEnabled = false;
    public bool aiEnabled = false;
    public bool gameInputEnabled = false;
}

public class TutorialManager : MonoBehaviour
{
    public bool TutorialEnabled = false;
    public int CurrentTutorialIdx = 0;

    public Canvas tutorialCanvas;
    public Image overlay;
    public PlanetManager planetManager;
    public TutorialStep[] tutorialEntries;
    public Image highlight;
    public GameObject textBox;
    public Text text;

    float elapsed;
    float currentDuration;
    bool running;

    public float startDelay = 2f;

	// Use this for initialization
	void Start ()
    {
        elapsed = -1f;
        tutorialCanvas.enabled = false;
        overlay.gameObject.SetActive(false);
        highlight.gameObject.SetActive(false);
        textBox.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (!running)
        {
            return;
        }

        if (elapsed >= 0)
        {
            TutorialStep step = tutorialEntries[CurrentTutorialIdx];

            elapsed += Time.deltaTime;
            if (elapsed >= currentDuration)
            {
                if (step.playNextAutomatically && CurrentTutorialIdx < tutorialEntries.Length - 1)
                {
                    StartTutorialStepAt(CurrentTutorialIdx + 1);
                }
                else
                {
                    FinishTutorial();
                }
            }
            else
            {
                if (step.skipAllowed && Input.anyKeyDown)
                {
                    if (CurrentTutorialIdx < tutorialEntries.Length - 1)
                        StartTutorialStepAt(CurrentTutorialIdx + 1);
                    else FinishTutorial();
                }
            }
        }
	}

    public void StartTutorial(int idx, float delay)
    {
        if (delay > 0)
        {
            StartCoroutine(StartTutorialDelayed(idx, delay));
        }
        else
        { 
            StartTutorialStepAt(idx);
        }
    }

    public IEnumerator StartTutorialDelayed(int idx, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTutorialStepAt(idx);
    }

    void StartTutorialStepAt(int idx)
    {
        running = true;
        CurrentTutorialIdx = idx;
        TutorialStep step = tutorialEntries[CurrentTutorialIdx];
        currentDuration = step.duration;
        if (currentDuration >= 0)
        {
            elapsed = 0;
        }

        tutorialCanvas.gameObject.SetActive(true);
        tutorialCanvas.enabled = true;
        overlay.gameObject.SetActive(step.showOverlay);
        bool highlightPos = step.highlightPositionNode != null;
        highlight.gameObject.SetActive(highlightPos);
        if (highlightPos)
        {
            highlight.transform.position = step.highlightPositionNode.position;
        }

        bool showText = !string.IsNullOrEmpty(step.tutorialText);
        textBox.gameObject.SetActive(showText);
        if (showText)
        {
            text.text = step.tutorialText;
        }

        planetManager.PauseLogic(!step.gameEnabled);
        planetManager.SetInputEnabled(step.gameInputEnabled);
        planetManager.setAIEnabled(step.aiEnabled);
    }

    void FinishTutorial()
    {
        // Resume game state
        running = false;
        planetManager.SetInputEnabled(true);
        planetManager.setAIEnabled(true);
        planetManager.PauseLogic(false);
        tutorialCanvas.enabled = false;
        overlay.gameObject.SetActive(false);
        highlight.gameObject.SetActive(false);
        textBox.gameObject.SetActive(false);
        elapsed = -1f;
    }

}
