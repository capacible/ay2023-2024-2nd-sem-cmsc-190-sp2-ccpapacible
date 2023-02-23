using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// contains object data that will be accessed during runtime in-world
// this will be used to wrap the necessary info and pass it along to the different parts of the dialogue
// attribs to keep:
//  - npcId
//  - speaker archetype
//  - npcportrait
//  - display name
// this will no longer be a scriptable object afterwards.
[CreateAssetMenu(fileName ="NPC_type_archetype", menuName ="Create a new NPC")]
public class NPCData : ScriptableObject
{
    /* 
     * INTERNAL STUFF
     */

    // acquired from CSV file...
    public string speakerArchetype;     // the archetype that determines which lines to consider for this NPC object
    public List<string> speakerTraits = new List<string>();
}
