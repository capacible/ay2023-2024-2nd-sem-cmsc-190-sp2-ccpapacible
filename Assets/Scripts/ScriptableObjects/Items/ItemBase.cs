using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object to be attached to Item scripts.
/// </summary>
[CreateAssetMenu(fileName = "item", menuName = "Items/Create a new item")]
public class ItemBase : ScriptableObject
{
    // can be read from csv pero since small project keri na to manual :>
    public string itemId;
    public string itemName;
    public string itemDesc; // description of item (will b displayed)

    // prompt for when you use the item -- loaded from xml in game msgs
    public string useItemMsgKey;

    // sprite in overworld
    public Sprite itemSprite;
    // sprite in inventory
    public Sprite inventorySprite;

    public bool discardAfter;

    // on use
    public string addToPlayerMemoryOnUse;
    public string addToGlobalOnUse;
    public string addToMapOnUse;

    /// <summary>
    /// Use function of an item, called when the item is being held when interacting, and when the object we are interacting
    /// with has this item as a valid item to interact with.
    /// </summary>
    /// <param name="useOnObjId">
    ///     The id of the object we will use the item on
    /// </param>
    public virtual void UseItem(string useOnObjId)
    {
        if (useItemMsgKey != "")
        {
            Debug.Log("Using item, calling this game msg: " + useItemMsgKey);
            EventHandler.Instance.InteractMessage(useItemMsgKey, new Dictionary<string, string> {
                { MSG_TAG_TYPE.ITEM_NAME, itemName }
            });

            // do item effect, passing itself.
            EventHandler.Instance.TriggerItemEffect(new object[] { useOnObjId, this });
        }
        else
        {
            // no msg
            EventHandler.Instance.TriggerItemEffect(new object[] { useOnObjId, this });
        }

    }
}
