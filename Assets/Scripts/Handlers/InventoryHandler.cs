using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    // here we will pull actual data on the item when viewed or selected or held and so on.
    private Dictionary<string, ItemData> Inventory = new Dictionary<string, ItemData>();

    // list of all object ids (unique items) in order , we use this to scroll through all items we have in proper order
    private List<string> itemList = new List<string>();
    private int currentHeldItem = 0;

    // UI stuff here; click drag the ui stuff into these fields
    // button (2) -> arrows
    // image field (for icon)
    // window or image field (for whole container
    // text field (for name of item (display name))
    // text field for description of item

    // held item id
    // in the ui, we get the ItemData that corresponds to the heldItem (object id), access the inventory icon and then draw it
    private string heldItem = "";

    private void Start()
    {
        // dontdestroy?
        

        // subscribing
        EventHandler.OnPickup += AddToInventory;
    }

    private void AddToInventory(string objId, ItemData data)
    {
        // no limit to inventory sort of
        Inventory.Add(objId, data);

        // add to list of ids
        itemList.Add(objId);

        // if we don't have a held item, we set this to be the held item
        if (heldItem == "")
        {
            heldItem = objId;

            // update ui
            RefreshUi();
        }
    }
    
    // on click event listener thingy
    // scroll to next held item; if our length of items is less than max
    // if more than max, we move to the start or 1st item
    public void NextItem()
    {
        if(currentHeldItem + 1 < itemList.Count)
        {
            currentHeldItem++;            
        }
        else
        {
            // if max na;
            currentHeldItem = 0; // set to 0 (starting index)
        }

        // access the new held item and overwrite
        heldItem = itemList[currentHeldItem];

        // updating ui
        RefreshUi();
    }

    // on click --> prev item
    public void PrevItem()
    {
        // if our current held item is NOT 0 (aka not the first...)
        if (currentHeldItem > 0)
        {
            // decrement to go to previous index
            currentHeldItem--;
        }
        else
        {
            // if min na, just get the max index of the itemList
            currentHeldItem = itemList.Count - 1;
        }

        // access the new held item and overwrite
        heldItem = itemList[currentHeldItem];

        // updating ui
        RefreshUi();
    }

    // refreshes ui -- changes the ui/images/text based on the changes in the held item.
    private void RefreshUi()
    {

    }

}
