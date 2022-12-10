using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPrompt : MonoBehaviour
{
    public SpriteRenderer interactPrompt;
    private Collider2D parentCollider;

    private void Start()
    {
        // get the parent collider
        parentCollider = gameObject.transform.parent.GetComponent<Collider2D>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // we show the talk prompt renderer only when the collider of player is in the trigger
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("player");

            // show interactprompt.
            interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 1);

            // add to player collided
            other.GetComponent<PlayerController>()?.collided.Add(parentCollider);

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player")
        {

            // hide the interactprompt
            interactPrompt.color = new Color(interactPrompt.color.r, interactPrompt.color.g, interactPrompt.color.b, 0);

            other.GetComponent<PlayerController>()?.collided.Remove(parentCollider);

        }
    }
}
