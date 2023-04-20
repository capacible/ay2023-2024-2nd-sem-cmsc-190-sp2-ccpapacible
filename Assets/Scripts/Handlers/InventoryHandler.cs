using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class InventoryHandler : MonoBehaviour
{
    // list of all object ids (unique items) in order , we use this to scroll through all items we have in proper order
    private List<ItemData> Inventory = new List<ItemData>();
    // index of our held item
    private int heldItem = -1;

    // UI stuff here; click drag the ui stuff into these fields
    public Canvas inventoryCanvas;
    public Image iconContainer;
    public Image iconImg;
    public Button next;
    public Button prev;
    public TextMeshProUGUI itemName;

    private void Awake()
    {
        Debug.Log("Instantiated invenotry");
    }

    private void Start()
    {
        // subscribing
        EventHandler.OnPickup += AddToInventory;
        EventHandler.MapSceneLoaded += Init;
    }

    private void OnDestroy()
    {
        EventHandler.OnPickup -= AddToInventory;
    }

    // whenever we load a map scene, we reinitialize the world camera of our inventory.
    private void Init(object[] param = null)
    {

        // find the camera and set the canvas camera to that.
        inventoryCanvas.worldCamera = Camera.main;
    }

    private void AddToInventory(string objId, ItemData data)
    {

        // add to list of item data.
        Inventory.Add(data);

        // if we don't have a held item, we set this currently added thing to be our held item
        if (heldItem == -1)
        {
            heldItem = Inventory.Count - 1;

            // update ui
            RefreshUi();
        }
    }
    
    // on click event listener thingy
    // scroll to next held item; if our length of items is less than max
    // if more than max, we move to the start or 1st item
    public void NextItem()
    {
        // if no items
        if(Inventory.Count == 0)
        {
            return;
        }

        Debug.Log("Moving to next item " + heldItem);
        if(heldItem + 1 < Inventory.Count)
        {
            heldItem++;            
        }
        else
        {
            // if max na;
            heldItem = 0; // set to 0 (starting index)
        }

        // updating ui
        RefreshUi();
    }

    // on click --> prev item
    public void PrevItem()
    {
        // if no items
        if (Inventory.Count == 0)
        {
            return;
        }

        // if our current held item is NOT 0 (aka not the first...)
        if (heldItem > 0)
        {
            // decrement to go to previous index
            heldItem--;
        }
        else
        {
            // if min na, just get the max index of the itemList
            heldItem = Inventory.Count - 1;
        }

        // updating ui
        RefreshUi();
    }

    /// <summary>
    /// Updates the UI based on the held item.
    /// </summary>
    private void RefreshUi()
    {
        if (heldItem != -1)
        {
            // changing the item name
            itemName.text = Inventory[heldItem].itemName;
            // changing the item sprite
            iconImg.sprite = Inventory[heldItem].itemSprite;
        }
        else
        {
            // error
            Debug.LogError("Invalid item index.");
        }
    }
   

}
