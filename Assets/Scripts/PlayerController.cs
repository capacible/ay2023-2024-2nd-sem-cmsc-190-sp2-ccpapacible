using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls player actions including movement; attack; open/close of menu ??
public class PlayerController : MonoBehaviour
{
    public float speed;
    private bool busy = false;

    private Rigidbody2D body;
    private Vector2 movement;
    private SpriteRenderer r;

    // 
    [HideInInspector]
    public List<Collider2D> collided = new List<Collider2D>();

    // Start is called before the first frame update {
    void Start()
    {
        EventHandler.OnInteractConclude += EndInteraction;

        Init();
    }

    private void OnDisable()
    {
        EventHandler.OnInteractConclude -= EndInteraction;
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
        // our interaction will go kung ano yung last na nalapitan
        Collider2D coll = collided[collided.Count - 1];
        
        // check the type
        if (coll.gameObject.TryGetComponent<NPCController>(out var npc))
        {
            // if the game object is an NPC:
            // run eventhandler
            EventHandler.Instance.TriggerDialogue(npc.npc);
            busy = true;
        }
        

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
