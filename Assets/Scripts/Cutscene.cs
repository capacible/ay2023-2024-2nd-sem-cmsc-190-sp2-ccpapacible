using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public TextAsset cutsceneDialogue;  // json text asset
    public SpriteRenderer[] sprites;    // sprites that must be moved in the cutscene
    public NPCData[] relatedNPCData;
    public string[] activateObjects;    // activates objects after cutscene

    public void StartCutscene()
    {
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

        foreach(string o in activateObjects)
        {
            EventHandler.Instance.SetNewState(o, true);
        }
    }
}
