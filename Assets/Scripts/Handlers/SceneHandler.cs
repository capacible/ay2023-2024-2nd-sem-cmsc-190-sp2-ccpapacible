using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance = null;

    // upon creation of the scene handler, we automatically load the first scene.
    [SerializeField]
    private string firstScene;

    // OTHER SCENE NAMES
    private string currentUiScene = "";
    private string currentMapScene = "";
    private string toLoadMap = "";
    private string toLoadUi = "";

    // other

    // integer for keeping track of where exactly in the map the player will go
    // if this is -1, then the player will spawn at default position in the loaded scene.
    private string transitionDestId;

    // some outside function is subscribed here; which dictates what happens when a certain UI is DONE loading.
    public static event System.Action<object[]> OnUiLoaded;

    // remembering the parameters of the current ui
    private object[] uiParams;

    // awake runs first before everything else.
    private void Awake()
    {
        // ensure that we have only ONE instance of this guy
        if (Instance != null)
        {
            Debug.LogWarning("Multiple instances of SceneHandler");
            Destroy(gameObject);
        }

        // dontdestroy this guy
        Instance = this;
        DontDestroyOnLoad(Instance);

        // loadedScene will run after the scene has been loaded. LoadedScene "listens" to sceneLoaded if it happens
        SceneManager.sceneLoaded += LoadedScene;

        // load starting scene
        LoadMapScene(firstScene);

    }

    private void Start()
    {
        //
        //  TEMP CODE BLOCK -- "STARTS" GAME TO INCLUDE ALL STUFF
        //
        EventHandler.Instance.StartGame();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= LoadedScene;
    }

    /*
     *  SCENE LOADING THINGS
     */

    public void LoadMapScene(string nameOfScene, string dest = "")
    {
        Debug.Log("loading map scene..." + nameOfScene);
        
        // first unload the current map scene if meron
        if (currentMapScene != "")
        {
            // unload the map scene we currently are at
            SceneManager.UnloadSceneAsync(currentMapScene);
        }
        
        // load the scene
        toLoadMap = nameOfScene;
        transitionDestId = dest;

        SceneManager.LoadScene(toLoadMap, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Loads another sccene (like a dialogue UI) on top of another scene (map)
    /// </summary>
    public void LoadUiScene(string nameOfScene, object[] parameters = null)
    {
        // we can set a callback / delegate to sceneloaded
        // call SceneLoaded when SceneManager's sceneLoaded function (after loadsceneasync) is done.

        Debug.Log("we loading ui scene...");

        toLoadUi = nameOfScene;
        // we save the UI parameters to be whatever parameter we pass when LoadUiScene is called, whether it be by
        // triggering the dialogue fxn, a menu, etc.
        uiParams = parameters;

        SceneManager.LoadScene(toLoadUi, LoadSceneMode.Additive);
    }

    public void UnloadUiScene(string nameOfScene)
    {
        SceneManager.UnloadSceneAsync(nameOfScene);
    }

    /*
     *  IMPLEMENTATIONS OF SCENELOADED
     */

    /// <summary>
    /// This function implements sceneloaded; when the scene is done loading, we check the type of scene that loaded and
    /// redirect to the proper function kung ano dapat gawin.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void LoadedScene(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("scene " + scene.name + " is loaded");
        
        // we check if ui scene or map scene ang niloload
        if(toLoadUi == scene.name)
        {
            // if UI scene ang niloload; call UiSceneLoaded
            UiSceneLoaded(scene, mode);
        }
        else if(toLoadMap == scene.name)
        {
            MapSceneLoaded(scene, mode);
        }
    }

    /// <summary>
    /// When the scene gets loaded, we set the current scene to be the interactable or instantiable one, and then call a delegate
    /// that all ui-related functions can subscribe to when the right condition is triggered.
    /// </summary>
    private void UiSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("UI scene is loaded");

        // set current scene to be active
        currentUiScene = toLoadUi;

        // call OnUiLoaded to run the appropriately subscribed function
        OnUiLoaded?.Invoke(uiParams);
    }

    /// <summary>
    /// Runs this when the map scene gets loaded successfully.
    ///     This gets run first before Start()
    /// </summary>
    private void MapSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("map scene is loaded");
                        
        // change our current map scene.
        currentMapScene = toLoadMap;
        // we also set the eventhandler's current map to be kung saan na tayo now.
        EventHandler.Instance.currentMap = currentMapScene;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentMapScene));

        // position player first
        SceneData.PositionPlayer(transitionDestId);

        // update scene data once our active scene has been set.
        SceneData.UpdateDataOnLoad(currentMapScene);
        
    }
    
}


/// <summary>
/// SceneData is in charge of remembering data between scenes; ensuring that an item that has been 'picked up'
/// does not get re-instantiated upon leaving and re-entering the scene.
/// </summary>
public static class SceneData
{
    // defines player position at start of scene
    public static Vector3 playerDirOffset;
    public static Vector3? destPosition;

    // keep track of our current scene -- must be same as scenehandler
    public static string currentScene;
    public static Scene activeScene;
    public static List<string> sceneList = new List<string>();

    // a dictionary of scenes that contain a list of ids of objects that exist in the scene when we left it.
    public static Dictionary<string, List<string>> existingObjInScene = new Dictionary<string, List<string>>();
    
    // a copy of data in our objects -- not needed so far, but options r open lol

    /// <summary>
    /// Updates scene data
    /// </summary>
    public static void UpdateDataOnLoad(string currentMapScene)
    {
        // update current scene we are in by remembering the scene name (for the tracker) and the active scene.
        currentScene = currentMapScene;
        activeScene = SceneManager.GetActiveScene();

        // if the scenelist doesnt have the current scene yet (meaning first load palang), we add it to the scenelist.
        // we also initialize the initial objects in this scene into existingObjInScene
        if (!sceneList.Contains(currentMapScene))
        {
            sceneList.Add(currentMapScene);
            existingObjInScene.Add(currentMapScene, new List<string>());
            
            GameObject[] root = activeScene.GetRootGameObjects();

            foreach (GameObject o in root)
            {
                // here we check the objects that are bound to change; ex. pick-up items, characters.
                // we discard or ignore stuff like tilemaps etc.
                if (o.TryGetComponent<Item>(out Item item))
                {
                    // if the object has an item component, we will add it to the existing object in scene
                    existingObjInScene[currentMapScene].Add(item.objId);
                }
                else if(o.TryGetComponent<NPCController>(out NPCController npc))
                {
                    existingObjInScene[currentMapScene].Add(npc.objId);
                }
            }
        }
    }

    /// <summary>
    /// This function simply removes a 'gameobject id' from our current scene.
    /// </summary>
    /// <param name="objId">What to remove</param>
    public static void RemoveObject(string objId)
    {
        existingObjInScene[currentScene].Remove(objId);
    }
    
    // check if the object is in our scene data
    public static bool ObjectIsInScene(string id)
    {
        return existingObjInScene[currentScene].Contains(id);
    }

    /// <summary>
    /// Finds the correct transition object and sets the sceneData to remember the needed positions and offsets for positioning
    /// the player
    /// </summary>
    public static void PositionPlayer(string transitionDestId)
    {
        // if we DO NOT have a transition location in mind, we set player position to be null (default)
        // player controller will adjust accordingly
        if (transitionDestId == "")
        {
            destPosition = null;
            playerDirOffset = new Vector3(0, 0, 0);
            return;
        }

        GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in root)
        {
            // if transition.
            if (go.TryGetComponent(out Transition t))
            {
                if (go.name == transitionDestId)
                {
                    Debug.Log("transport player into location :" + go.name);
                    destPosition = go.transform.position;
                    playerDirOffset = t.offsetDirection;
                }
            }
        }
    }
}