using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zooms in and "examines" something, either by showing an image or whatever
/// </summary>
public class ExamineInteraction : InteractionBase
{

    public string examineSceneToLoad;
    public string imgToLoad;

    private void Start()
    {
        InitializeInteraction();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public override void HandleInteraction(object[] interactParams)
    {

        string id = interactParams[0].ToString();
        if (id == objId)
        {
            // we load the examine scene, passing the name of scene, and the id of this interaction + image to load if any
            EventHandler.Instance.LoadInteractionScene(new object[] { examineSceneToLoad, objId, imgToLoad });

            // if there's an interactkey, then load that too -- on top of the interaction scene
            if (interactionMsgKey != "")
            {
                EventHandler.Instance.InteractMessage(interactionMsgKey, null);
            }
        }
    }

    [ContextMenu("Generate examine interaction object id")]
    public override void GenerateId()
    {
        objId = "ExamineInteraction_";
        if (interactionMsgKey == "")
        {
            // generate based on name of object
            objId += gameObject.name + "_x";
            return;
        }
        // interaction msg key + scene name + guid
        objId += UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + interactionMsgKey + "_x";

    }

}
