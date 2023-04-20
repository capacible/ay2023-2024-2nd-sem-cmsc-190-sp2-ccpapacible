using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EventHandler handles all interactions between scripts and singletons so that we can minimize dependencies
/// between scripts -- all scripts depend on this one only and nothing else
/// 
/// The EventHandler also handles the values to pass for the dialogue system considering that we have 2 versions of it--
/// the one with the director and the other with Ink.
/// </summary>
public class EventHandler : MonoBehaviour
{
    public static EventHandler Instance;
    
    // ACTIONS

    // DIALOGUE SPECIFIC
    public static event System.Action<object[]> TriggeredDialogue; // object parameter bc this uses multiple fxns
    public static event System.Action<List<string>> FoundPlayerLines;
    public static event System.Action<object[]> FoundNPCLine;

    // INVENTORY AND OTHER INTERACTIONS
    public static event System.Action<string, ItemData> OnPickup; // id of object, itemdata of object
    public static event System.Action<GameObject> OnCollision;
    public static event System.Action<GameObject> OnNotCollision;
    
    // OTHER
    public static event System.Action OnInteractConclude;       // when you're done interacting with object

    // scenes
    public static event System.Action<string, object[]> LoadUiScene;
    public static event System.Action<string, object[]> LoadMapScene;
    public static event System.Action<string> UnloadUiScene;
    public static event System.Action<object[]> MapSceneLoaded;

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
        LoadUiScene?.Invoke("_Inventory", null);
    }

    /*
     *  DIALOGUE SYSTEM
     */
    /// <summary>
    /// Triggers the start of a dialogue by loading the ui.
    /// </summary>
    /// <param name="dialogueParameters"> parameters including
    ///     - npc object id
    ///     - npcdata
    ///     - displayname override if any.
    /// </param>
    /// <param name="npcObjId">The specific object id of the npc.</param>
    /// <param name="npc">NPCData container of the npc.</param>
    public void TriggerDialogue(object[] dialogueParameters)
    {
        // StartDialogue will run when OnUiLoaded is called within the SceneHandler.
        SceneHandler.OnUiLoaded += StartDialogue;

        // we load the ui scene, passing an object array of our trigger dialogue parameters.
        LoadUiScene?.Invoke("_Dialogue", dialogueParameters);
        
    }

    /// <summary>
    /// Called when the dialogue UI scene is loaded.
    /// </summary>
    /// <param name="param">An array of parameters that include:
    ///     - npc id                            [0]
    ///     - npc data                          [1]
    /// </param>
    public void StartDialogue(object[] param)
    {
        // we unsubsribe to OnUiLoad since we're done loading the dialogue.
        SceneHandler.OnUiLoaded -= StartDialogue;

        string npcId = (string)param[0];
        NPCData npc = (NPCData)param[1];

        // Calls and presents the UI for the dialogue -- this call is for general stuff like the portrait.
        TriggeredDialogue?.Invoke(new object[] { npc });

        // The speaker should exist within the director's speaker tracker in order to use the director.
        if (!npc.usesDirector && Director.SpeakerExists(npcId))
        {
            Debug.Log("speaker exists! Director is active.");

            // set the director to be the active dialogue manager
            Director.isActive = true;

            string[] lineData = Director.StartAndGetLine(npcId, SceneData.currentScene);
            
            FoundNPCLine?.Invoke(
                new object[] {
                    Director.ActiveNPCDisplayName(),
                    lineData[0],                    // the line itself
                    lineData[1]                    // the dialogue portrait.
                });
        }
        // IF THE NPC USES A TREE
        else
        {
            Debug.Log("We use InkHandler for this one");

            // set ink handler to be active dialogue manager
            InkDialogueManager.isActive = true;

            string[] lineData = InkDialogueManager.StartDialogue(npc.inkJSON);

            FoundNPCLine?.Invoke(new object[] {
                InkDialogueManager.ActiveNPCDisplayName(),
                lineData[0],                    // line.      
                lineData[1]                     // dialogue portrait
            });
        }

    }

    /// <summary>
    /// Display NPC line response given the player's selection.
    /// </summary>
    /// <param name="selectedLine"></param>
    public void DisplayNPCLine(int selectedLine)
    {
        if (Director.isActive)
        {
            Debug.Log("Director");
            string[] lineData = Director.GetNPCLine(selectedLine);
            
            FoundNPCLine?.Invoke(new object[] 
            {
                Director.ActiveNPCDisplayName(),
                lineData[0],                    // the line itself
                lineData[1]                    // the dialogue portrait.
            });
        }
        else if (InkDialogueManager.isActive)
        {
            Debug.Log("InkHandler");

            string[] lineData = InkDialogueManager.BranchOutGivenChoice(selectedLine);

            FoundNPCLine?.Invoke(new object[] {
                InkDialogueManager.ActiveNPCDisplayName(),
                lineData[0],                    // line.      
                lineData[1]                     // dialogue portrait
            });
            
        }
    }

    // triggers the player choice menu.
    public void DisplayPlayerLines()
    {
        if (Director.isActive)
        {
            Debug.Log("director");
            // display the lines acquired.
            FoundPlayerLines?.Invoke(Director.GetPlayerLines());
        }
        else if (InkDialogueManager.isActive)
        {
            Debug.Log("ink handler");
            FoundPlayerLines?.Invoke(InkDialogueManager.GetPlayerChoices());
        }
    }

    /// <summary>
    /// Unloads the dialogue scene.
    /// </summary>
    public void ConcludeDialogue()
    {
        // unload the dialogue scene
        UnloadUiScene?.Invoke("_Dialogue");

        // set inactive.
        if (Director.isActive) { Director.isActive = false; }
        else if (InkDialogueManager.isActive) { InkDialogueManager.isActive = false; }

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

    /// <summary>
    /// When object is no longer colliding w/ player.
    /// </summary>
    /// <param name="collisionObj">Parent object of collider.</param>
    public void NotCollidingWithPlayer(GameObject collisionObj)
    {
        OnNotCollision?.Invoke(collisionObj);
    }

    /// <summary>
    /// Called upon interacting with an item, the following events will happen:
    ///     > the inventory will keep track of the item data (Inventory Handler will deal w this)
    ///     > inventory popup of the item will pop up on top of the player position (as a notification sort of) (UI)
    ///     > the item destroys itself and removes itself fromt the SceneData. (Item script deals with this)
    /// </summary>
    public void PickupItem(string objId, ItemData item)
    {
        OnPickup?.Invoke(objId, item);
    }

    /*
     *   MAP SCENEHANDLER REFERENCES
     */

    /// <summary>
    /// Changes scene
    /// </summary>
    /// <param name="sceneName">Name of scene to switch to</param>
    /// <param name="transitionTo">The exact location or transition object name</param>
    public void TransitionToScene(string sceneName, string transitionTo)
    {
        LoadMapScene?.Invoke(sceneName, new object[] { transitionTo });
    }

    /// <summary>
    /// Called when done changing scenes.
    /// </summary>
    /// <param name="param"> A list of parameters that associated listeners/observers may use.
    ///     - so far this list is empty.
    /// </param>
    public void LoadedMapScene(object[] param)
    {
        MapSceneLoaded?.Invoke(param);
    }
}
