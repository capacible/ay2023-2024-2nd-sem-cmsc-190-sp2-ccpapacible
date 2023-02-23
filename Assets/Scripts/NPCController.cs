using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls NPC movement and other parts
// NPCs will be changed into prefabs once database reading at start of game is implemented.
public class NPCController : MonoBehaviour
{
    // id represents the object id w/c we use to access speaker info sa manager and to distinguish the gameobjects frm each other.
    public string id;
    public NPCData npc;                 // holds archetype id

    // BELOW ARE ATTRIBUTES THAT WILL BE USED WHEN THE READING DATABASE @ GAME START IS IMPLEMENTED
    public Sprite dialoguePortrait;     // default dialogue portrait
    public string npcDisplayName;       // display name

    void Start()
    {
        if(id == "")
        {
            Debug.LogError("Id field is empty. Please generate an identifier for this NPC object");
        }
        else
        {
            // upon starting, we check if this NPC object is already in the allspeaker dictionary
            if (!Director.SpeakerExists(id))
            {
                // we add the npc into the allSpeakers list if it doesnt exist yet
                Director.AddNewSpeaker(npc, id, npcDisplayName);
            }
        }
    }
    
    [ContextMenu("Generate Id for NPC")]
    public void GenerateId()
    {
        id = npc.speakerArchetype + System.Guid.NewGuid().ToString();
    }
}
