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

    public virtual void UseItem()
    {
        // show some interact msg
        if(useItemMsgKey!="")
            EventHandler.Instance.InteractMessage(useItemMsgKey, null);

        // possible laman:
        // access the director, to add/remove an event there
        // deactivate or change the state of some game object (door open -> close, etc) => need a global way to do this though
        // eventhandler to the rescue? - interaction objects may have certain states? idk
    }
}
