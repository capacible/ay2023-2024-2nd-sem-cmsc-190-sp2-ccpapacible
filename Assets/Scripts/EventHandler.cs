using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public static EventHandler Instance;
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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(Instance);
    }

    // DIALOGUE SYSTEM EVENTS

    /// <summary>
    /// Triggers the start of a dialogue.
    /// </summary>
    /// <param name="npc">NPCData container of the npc.</param>
    public void TriggerDialogue(NPCData npc)
    {
        Debug.Log("dialogue is triggered");
        // upon triggering a dialogue, we subscribe to this delegate. when onuiload is called, that means the UI is done loading
        SceneHandler.OnUiLoaded += StartDialogue;

        // load the dialogue scene
        SceneHandler.Instance.LoadUiScene("_Dialogue", new object[] { npc });
    }

    /// <summary>
    /// Called when the dialogue UI scene is loaded.
    /// </summary>
    /// <param name="param"></param>
    public void StartDialogue(object[] param)
    {
        Debug.Log("we start the dialogue now");
        // we unsubsribe to OnUiLoad since we're done loading the dialogue.
        SceneHandler.OnUiLoaded -= StartDialogue;

        NPCData npc = (NPCData)param[0];

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
        // unload the dialogue scene
        SceneHandler.Instance.UnloadUiScene("_Dialogue");

        OnInteractConclude?.Invoke();
    }

    public void AddNPCToManager(NPCData npc)
    {
        Director.NewFillerSpeaker(npc);
    }
    
}
