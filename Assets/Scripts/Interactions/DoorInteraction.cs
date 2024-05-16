using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : InteractionBase
{
    private bool locked = true;

    private void Start()
    {
        InitializeInteraction();
        Subscribe();

        // door specific interaction
        EventHandler.ItemEffect += OpenDoor;
    }

    private void OnDestroy()
    {
        Unsubscribe();
        EventHandler.ItemEffect -= OpenDoor;
    }

    /// <summary>
    /// Opens or deactivates this door for the duration of the game.
    /// </summary>
    /// <param name="parameters"></param>
    private void OpenDoor(object[] parameters)
    {
        // if the id in the parameters is the same:
        if (parameters[0].ToString() == objId)
        {
            // make the door openable by destroying this object & removing from sceneutility's dict
            Debug.Log("Unlocked door with id: " + objId);

            locked = false;

            if(closeTopic!="")
                Director.CloseTopicForAll(closeTopic);
        }
    }

    public override void HandleInteraction(object[] interactParams)
    {
        if(interactParams[0].ToString() == objId && !locked)
        {
            // open the door permanently
            SceneUtility.RemoveObject(objId);
            SoundHandler.Instance.PlaySFX("open_door", 1);
            Destroy(gameObject);
        }
        // if the above isn't true, then we call base so there's still a default message
        else
        {
            base.HandleInteraction(interactParams);
        }
    }
}
