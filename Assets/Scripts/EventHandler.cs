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
    public static event System.Action<List<DialogueLine>> OnPlayerLinesFound;

    // the following string params will be changed into DialogueLines later
    public static event System.Action<string, DialogueLine> OnDialogueFound;

    // OTHER
    public static event System.Action OnInteractConclude;       // when you're done interacting with object

    // ensure that EventHandler is initialized first before all other components
    private void Start()
    {
        current = this;

        DontDestroyOnLoad(current);
    }

    // DIALOGUE SYSTEM EVENTS

    /// <summary>
    /// Triggers the start of a dialogue.
    /// </summary>
    /// <param name="npc">NPCData container of the npc.</param>
    public void TriggerDialogue(NPCData npc)
    {

        // on dialogue trigger is now only for the ui, remove the last 2 elements in it
        OnDialogueTrigger?.Invoke(new object[] { npc.npcPortrait });

        DialogueLine line = Director.StartAndGetLine(npc.npcId, currentMap);

        OnDialogueFound?.Invoke(Director.ActiveNPCDisplayName(), line);
    }

    public void DisplayNPCLine(DialogueLine selectedLine)
    {
        OnDialogueFound?.Invoke(Director.ActiveNPCDisplayName(), Director.GetNPCLine(selectedLine));
    }

    // triggers the player choice menu.
    public void DisplayPlayerLines()
    {
        // display the lines acquired.
        OnPlayerLinesFound?.Invoke( Director.GetPlayerLines() );
    }

    public void ConcludeDialogue()
    {
        OnInteractConclude?.Invoke();
    }

    public void AddNPCToManager(NPCData npc)
    {
        Director.NewFillerSpeaker(npc);
    }
    
    
}
