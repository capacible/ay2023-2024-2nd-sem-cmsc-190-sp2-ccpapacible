using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for fading and unfading the cover of a room upon entry.
/// Attached to Cover prefabs; required the use of a collider
/// </summary>
public class RoomChange : MonoBehaviour
{
    // the sprite of the roof prefab.
    public Sprite roomCover;

    /// <summary>
    /// Upon entry of an object, we slowly unfade the roomCover sprite.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            // unfade
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            // fade to transparency
        }
    }

}
