using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteraction : InteractionBase
{
    // storage of item data, 
    public ItemBase data;

    private void Start()
    {
        InitializeInteraction();

        // instantiate sprite image
        if (gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer s))
        {
            s.sprite = data.itemSprite;
        }

        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Calls the pickup item method from event handler if this is the object we are interacting with
    /// </summary>
    /// <param name="interactParams"></param>
    public override void HandleInteraction(object[] interactParams)
    {
        string id = interactParams[0].ToString();

        if (id == objId)
        {
            // call pickup item
            EventHandler.Instance.PickupItem(id, data);

            // call item is picked up
            ItemIsPickedUp();
        }
    }

    private void ItemIsPickedUp()
    {
        // we destroy ONLY IF the object id passed is the same as the object id of this item instance. without this
        // if statement all objects might be destroyed as long as its an Item

        // remove from the existingObjects in scene list -- thus game remembers na nakuha mo na to and avoiding dupes
        SceneUtility.RemoveObject(objId);

        // destroy yourself
        Destroy(gameObject);
    }

    [ContextMenu("Generate Item Object Id")]
    public override void GenerateId()
    {
        if (data != null)
        {
            // if we have an attached itemdata, generate id based on itemId + some guid
            objId = "ItemInteraction_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + data.itemId + "_x";
        }
        else
        {
            objId = "TEMP_x";
        }
    }
}
