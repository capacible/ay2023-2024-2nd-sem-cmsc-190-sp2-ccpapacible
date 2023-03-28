using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // unique id for the object attached.
    public string objId;

    // storage of item data, 
    public ItemData data;

    private void Start()
    {
        // we check if the item exists in the scene
        if (!SceneData.ObjectIsInScene(objId))
        {
            Debug.Log("This item has been removed from the scene before you left.");
            Destroy(gameObject);
        }

        // instantiate sprite image
        if(gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer s))
        {
            s.sprite = data.itemSprite;
        }

        // listen to on pickup event
        EventHandler.OnPickup += ItemIsPickedUp;
        
    }

    private void OnDestroy()
    {
        EventHandler.OnPickup -= ItemIsPickedUp;
    }

    private void ItemIsPickedUp(string id, ItemData item)
    {
        // we destroy ONLY IF the object id passed is the same as the object id of this item instance. without this
        // if statement all objects might be destroyed as long as its an Item
        if (objId == id)
        {
            SceneData.RemoveObject(objId);

            // destroy yourself
            Destroy(gameObject);
        }
    }

    [ContextMenu("Generate Item Object Id")]
    public void GenerateObjectId()
    {
        if (data != null)
        {
            // if we have an attached itemdata, generate id based on itemId + some guid
            objId = data.itemId + System.Guid.NewGuid().ToString();
        }
        else
        {
            objId = "TEMP_" + System.Guid.NewGuid().ToString();
        }
    }
}
