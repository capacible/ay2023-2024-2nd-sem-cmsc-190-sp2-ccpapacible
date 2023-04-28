using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player interaction with the associated object.
/// </summary>
public class InteractPrompt : MonoBehaviour
{
    // a simple sprite that pops up when you can interact with a thing.
    public SpriteRenderer interactPrompt;

    // the object that is associated with this prompt; what the player will interact with
    private GameObject parentObject;

    private void Start()
    {
        // get the parent
        parentObject = gameObject.transform.parent.gameObject;
    }

    /// <summary>
    /// If a collider enters the collider of the interactprompt object, tsaka lang magrurun to.
    /// We only remember this object when we collide with the player (player tag), otherwise, we do nothing.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we show the talk prompt renderer only when the collider of player is in the trigger
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player entered bounds of this object");
            // if interactprompt is set, then we show; else we assume wlang prompt.
            if(interactPrompt != null)
            {
                Debug.Log("Showing prompt");
                // show interactprompt; setting the alpha to 1.
                interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 1);
            }

            EventHandler.Instance.CollidingWithPlayer(parentObject);
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player")
        {
            if (interactPrompt != null)
            {
                // hide the interactprompt
                interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 0);
            }

            EventHandler.Instance.NotCollidingWithPlayer(parentObject);

        }
    }
}
