using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The interaction script for the vase puzzle
/// </summary>
public class VasePuzzlePartInteraction : InteractionBase
{
    public static readonly string INVALID_ITEM = "wrong_item";

    public SpriteRenderer spriterenderer;   // renders the item to be dropped
    public string mainPuzzleId;
    public string requiredItemId;
    public int puzzlePartNum;
    bool occupied = false;
    private ItemBase heldItem;  // item that this drop slot holds

    // subscribe to TriggerItemEffect
    private void Start()
    {
        InitializeInteraction();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public override void HandleInteraction(object[] parameters)
    {
        if (!interactable)
        {
            return;
        }

        ItemBase toDrop = InventoryHandler.CurrentItem();

        if(objId.Equals(parameters[0].ToString()) && toDrop == null)
        {
            EventHandler.Instance.PickupItem(objId, heldItem);
        }
        else if (objId.Equals(parameters[0].ToString()) && useableItems.Contains(toDrop))
        {
            // if it's a valid item to drop
            if (useableItems.Contains(toDrop))
            {
                if (!occupied)
                {
                    // drop the item without any fanfare
                    DropItem(toDrop);
                }
                else
                {
                    // re-add the previous item to the inventory
                    EventHandler.Instance.PickupItem(objId, heldItem);

                    if (toDrop != null)
                    {
                        // replace held item with new item (toDrop)
                        heldItem = toDrop;
                        // change the rendered sprite
                        spriterenderer.sprite = heldItem.itemSprite;
                        // remove the item current from inventory
                        EventHandler.Instance.DropItem();
                    }
                    else
                    {
                        // pick up and change
                        heldItem = null;
                        // change rendered item to null?
                        spriterenderer.sprite = null;
                    }

                }

                // check if this item that is dropped satisfies our required item
                if (toDrop.itemId == requiredItemId)
                {
                    // if yes, we invoke puzzle progress to update the puzzle interaction of the vase.
                    PuzzleInteraction.OnPuzzleProgress?.Invoke(mainPuzzleId, puzzlePartNum, false);
                }
            }
            else
            {
                EventHandler.Instance.InteractMessage(INVALID_ITEM, null);
            }
        }

    }

    /// <summary>
    /// Placing the item
    ///     ISSUE: we use item effect in order to handle this interaction -- thus, if there is no item in inventory, the item
    ///     cannot be picked up
    /// </summary>
    /// <param name="parameters">
    ///     0 object name
    ///     1 item
    ///     2 special
    /// </param>
    private void PlaceItem(object[] parameters)
    {
    }

    private void DropItem(ItemBase toDrop)
    {
        // change the dropped sprite
        spriterenderer.sprite = toDrop.itemSprite;
        heldItem = toDrop;              // set the scriptable object to be the dropped item

        // set self as occupied
        occupied = true;

        // remove item from inventory
        EventHandler.Instance.DropItem();
        
    }
}
