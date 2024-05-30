using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static string TUTORIAL_SCENE_PREFIX = "Tutorial_";
    
    public Button next;
    public Button start;
    public Button previous;
    public Canvas mainCanvas;
    // we put the parent UI gameobjects into this array in the page order.
    public GameObject[] tutorialPages;
    private int currentTutorialPage = 0;

    private void Start()
    {
        mainCanvas.worldCamera = Camera.main;
    }

    public void Next()
    {
        SoundHandler.Instance.PlaySFX("select_button_2");

        // hide current page
        tutorialPages[currentTutorialPage].SetActive(false);

        // load the next page
        tutorialPages[++currentTutorialPage].SetActive(true);

        // if max page na, hide next then show start game
        if (currentTutorialPage + 1 == tutorialPages.Length)
        {
            next.gameObject.SetActive(false);
            start.gameObject.SetActive(true);
        }
        else if (currentTutorialPage > 0)
        {
            previous.gameObject.SetActive(true);
        }
    }

    public void Previous()
    {
        SoundHandler.Instance.PlaySFX("select_button_2");

        // hide current page
        tutorialPages[currentTutorialPage].SetActive(false);

        // load the next page
        tutorialPages[--currentTutorialPage].SetActive(true);

        // if the page is minimum na, set prev to be inactive
        if (currentTutorialPage == 0)
        {
            previous.gameObject.SetActive(false);
        }
        else if(currentTutorialPage + 1 != tutorialPages.Length)
        {
            if(start.gameObject.activeInHierarchy)
                start.gameObject.SetActive(false);

            next.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        SoundHandler.Instance.PlaySFX("select_button");
        EventHandler.Instance.LoadUi("_NarrationScene", new object[] { true, null, "Floor1" });
        EventHandler.Instance.UnloadUi("Tutorial_Base");
    }
}
