using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basically like an interaction with message, but activates by entering the collider range.
/// </summary>
public class MessagePopUp : InteractionBase
{     
    // Start is called before the first frame update
    void Start()
    {
        InitializeInteraction();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
            
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.ToLower() == "player" && interactable)
        {
            // show message
            EventHandler.Instance.InteractMessage(interactionMsgKey, null);
            // delete self
            EventHandler.Instance.SetNewState(objId, false);
        }
    }
}
