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

    private Collider2D parentCollider;
    private PlayerController playerRef = null;

    private void Start()
    {
        // get the parent collider
        parentCollider = gameObject.transform.parent.GetComponent<Collider2D>();
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
            if (playerRef == null)
            {
                // get the reference to playercontroller component only once -> when player ref is null (first time).
                playerRef = other.GetComponent<PlayerController>();
            }

            // show interactprompt; setting the alpha to 1.
            interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 1);

            // add to player collided
            playerRef?.collided.Add(parentCollider);

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player")
        {
            if (playerRef == null)
            {
                // get the reference to playercontroller component only once -> when player ref is null.
                playerRef = other.GetComponent<PlayerController>();
            }

            // hide the interactprompt
            interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 0);

            playerRef?.collided.Remove(parentCollider);

        }
    }
}
