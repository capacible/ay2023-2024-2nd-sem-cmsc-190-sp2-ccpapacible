using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object to be attached to Item scripts.
/// </summary>
[CreateAssetMenu(fileName = "item", menuName = "Create a new item")]
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

    /// <summary>
    /// Use function of an item, called when the item is being held when interacting, and when the object we are interacting
    /// with has this item as a valid item to interact with.
    /// </summary>
    /// <param name="useOnObj"></param>
    public virtual void UseItem(string useOnObj)
    {
        // show some interact msg
        if(useItemMsgKey!="")
            EventHandler.Instance.InteractMessage(useItemMsgKey, null);

        // possible laman:
        // access the director, to add/remove an event there
            // we have storage of the object id of the object we will use the item on upon interaction
            // if ganto, we can call Director.AddToNPCMemory(string useOnObj, string eventName)
        // deactivate or change the state of some game object (door open -> close, etc)
            // call eventhandler.SetNewState(useObObj, true/false)
        // a problem would be when using an item changes the actual thing where we require a reference of the object itself
            // eventhandler will have to deal w it
    }
}
