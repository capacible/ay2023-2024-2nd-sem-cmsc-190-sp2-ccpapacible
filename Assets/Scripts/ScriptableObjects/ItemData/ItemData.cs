using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ScriptableObject
{
    // can be read from csv pero since small project keri na to manual :>
    public string itemId;
    public string itemName;
    public string itemDesc; // description of item (will b displayed)

    // sprite in overworld
    public Sprite itemSprite;
    // sprite in inventory
    public Sprite inventorySprite;
}
