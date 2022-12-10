using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public static EventHandler current;
    private string currentMap = "sample";

    // ACTIONS

    // DIALOGUE SPECIFIC
    public static event System.Action<object[]> OnDialogueTrigger; // object parameter bc this uses multiple fxns
    public static event System.Action OnNextDialogue;
    public static event System.Action<NPCData> OnFillerNPCSpawned;

    // the following string params will be changed into DialogueLines later
    public static event System.Action<string, string> OnDialogueFound;
    public static event System.Action<string[]> OnChoicesFound;
    public static event System.Action<string> OnDialogueSelect;

    // OTHER
    public static event System.Action OnInteractConclude;       // when you're done interacting with object

    // ensure that EventHandler is initialized first before all other components
    private void Start()
    {
        current = this;

        DontDestroyOnLoad(current);
    }

    // DIALOGUE SYSTEM EVENTS
    public void TriggerDialogue(NPCData npc)
    {
        OnDialogueTrigger?.Invoke(new object[] { npc.npcPortrait, npc.npcId, currentMap });
    }

    public void DisplayDialogue(string displayType, object[] param)
    {
        if (displayType == "choice")
        {
            // invoke on choice found
            OnChoicesFound?.Invoke((string[]) param[0]);
        }
        else if (displayType == "npc")
        {
            OnDialogueFound?.Invoke(param[0].ToString(), param[1].ToString());
        }
    }

    // this becomes a dialogueLine
    public void DialogueClicked(string selectedLine)
    {
        OnDialogueSelect?.Invoke(selectedLine);
    }

    public void NextDialogue()
    {
        OnNextDialogue?.Invoke();
    }

    public void ConcludeDialogue()
    {
        OnInteractConclude?.Invoke();
    }

    public void AddNPCToManager(NPCData npc)
    {
        OnFillerNPCSpawned?.Invoke(npc);
    }
    
    
}
