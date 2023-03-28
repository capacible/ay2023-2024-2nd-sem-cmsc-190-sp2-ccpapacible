using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls overworld player movement; attached to every single instance of player in separate scenes
/// This is separate from the script that will handle inventory/held item/whatever else
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float speed;
    private bool busy = false;

    private Rigidbody2D body;
    private Vector2 movement;
    private SpriteRenderer r;

    // 
    [HideInInspector]
    public List<GameObject> collided = new List<GameObject>();

    // Start is called before the first frame update {
    void Start()
    {
        EventHandler.OnInteractConclude += EndInteraction;
        EventHandler.OnCollision += AddCollision;
        EventHandler.OnNotCollision += RemoveCollision;
        
        Init();
    }

    private void OnDisable()
    {
        EventHandler.OnInteractConclude -= EndInteraction;
        EventHandler.OnCollision -= AddCollision;
        EventHandler.OnNotCollision -= RemoveCollision;
    }

    private void Init()
    {
        // rigidbody
        body = GetComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezeRotation; // freeze the rotation so di magrorotate si player

        // get the renderer component
        r = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (!busy)
        {
            // movement
            movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            PlayerMove(movement);
            
            PlayerAction();

        }
    }
    
    // temporary function to track the buttons player has pressed.
    private void PlayerAction()
    {
        // player presses space:
        // check all bounds that collide w player
        // then run spacekey delegate, leading to PickedUp() funct

        if (Input.GetKeyDown(KeyCode.Space) && !busy)
        {
            Debug.Log("space key");
            // talk
            Interact();
        }
    }

    private void Interact()
    {
        // only do things if we are colliding w smth
        if(collided.Count > 0)
        {
            // our interaction will go kung ano yung last na nalapitan
            GameObject coll = collided[collided.Count - 1];

            // check the type
            if (coll.TryGetComponent<NPCController>(out var npc))
            {
                // if the game object is an NPC:
                // run eventhandler
                EventHandler.Instance.TriggerDialogue(npc.objId, npc.npc.speakerPortraits);
                busy = true;
            }
            if (coll.TryGetComponent<Item>(out var item))
            {
                // if our collided object is an item, then we pickup
                EventHandler.Instance.PickupItem(item.objId, item.data);
            }
        }
        
    }

    /// <summary>
    /// Adds the game object that the player collides with to the list of objects that the player is currently colliding with
    /// </summary>
    /// <param name="obj"></param>
    private void AddCollision(GameObject obj)
    {
        collided.Add(obj);
    }

    private void RemoveCollision(GameObject obj)
    {
        collided.Remove(obj);
    }

    private void EndInteraction()
    {
        busy = false;
    }

    private void PlayerMove(Vector2 dir)
    {
        body.MovePosition(new Vector2(transform.position.x + dir.x * speed, transform.position.y + dir.y * speed));
        //transform.Translate(dir * speed * Time.deltaTime);
    }
    
}
