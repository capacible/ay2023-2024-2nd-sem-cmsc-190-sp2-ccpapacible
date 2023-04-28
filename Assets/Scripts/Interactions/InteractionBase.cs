using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionBase : MonoBehaviour
{
    public string objId;
    public string interactionMsgKey;

    // Start is called before the first frame update
    void Start()
    {
        EventHandler.InteractionTriggered += HandleInteraction;
    }

    private void OnDestroy()
    {
        EventHandler.InteractionTriggered -= HandleInteraction;   
    }

    /// <summary>
    /// Method that will be overridden by other types of interaction
    /// </summary>
    /// <param name="interactParams"></param>
    public virtual void HandleInteraction(object[] interactParams)
    {
        // show interact message
        string id = interactParams[0].ToString();

        // if the id of the object the player interacted with is the same as the id of this interaction
        if(id == objId)
        {
            // some default task -- trigger an interaction message
            EventHandler.Instance.InteractMessage(interactionMsgKey, null);
        }
    }

    [ContextMenu("Generate interaction object id")]
    public void GenerateInteractionId()
    {
        // interaction msg key + scene name + guid
        objId = interactionMsgKey + "_" + System.Guid.NewGuid().ToString();
    }
}
