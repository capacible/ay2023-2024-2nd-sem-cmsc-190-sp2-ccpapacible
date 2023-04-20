using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour
{
    // name of scene to transition to.
    public string moveTo;

    // name of the transitioning object to place yourself in front of.
    public string transitionDest;

    // what's the direction of our offset? This + the size of the player determines by how much yung offset.
    public Vector3 offsetDirection;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if we collide as a player
        if (collision.gameObject.tag == "Player")
        {
            // here we pass the scene location as well as the destination transition object para appropriate location 
            // nilalagay si player
            EventHandler.Instance.TransitionToScene(moveTo, transitionDest);
        }
    }
}
