using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script for fading and unfading the cover of a room upon entry.
/// Attached to Cover prefabs; required the use of a collider
/// </summary>
public class RoomChange : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer roomCover;
    [SerializeField]
    private Fader fader;

    /// <summary>
    /// Upon entry of an object, we slowly unfade the roomCover sprite.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            // fade out
            fader.Fade(roomCover, 0.25);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            // fade back to opaque
            fader.Fade(roomCover, 0.25, false);
        }
    }

}
