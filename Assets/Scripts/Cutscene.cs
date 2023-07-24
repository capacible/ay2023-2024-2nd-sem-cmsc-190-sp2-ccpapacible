using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public SpriteRenderer cover;
    public Fader fader;

    public TextAsset cutsceneDialogue;  // json text asset
    public SpriteRenderer[] sprites;    // sprites that must be moved in the cutscene
    public NPCData[] relatedNPCData;
    public GameObject[] activateObjects;    // activates objects after cutscene

    private bool startCutscene = false;

    private void Start()
    {
        EventHandler.OnInteractConclude += EndCutscene;
    }

    private void OnDestroy()
    {
        EventHandler.OnInteractConclude -= EndCutscene;
    }

    public void StartCutscene()
    {
        startCutscene = true;

        // activates dialogue ui, thus makes the player busy.
        EventHandler.Instance.LoadDialogueScene(new object[]
        {
            null,
            relatedNPCData,
            cutsceneDialogue
        });

        // all sprites must be activated
        foreach(SpriteRenderer s in sprites)
        {
            s.gameObject.SetActive(true);
        }

        foreach(GameObject o in activateObjects)
        {
            o.SetActive(true);
        }
    }

    private void EndCutscene()
    {
        if(startCutscene)
        {
            // fade the cover
            fader.Fade(cover, fadeOut: false);    // fade in

            // we end the cutscene
            foreach(SpriteRenderer s in sprites)
            {
                s.gameObject.SetActive(false);
            }

            // fade in
            fader.Fade(cover, fadeOut: true);

            startCutscene = false;
        }
    }
}
