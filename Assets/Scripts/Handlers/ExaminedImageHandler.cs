using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Deals with images that will be loaded when activating or triggering an ExamineInteraction
/// Is attached to most Interaction scenes
/// </summary>
public class ExaminedImageHandler : MonoBehaviour
{
    public string scene;                // the scene for this script.

    // holds all possible images that can be loaded in the scene this component is placed in.
    public List<Sprite> imgList;

    public Canvas imgCanvas;
    public Image image;                 // the image to load
    public Button exitButton;

    public Fader fader;
    
    /// <summary>
    /// Must immediately be subscribed to stuff, right when we load scene.
    /// </summary>
    void Awake()
    {

        // set camera
        imgCanvas.worldCamera = Camera.main;

        // subscribe to eventHandler events
        EventHandler.Examine += LoadImage;
        Debug.Log("Instantiated examinedimage");
    }

    private void OnDestroy()
    {
        EventHandler.Examine -= LoadImage;
        Debug.Log("destroyed");
    }

    /// <summary>
    ///  Fades the image in
    /// </summary>
    /// <param name="obj">
    ///     [0] ui type
    ///     [1] scene name
    ///     [2] img to load
    /// </param>
    private void LoadImage(object[] obj)
    {
        string imgToLoad = obj[2].ToString();
        Debug.Log("test");
        
        // not empty, we find the image to load.
        if(imgToLoad != "")
        {
            image.sprite = imgList.FirstOrDefault(i => i.name.Contains(imgToLoad));

            Debug.Log("Fading in the image of name: " + image.sprite.name);

            // fade image in
            fader.Fade(image, duration: 0.1, fadeOut: false);
        }
        else
        {
            // if the image list is empty, then there's no images at all to show.
            if(imgList.Count == 0)
            {
                Debug.LogWarning("Neither an image name or the image list was set -- did you mean to call this fxn?");
                return;
            }

            // we load first thing in image list
            image.sprite = imgList[0];

            // fade in
            fader.Fade(image, 0.1, false);

            // if it's empty, just show warning for now.
            Debug.LogWarning("Did not pass an image name. Will use the first image instead.");

        }
    }

    /// <summary>
    /// Fades the image out, then unloads the scene associated w this image
    /// </summary>
    public void Exit()
    {
        fader.Fade(image, 0.1, true);
        // end interaction then unload
        EventHandler.Instance.ConcludeInteraction();
        EventHandler.Instance.UnloadUi(scene);
    }

    [ContextMenu("Get scene name")]
    public void GetSceneName()
    {
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
}
