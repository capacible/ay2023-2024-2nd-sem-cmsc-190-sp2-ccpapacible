using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropInteraction : InteractionBase
{
    bool occupied = false;

    // subscribe to TriggerItemEffect
    private void Start()
    {
        EventHandler.ItemEffect += PlaceItem;

        InitializeInteraction();
        Subscribe();
    }

    private void OnDestroy()
    {
        EventHandler.ItemEffect += PlaceItem;

        Unsubscribe();
    }

    /// <summary>
    /// Placing the item
    /// </summary>
    /// <param name="parameters">
    ///     0 object name
    ///     1 item
    ///     2 special
    /// </param>
    private void PlaceItem(object[] parameters)
    {
        if(!occupied && objId.Equals(parameters[0].ToString()))
        {
            ItemBase toDrop = (ItemBase)parameters[1];

            // Instantiate todrop copy
            toDrop = Instantiate(toDrop);

            // delete self
            occupied = true;
        }
        
    }
}
