using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls NPC movement and other parts
public class NPCController : MonoBehaviour
{
    // id represents the object id w/c we use to access speaker info sa manager and to distinguish the gameobjects frm each other.
    [HideInInspector]
    public string id = ""; // might have to be removed since npc id is now in npc data
    public NPCData npc;

    void Start()
    {
        // add npc to dialogue manager if the npc is filler
        if (npc.isFillerCharacter && npc.npcId == "")
        {
            npc.GenerateId();
            EventHandler.current.AddNPCToManager(npc);
        }

        // set sprite to be the sprite set in NPC data.
        SpriteRenderer s = gameObject.GetComponent<SpriteRenderer>();
        s.sprite = npc.npcSprite;
    }
}
