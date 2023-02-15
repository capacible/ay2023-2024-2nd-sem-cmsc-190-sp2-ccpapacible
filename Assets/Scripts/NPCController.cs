using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls NPC movement and other parts
public class NPCController : MonoBehaviour
{
    // id represents the object id w/c we use to access speaker info sa manager and to distinguish the gameobjects frm each other.
    public string id = "";
    public NPCData npc;

    // we can add the npc itself into a list of NPCs in the eventhandler

    void Start()
    {
        // add npc to dialogue manager if the npc is filler
        if (npc.isFillerCharacter && npc.npcId == "")
        {
            if (id == "")
            {
                Debug.LogError("Id field is empty");
            }

            npc.npcId = id;
            EventHandler.Instance.AddNPCToManager(npc);
        }

        // set sprite to be the sprite set in NPC data.
        SpriteRenderer s = gameObject.GetComponent<SpriteRenderer>();
        s.sprite = npc.npcSprite;
    }
    
    [ContextMenu("Generate Id for NPC")]
    public void GenerateId()
    {
        id = npc.speakerArchetype + System.Guid.NewGuid().ToString();
    }
}
