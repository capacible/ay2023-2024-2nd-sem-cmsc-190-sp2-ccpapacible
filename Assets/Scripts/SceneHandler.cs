using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;

    // upon creation of the scene handler, we automatically load the first scene.
    [SerializeField]
    private string firstScene;

    // OTHER SCENE NAMES
    private string currentUiScene;
    private string currentMapScene;
    private string toLoad;

    public static event System.Action<object[]> OnUiLoaded;

    // remembering the parameters of the current ui
    private object[] uiParams;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(Instance);

        // load the first scene additively
        SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Loads another sccene (like a dialogue UI) on top of another scene (map)
    /// </summary>
    public void LoadUiScene(string nameOfScene, object[] parameters = null)
    {
        // we can set a callback / delegate to sceneloaded
        SceneManager.sceneLoaded += SceneLoaded;

        Debug.Log("we loading ui scene...");

        toLoad = nameOfScene;
        // we save the UI parameters to be whatever parameter we pass when LoadUiScene is called, whether it be by
        // triggering the dialogue fxn, a menu, etc.
        uiParams = parameters;

        SceneManager.LoadSceneAsync(toLoad, LoadSceneMode.Additive);
    }

    public void UnloadUiScene(string nameOfScene)
    {
        SceneManager.UnloadSceneAsync(nameOfScene);
    }

    /// <summary>
    /// When the scene gets loaded, we set the current scene to be the interactable or instantiable one, and then call a delegate
    /// that all ui-related functions can subscribe to when the right condition is triggered.
    /// </summary>
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("scene is loaded");

        SceneManager.sceneLoaded -= SceneLoaded;

        // set current scene to be active
        currentUiScene = toLoad;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentUiScene));

        // call OnUiLoaded to run the appropriately subscribed function
        OnUiLoaded(uiParams);
    }


}
