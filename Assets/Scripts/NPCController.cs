using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls NPC movement and other parts
// NPCs will be changed into prefabs once database reading at start of game is implemented.
public class NPCController : MonoBehaviour
{
    // ALL NPCS MUST HAVE A UNIQUE ID
    // id represents the object id w/c we use to access speaker info sa manager and to distinguish the gameobjects frm each other.
    public string objId;
    public NPCData npc;                 // holds archetype id

    // display name override
    public string npcDisplayName;       // display name

    void Start()
    {
        if(objId == "")
        {
            Debug.LogError("Id field is empty. Please generate an identifier for this NPC object");
        }
        else
        {
            // upon starting, we check if this NPC object is already in the allspeaker dictionary; ensure that we don't
            // add non-directed npcs here
            if (npc.usesDirector && !Director.SpeakerExists(objId))
            {
                // we add the npc into the allSpeakers list if it doesnt exist yet
                Director.AddNewSpeaker(npc, objId, npcDisplayName);
            }
        }

        SetSprite();
    }

    /// <summary>
    /// Gets the spriterenderer component of the gameobject of this script and modifies the default sprite to be
    /// the actual sprite.
    /// </summary>
    private void SetSprite()
    {

        if (gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer s))
        {
            if (npc.worldSprite == null)
            {
                Debug.LogWarning("speakerSprite for " + gameObject.name + " does not exist.");
            }
            else
            {
                // change sprite from npc default to what's attached to the NPC so
                s.sprite = npc.worldSprite;
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found.");
        }
    }
    
    /// <summary>
    /// Generates an id for the NPC object exactly once.
    /// </summary>
    [ContextMenu("Generate Id for NPC")]
    public void GenerateId()
    {
        if (npc != null)
        {
            if (!npc.usesDirector)
            {
                // attach display name
                objId = npcDisplayName + "_" + System.Guid.NewGuid().ToString();
                return;
            }

            // we have a speaker archetype
            objId = npc.speakerArchetype + System.Guid.NewGuid().ToString();
            return;
        }

        Debug.LogError("No NPC data attached to this controller yet.");
    }
}
