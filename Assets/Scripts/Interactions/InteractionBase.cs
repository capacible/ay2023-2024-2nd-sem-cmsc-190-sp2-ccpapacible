using UnityEngine;

public class InteractionBase : MonoBehaviour
{
    public string objId;
    public string interactionMsgKey;        // this determines what interaction message we will show, if any
    public ItemBase[] useableItems;         // if there are items that will change state upon using an item, we put it here
    public bool interactable;               // basically if this interaction is active or will respond to the player

    [Header("Director-related")]
    public string[] addToGlobalEventList;
    public string[] addToCurrentMapEventList;

    private InteractPrompt prompt;          // prompt component

    // Start is called before the first frame update
    void Start()
    {
        InitializeInteraction();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>
    /// All common stuff for all interactions go here
    /// </summary>
    public void InitializeInteraction()
    {
        // we check if the obj exists in the scene
        if (!SceneUtility.ObjectIsInScene(objId))
        {
            Debug.Log("This object " + objId + " has been removed from the scene before you left.");
            Destroy(gameObject);
        }

        prompt = GetComponentInChildren<InteractPrompt>();

        // interactable checks
        if (!interactable)
        {
            // disable interact prompt if interaction not interactable.
            if (prompt != null)
            {
                Debug.Log("inactive prompt");
                prompt.gameObject.SetActive(false);
            }
            /*
            if(TryGetComponent(out SpriteRenderer s))
            {
                // set transparent.
                s.color = new Color(1, 1, 1, 0);
            }*/
        }
        
    }

    /// <summary>
    /// Common subscriptions
    /// </summary>
    public void Subscribe()
    {
        EventHandler.InteractionTriggered += HandleInteraction;
        EventHandler.SetStateOfObj += SetInteractableState;
    }

    /// <summary>
    /// Unsubscribe common subscriptions among all interactions
    /// </summary>
    public void Unsubscribe()
    {
        EventHandler.InteractionTriggered -= HandleInteraction;
        EventHandler.SetStateOfObj -= SetInteractableState;
    }

    /// <summary>
    /// Modifies the state of our interaction -- whether it is interactable or not
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    private void SetInteractableState(string id, bool state)
    {
        if (id == objId)
        {
            interactable = state;

            UpdateObject(state);
        }
    }

    /// <summary>
    /// Updates object appearance etc based on state
    /// </summary>
    /// <param name="state"></param>
    private void UpdateObject(bool state)
    {
        if (state == true)
        {

            // set our prompt to active
            if (prompt != null)
            {
                prompt.gameObject.SetActive(true);
            }
            // set our spriterenderer to max alpha/opacity
            if (TryGetComponent(out SpriteRenderer sRenderer))
            {
                sRenderer.color = new Color(1, 1, 1, 1);
            }

            return;
        }

        // set everything above to opposite// set our prompt to active
        if (prompt != null)
        {
            prompt.gameObject.SetActive(false);
        }
        /*
        // set our spriterenderer to max alpha/opacity
        if (TryGetComponent(out SpriteRenderer s))
        {
            s.color = new Color(1, 1, 1, 0);
        }*/
    }

    
    /// <summary>
    /// Method that will be overridden by other types of interaction
    /// </summary>
    /// <param name="interactParams"></param>
    public virtual void HandleInteraction(object[] interactParams)
    {
        // show interact message
        string id = interactParams[0].ToString();

        // if the id of the object the player interacted with is the same as the id of this interaction
        if(id == objId && interactable)
        {
            // some default task -- trigger an interaction message
            EventHandler.Instance.InteractMessage(interactionMsgKey, null);
        }
    }

    [ContextMenu("Generate basic interaction object id")]
    public virtual void GenerateId()
    {
        objId = "InteractionBase_";

        if (interactionMsgKey == "")
        {
            // generate based on name of object
            objId += gameObject.name + "_x";
            return;
        }
        // interaction msg key + scene name + guid
        objId += UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + interactionMsgKey + "_x";
    }
}
