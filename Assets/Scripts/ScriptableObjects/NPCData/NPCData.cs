using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPCData automatically gets generated from CSV file. This is a data container that will be attached into the 
/// NPC object. When an NPC gets created, if the NPC id pre-set in the game is NOT in the Director tracker,
/// NPCData gets passed into the director via eventhandler in order to create a new "Speaker" that will then
/// be used to keep track of this specific NPC
/// </summary>
[CreateAssetMenu(fileName ="NPC_type_archetype", menuName ="Create a new NPC")]
public class NPCData : ScriptableObject
{
    /* 
     * INTERNAL STUFF
     */

    // acquired from CSV file...
    public string speakerArchetype;     // the archetype that determines which lines to consider for this NPC object

    // a list of speaker traits loaded from csv file. if filler character, these traits are randomized
    // for more variety between each individual filler character instance
    public List<string> speakerTraits = new List<string>();
    // this will be a list of portraits eventually -- for different facesets
    public Sprite speakerPortraits;
    // for changing the sprite into the actual.
    public Sprite speakerSprite;
}
