using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum WALK_DIR
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

/// <summary>
/// Controls overworld player movement; attached to every single instance of player in separate scenes
/// This is separate from the script that will handle inventory/held item/whatever else
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float speed;
    private bool busy = true;
    private static readonly KeyCode[] movements = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

    private Rigidbody2D body;
    private Vector2 movement;
    private SpriteRenderer r;
    public Animator playerAnimator;
    
    // 
    [HideInInspector]
    public List<InteractionBase> collided = new List<InteractionBase>();
    

    // Start is called before the first frame update {
    void Start()
    {
        Debug.Log("Loaded the player object");
        EventHandler.InGameMessage += InteractionBegin;
        EventHandler.StartDialogue += InteractionBegin;
        EventHandler.Examine += InteractionBegin;
        EventHandler.OnInteractConclude += EndInteraction;

        EventHandler.OnPlayerCollision += AddCollision;
        EventHandler.OnPlayerNotCollision += RemoveCollision;
        
        Init();
    }

    private void OnDestroy()
    {
        EventHandler.InGameMessage -= InteractionBegin;
        EventHandler.StartDialogue -= InteractionBegin;
        EventHandler.Examine -= InteractionBegin;
        EventHandler.OnInteractConclude -= EndInteraction;
        EventHandler.OnPlayerCollision -= AddCollision;
        EventHandler.OnPlayerNotCollision -= RemoveCollision;
    }

    private void Init()
    {
        // rigidbody
        body = GetComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezeRotation; // freeze the rotation so di magrorotate si player

        // get the renderer component
        r = GetComponent<SpriteRenderer>();
        
        // set the position if a specific position exists
        if(SceneUtility.destPosition != null)
        {
            Debug.Log("changing position of player");

            // size of player
            Vector3 size = r.bounds.size;
            // the destination and the offset
            Vector3 dest = (Vector3)SceneUtility.destPosition;
            Vector3 offset = new Vector3(size.x * SceneUtility.playerDirOffset.x * 2, size.y * SceneUtility.playerDirOffset.y * 2, size.z * SceneUtility.playerDirOffset.z * 2);

            // we multiply the playerDiroffset with the bounds of player, then add it to the destination.
            gameObject.transform.localPosition = dest + offset;
        }

        busy = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!busy)
        {
            // movement
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            playerAnimator.SetFloat("horizontal", movement.x);
            playerAnimator.SetFloat("vertical", movement.y);
            playerAnimator.SetFloat("speed", movement.magnitude);

            PlayerMove();
            
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
            InteractionBase interaction = collided[collided.Count - 1];
            
            // we get any interaction component here.
            //if(coll.TryGetComponent<InteractionBase>(out var interaction))
            //{
                // player interacts with the object / get the object id of that interaction so that the right object will do stuff
                // and other processes will do important stuff
                EventHandler.Instance.PlayerInteractWith(new object[] { interaction.objId, interaction.useableItems });
            //}
        }
        
    }

    /// <summary>
    /// Adds the game object that the player collides with to the list of objects that the player is currently colliding with
    /// </summary>
    /// <param name="obj"></param>
    private void AddCollision(InteractionBase obj)
    {
        collided.Add(obj);
    }

    private void RemoveCollision(InteractionBase obj)
    {
        collided.Remove(obj);
    }
    
    private void InteractionBegin(object[] obj)
    {
        // get interaction ui type
        UiType t = (UiType)obj[0];

        // quick message doesn't need you to be busy.
        if (t == UiType.IN_BACKGROUND)
        {
            return; // do nothing
        }

        //otherwise we set busy
        busy = true;
    }

    private void EndInteraction()
    {
        if(EventHandler.activeUi.Count == 0)
        {
            busy = false;
        }
    }

    private void PlayerMove()
    {


        body.MovePosition(body.position + movement * speed * Time.fixedDeltaTime);
    }

    
    
}
