using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this script is only so that objects don't have to reference each other (or other singletons aside frm this one)
/// directly.
/// Having to rely on them depending on each other gives me a headache when something goes wrong
/// </summary>
public class EventHandler : MonoBehaviour
{
    public static EventHandler Instance;
    
    public string currentMap;

    // ACTIONS

    // DIALOGUE SPECIFIC
    public static event System.Action<object[]> OnDialogueTrigger; // object parameter bc this uses multiple fxns
    public static event System.Action<List<DialogueLine>> OnPlayerLinesFound;

    // the following string params will be changed into DialogueLines later
    public static event System.Action<string, DialogueLine> OnDialogueFound;

    // INVENTORY AND OTHER INTERACTIONS
    public static event System.Action<string, ItemData> OnPickup; // id of object, itemdata of object
    public static event System.Action<GameObject> OnCollision;
    public static event System.Action<GameObject> OnNotCollision;
    
    // OTHER
    public static event System.Action OnInteractConclude;       // when you're done interacting with object

    // ensure that EventHandler is initialized first before all other components
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(Instance);

        InitGame();
    }

    /// <summary>
    /// Initializes what's needed for the game from the TITLE SCREEN
    ///     > sounds / music player
    ///     > scene
    /// </summary>
    private void InitGame()
    {
        Director.Start();
    }

    /// <summary>
    /// Starts the game proper with the actual starting game scene.
    ///     > when start game is selected
    /// The ff function may be changed if may title screen na tayo and stuff; for now, this gets called in SceneHandler
    /// when our starting scene is loaded.
    /// </summary>
    public void StartGame()
    {
        SceneHandler.Instance.LoadUiScene("_Inventory", null);
    }

    /*
     *  DIALOGUE SYSTEM
     */
    /// <summary>
    /// Triggers the start of a dialogue.
    /// </summary>
    /// <param name="npc">NPCData container of the npc.</param>
    public void TriggerDialogue(string npcObjId, Sprite portrait)
    {
        // StartDialogue will run when OnUiLoaded is called within the SceneHandler.
        SceneHandler.OnUiLoaded += StartDialogue;

        SceneHandler.Instance.LoadUiScene("_Dialogue", new object[] { npcObjId, portrait });
        
    }

    /// <summary>
    /// Called when the dialogue UI scene is loaded.
    /// </summary>
    /// <param name="param">An array of parameters that include NPC Data.</param>
    public void StartDialogue(object[] param)
    {
        // we unsubsribe to OnUiLoad since we're done loading the dialogue.
        SceneHandler.OnUiLoaded -= StartDialogue;

        string npcId = (string)param[0];
        Sprite portrait = (Sprite)param[1];

        // Calls and presents the UI for the dialogue.
        OnDialogueTrigger?.Invoke(new object[] { portrait });

        // The speaker should exist within the director's speaker tracker in order to use the director.
        if (Director.SpeakerExists(npcId))
        {
            Debug.Log("speaker exists!");
            DialogueLine line = Director.StartAndGetLine(npcId, currentMap);

            OnDialogueFound?.Invoke(Director.ActiveNPCDisplayName(), line);
        }
        // IF THE NPC USES A TREE
        else
        {
            // do tree related dialogue here

            //OnDialogueFound?.Invoke(npc.)
        }

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
    
    /*
     *  INTERACTIONS
     */
    
    /// <summary>
    /// Called when player enters the interact prompt trigger zone.
    /// </summary>
    /// <param name="collisionObj"> The parent object of the collider or interact prompt. </param>
    public void CollidingWithPlayer(GameObject collisionObj)
    {
        OnCollision?.Invoke(collisionObj);
    }

    public void NotCollidingWithPlayer(GameObject collisionObj)
    {
        OnNotCollision?.Invoke(collisionObj);
    }

    /// <summary>
    /// Called upon interacting with an item, the following events will happen:
    ///     > the inventory will keep track of the item data (INVENTORY HANDLER delegate)
    ///     > inventory popup of the item will pop up on top of the player position (as a notification sort of) (UI delegate)
    ///     > the item destroys itself and removes itself fromt the SceneData. (ITEM delegate)
    /// </summary>
    public void PickupItem(string objId, ItemData item)
    {
        OnPickup?.Invoke(objId, item);
    }
}
