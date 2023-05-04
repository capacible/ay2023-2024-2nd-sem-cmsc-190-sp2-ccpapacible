using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;

public class InventoryHandler : MonoBehaviour
{
    // list of all items
    private List<ItemBase> Inventory = new List<ItemBase>();
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

        EventHandler.MapSceneLoaded += Init;

        // subscribing -- interactions
        EventHandler.OnPickup += AddToInventory;
        EventHandler.OnInteractConclude += UnhideUi;
        EventHandler.Examine += HideUi;
        EventHandler.InGameMessage += HideUi;
        EventHandler.StartDialogue += HideUi;

        // items
        EventHandler.InteractionTriggered += UseHeldItem;
    }

    private void OnDestroy()
    {
        EventHandler.OnPickup -= AddToInventory;
        EventHandler.MapSceneLoaded -= Init;
        EventHandler.OnInteractConclude -= UnhideUi;
        EventHandler.Examine -= HideUi;
        EventHandler.InGameMessage -= HideUi;
        EventHandler.StartDialogue -= HideUi;
        // items
        EventHandler.InteractionTriggered -= UseHeldItem;
    }

    // whenever we load a map scene, we reinitialize the world camera of our inventory.
    private void Init(object[] param = null)
    {

        // find the camera and set the canvas camera to that.
        inventoryCanvas.worldCamera = Camera.main;
    }

    private void AddToInventory(string objId, ItemBase data)
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

    private void HideUi(object[] obj)
    {
        // get interaction ui type
        UiType t = (UiType)obj[0];

        if (t == UiType.IN_BACKGROUND)
        {
            return; // do nothing
        }
        inventoryCanvas.gameObject.SetActive(false);
    }

    private void UnhideUi()
    {
        // if the canvas is inactive, we reactive it
        // also only if the active uis are not empty.
        if (!inventoryCanvas.gameObject.activeInHierarchy)
        {
            inventoryCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Implements the InteractionTriggered delegate
    /// </summary>
    /// <param name="interactionParameters"></param>
    private void UseHeldItem(object[] interactionParameters)
    {
        // interaction parameters will have a 2nd parameter triggerItems[] array (implement in respective Interaction classes)
        ItemBase[] triggerItems = (ItemBase[])interactionParameters[1];

        if(heldItem>=0 && heldItem < Inventory.Count)
        {
            // get our current held item 
            var held = Inventory[heldItem];

            // we check if any of the interactions' trigger items correspond to our held item's id
            if (triggerItems.Contains(held))
            {
                // use held item
                held.UseItem();
            }
        }
    }
   

}
