using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;

public class InventoryHandler : MonoBehaviour
{
    // list of all items
    private static List<ItemBase> Inventory = new List<ItemBase>();
    // index of our held item
    private static int heldItem = -1;

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
        EventHandler.TriggerOnDrop += DropItem;
    }

    private void OnDestroy()
    {
        EventHandler.OnPickup -= AddToInventory;
        EventHandler.ItemEffect -= RemoveFromInventory;
        EventHandler.MapSceneLoaded -= Init;
        EventHandler.OnInteractConclude -= UnhideUi;
        EventHandler.Examine -= HideUi;
        EventHandler.InGameMessage -= HideUi;
        EventHandler.StartDialogue -= HideUi;
        // items
        EventHandler.InteractionTriggered -= UseHeldItem;
        EventHandler.TriggerOnDrop -= DropItem;
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
   
    private void RemoveFromInventory(object[] parameters)
    {
        // removes the currently-held item if it's discardable
        if(( (ItemBase)parameters[1] ).discardAfter)
        {
            // remove
            Inventory.RemoveAt(heldItem);

            // change idx if held item is greater than the length of inventory
            if (heldItem > Inventory.Count)
                heldItem = 0;   // change held item to first item
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
        Debug.Log("We are at held item number: " + heldItem);

        if (heldItem != -1)
        {
            // changing the item name
            itemName.text = Inventory[heldItem].itemName;
            // changing the item sprite
            iconImg.sprite = Inventory[heldItem].itemSprite;
        }
        else
        {
            itemName.text = "";
            iconImg.sprite = null;
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
        if (!inventoryCanvas.gameObject.activeInHierarchy && EventHandler.activeUi.Count == 0)
        {
            inventoryCanvas.gameObject.SetActive(true);
            RefreshUi();
        }
    }

    public static ItemBase CurrentItem()
    {
        if(heldItem == -1)
        {
            return null;
        }
        return Inventory[heldItem];
    }
    
    /// <summary>
    /// Implements the InteractionTriggered delegate
    /// </summary>
    /// <param name="interactionParameters">
    ///     [0] - object id
    ///     [1] - useable items
    /// </param>
    private void UseHeldItem(object[] interactionParameters)
    {
        // interaction parameters will have a 2nd parameter triggerItems[] array (implement in respective Interaction classes)
        ItemBase[] triggerItems = (ItemBase[])interactionParameters[1];

        Debug.Log("Currently held item: " + heldItem);

        if(heldItem>=0 && heldItem < Inventory.Count)
        {
            // get our current held item 
            var held = Inventory[heldItem];

            // we check if any of the interactions' trigger items correspond to our held item's id
            if (triggerItems.Contains(held))
            {
                Debug.Log("Using item with id: " + held.itemId);

                // use held item
                held.UseItem(interactionParameters[0].ToString());
            }
        }
    }

    // drops the currently-held item
    private void DropItem()
    {
        EventHandler.Instance.QuickNotification(GENERIC_MSG_ID.ITEM_DROP,
            new Dictionary<string, string> { { MSG_TAG_TYPE.ITEM_NAME, Inventory[heldItem].itemName }
        });

        Inventory.RemoveAt(heldItem);

        // if held item is greater than length:
        if(heldItem >= Inventory.Count)
        {
            // modify to be inventory.count-1
            heldItem = Inventory.Count - 1;
        }

        RefreshUi();
    }
   

}
