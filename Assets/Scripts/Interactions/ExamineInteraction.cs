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

            EventHandler.Instance.LoadInteractionScene(new object[] { examineSceneToLoad, imgToLoad });

            // if there's an interactkey, then load that too -- on top of the interaction scene
            if (interactionMsgKey != "")
            {
                EventHandler.Instance.InteractMessage(interactionMsgKey, null);
            }
        }
    }
}
