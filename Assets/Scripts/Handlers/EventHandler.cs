using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Helps determine the ui that will be drawn over the game
/// </summary>
public enum UiType
{
    IN_BACKGROUND,              // EVERYTHING that will appear in the background -- aka nothing shud stop while it happens
    INTERACT_DIALOGUE,
    NPC_DIALOGUE,
    EXAMINE_OBJECT
}

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
    // for functionality that is supposed to return after interaction uis are done, we have to check first if all active uis are
    // gone.
    // we access this in the scripts that implement various ui (gamemessage, dialogue, safe etc)
    public static List<UiType> activeUi = new List<UiType>();
    
    // ACTIONS

    // DIALOGUE SPECIFIC
    public static event System.Action<object[]> StartDialogue;      // object parameter bc this uses multiple fxns
    public static event System.Action<List<string>> FoundPlayerLines;
    public static event System.Action<object[]> FoundNPCLine;
    public static event System.Action<string[]> DisplayNewDebugInfo;

    // INTERACTIONS
    public static event System.Action<object[]> InteractionTriggered;   // triggering any form of interaction
                                                                        // listened 2 by objects, which then calls proper eventhandler fxn

    public static event System.Action OnInteractConclude;               // when you're done interacting with object
    public static event System.Action<object[]> InGameMessage;          // triggers a message to pop up
    public static event System.Action<object[]> Examine;                // invoked when triggering ExamineInteraction

    // ITEMS AND INVENTORY
    public static event System.Action<string, ItemBase> OnPickup;       // id of object, itemdata of object
    public static event System.Action<object[]> ItemEffect;
    public static event System.Action TriggerOnDrop;                    // when item is dropped on floor

    // OTHER
    // handling some collision related events
    public static event System.Action<GameObject> OnPlayerCollision;
    public static event System.Action<GameObject> OnPlayerNotCollision;

    // SCENES
    public static event System.Action<string, object[]> LoadUiScene;
    public static event System.Action<string, object[]> LoadMapScene;
    public static event System.Action<string> UnloadUiScene;
    public static event System.Action<object[]> MapSceneLoaded;
    public static event System.Action<string, bool> SetStateOfObj;

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

    #region DIALOGUE SYSTEM

    /// <summary>
    /// Triggers the start of a dialogue by loading the ui
    /// </summary>
    /// <param name="dialogueParameters"> parameters including
    ///     - npc object id
    ///     - npcdata
    /// </param>
    /// <param name="npcObjId">The specific object id of the npc.</param>
    /// <param name="npc">NPCData container of the npc.</param>
    public void LoadDialogueScene(object[] dialogueParameters)
    {
        // StartDialogue will run when OnUiLoaded is called within the SceneHandler.
        SceneHandler.OnUiLoaded += DialogueSceneLoaded;

        // we load the ui scene, passing an object array of our trigger dialogue parameters.
        LoadUiScene?.Invoke("_Dialogue", dialogueParameters);
        
    }

    /// <summary>
    /// Called when the dialogue UI scene is loaded.
    /// </summary>
    /// <param name="param">An array of parameters that include:
    ///     - npc id                            [0] - the object id
    ///     - npc data                          [1]
    /// </param>
    public void DialogueSceneLoaded(object[] param)
    {
        // we unsubsribe to OnUiLoad since we're done loading the dialogue.
        SceneHandler.OnUiLoaded -= DialogueSceneLoaded;

        string npcId = (string)param[0];
        NPCData npc = (NPCData)param[1];

        // Calls and presents the UI for the dialogue -- this call is for general stuff like the portrait.
        activeUi.Add(UiType.NPC_DIALOGUE);
        StartDialogue?.Invoke(new object[] { UiType.NPC_DIALOGUE, npc });

        // The speaker should exist within the director's speaker tracker in order to use the director.
        if (npc.usesDirector && Director.SpeakerExists(npcId))
        {
            Debug.Log("speaker exists! Director is active.");

            // set the director to be the active dialogue manager
            Director.isActive = true;

            string[] lineData = Director.StartAndGetLine(npcId, SceneUtility.currentScene);
            
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
    /// displays the most recent probabilities acquired from the model
    /// </summary>
    public void UpdateDebugDisplay(string[] toDisplay)
    {
        DisplayNewDebugInfo?.Invoke(toDisplay);
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
        UnloadUi("_Dialogue");

        // set inactive.
        if (Director.isActive) { Director.isActive = false; }
        else if (InkDialogueManager.isActive) { InkDialogueManager.isActive = false; }

        // call conclude interaction so we can remove the npc dialogue ui type and subsequently conclude interaction itself.
        ConcludeInteraction(UiType.NPC_DIALOGUE);
    }

    #endregion

    #region INTERACTIONS
    /// <summary>
    /// Called when player enters the interact prompt trigger zone.
    /// </summary>
    /// <param name="collisionObj"> The parent object of the collider or interact prompt. </param>
    public void CollidingWithPlayer(GameObject collisionObj)
    {
        OnPlayerCollision?.Invoke(collisionObj);
    }

    /// <summary>
    /// When object is no longer colliding w/ player.
    /// </summary>
    /// <param name="collisionObj">Parent object of collider.</param>
    public void NotCollidingWithPlayer(GameObject collisionObj)
    {
        OnPlayerNotCollision?.Invoke(collisionObj);
    }

    /// <summary>
    /// Basic interaction method called by the player; invokes triggerInteraction which all types of itneractions subscribe to.
    /// </summary>
    /// <param name="interactionParams">
    ///     [0] - id of interaction
    ///     [1] - useableItems for interaction
    /// </param>
    public void PlayerInteractWith(object[] interactionParams)
    {
        InteractionTriggered?.Invoke(interactionParams);
    }

    /// <summary>
    /// Calls ItemEffect, a delegate that any object can listen to to trigger some effect given the right item.
    /// </summary>
    /// <param name="parameters">
    ///     [0] - Id of interaction
    ///     [1] - id of item
    ///     [2] - effect id -- for unique effects
    /// </param>
    public void TriggerItemEffect(object[] parameters)
    {
        ItemEffect?.Invoke(parameters);
    }

    /// <summary>
    /// Loads an interaction scene of some name.
    /// </summary>
    /// <param name="parameters">
    ///     [0] - scene to load
    ///     [1] - id of interaction obj
    ///     [2] - img name to load if any
    /// </param>
    public void LoadInteractionScene(object[] parameters)
    {
        string sceneToLoad = parameters[0].ToString();

        // subscribe to scenehandler onuiloaded
        SceneHandler.OnUiLoaded += InteractionSceneLoaded;

        // load ui scene
        LoadUiScene?.Invoke(sceneToLoad, parameters);
    }

    /// <summary>
    /// Implements UiLoaded so this gets ran whenever we load interactionScene. Basically invokes Examine(), which the
    /// Interaction scene-related scripts will listen to and have their own implementations.
    /// </summary>
    /// <param name="sceneParams">
    ///     toLoad scene name
    ///     image to load
    /// </param>
    public void InteractionSceneLoaded(object[] sceneParams)
    {
        SceneHandler.OnUiLoaded -= InteractionSceneLoaded;

        object[] examineParams = new object[sceneParams.Length + 1];
        examineParams[0] = UiType.EXAMINE_OBJECT;

        // copy the scene parameters into what we will pass to examine
        sceneParams.CopyTo(examineParams, 1);

        Debug.Log("Testing if first element is correct: " + examineParams[0]);

        activeUi.Add(UiType.EXAMINE_OBJECT);
        Examine?.Invoke(examineParams);
    }


    /// <summary>
    /// Called upon interacting with an item, the following events will happen:
    ///     > the inventory will keep track of the item data (Inventory Handler will deal w this)
    ///     > inventory popup of the item will pop up on top of the player position (as a notification sort of) (UI)
    /// </summary>
    public void PickupItem(string objId, ItemBase item)
    {
        OnPickup?.Invoke(objId, item);

        Dictionary<string, string> parameters = new Dictionary<string, string> { 
            { MSG_TAG_TYPE.ITEM_NAME, item.itemName }
        };
        
        QuickNotification(GENERIC_MSG_ID.ITEM_PICKUP, parameters);
        
    }

    /// <summary>
    /// Makes a quick notification appear across the top of the screen.
    /// </summary>
    /// <param name="interactKey">The message or interaction key</param>
    /// <param name="msgTags">A dictionary of tags we replace w some value</param>
    public void QuickNotification(string interactKey, Dictionary<string, string> msgTags)
    {
        // invoke found message, passing an object array with the key, tags, and type of message
        InGameMessage?.Invoke(new object[] { UiType.IN_BACKGROUND,  interactKey, msgTags });
    }

    /// <summary>
    /// Makes a dialogue-like text box appear below the screen.
    /// For interact msgs, we have to hide the inventory.
    /// </summary>
    /// <param name="interactKey"></param>
    /// <param name="msgTags"></param>
    public void InteractMessage(string interactKey, Dictionary<string, string> msgTags)
    { 
        // we add interact dialogue to eventhandler activeui
        activeUi.Add(UiType.INTERACT_DIALOGUE);
        InGameMessage?.Invoke(new object[] { UiType.INTERACT_DIALOGUE, interactKey, msgTags });
    }

    public void ConcludeInteraction(UiType? toConcl = null)
    {
        if (toConcl != null)
        {
            // remove certain ui type
            activeUi.Remove((UiType)toConcl);
        }

        OnInteractConclude?.Invoke();
    }
    

    #endregion

    #region SCENE RELATED
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

    /// <summary>
    /// General fxn for unloading a ui scene
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnloadUi(string sceneName)
    {
        UnloadUiScene?.Invoke(sceneName);
    }

    /// <summary>
    /// Called by some scripts to set some object to be interactable / active / whatever
    /// </summary>
    /// <param name="objId">id of the object</param>
    /// <param name="state">state to turn the object to.</param>
    public void SetNewState(string objId, bool state)
    {
        SetStateOfObj?.Invoke(objId, state);
    }

    #endregion
}
