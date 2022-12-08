using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls NPC movement and other parts
public class NPCController : MonoBehaviour
{
    // id represents the object id w/c we use to access speaker info sa manager and to distinguish the gameobjects frm each other.
    public string id = "";
    public Sprite npcPortrait;
    public NPCData npc;

    private void Start()
    {
        // add npc to dialogue manager if the npc is filler
        if (npc.isFillerCharacter && npc.npcId == "")
        {
            npc.GenerateId();
            EventHandler.current.AddNPCToManager(npc);
        }
    }
}
