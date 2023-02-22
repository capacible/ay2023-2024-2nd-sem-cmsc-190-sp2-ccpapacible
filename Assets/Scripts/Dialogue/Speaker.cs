using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/*
 * 
 * HOW THIS WILL WORK
 * (1) We convert the CSV of speakers to one XML
 *      (1.1) XML format is:
 *              <SpeakerCollection>
 *                  <Speakers>
 *                      <Speaker speakerArchetype="">
 *                          <displayName></displayname>
 *                          
 *                          <!-- the next parts are for the NPC data...>
 *                          
 *                          <isFillerCharacter> bool </isFillerCharacter>
 *                          <speakerTraits>
 *                              <Trait> string </Trait>
 *                          </>
 *                          
 *                      </Speaker>
 *                  </>
 *              </>
 * 
 */

// data container of the NPC to be attached into the NPC object
public class Speaker
{
    [XmlAttribute("speakerArchetype")]
    public string speakerArchetype;             // archetype of speaker aka speaker tag

    public string displayName;                  // display name of speaker
    
    /*
     * The following are not a part of the speaker sheet; this is updated during runtime
     */
    [XmlIgnore()]
    public List<string> speakerMemories = new List<string>();

    [XmlIgnore()]
    public int relWithPlayer;                   // value relationship with player

    [XmlIgnore()]
    public string speakerId;                    // id related to the game object.

    // because speaker traits can be randomized, this is not read during runtime. instead, we add this to the NPC data that
    // will be attached to the gameobject
    [XmlIgnore()]
    public List<string> speakerTraits = new List<string>();

    public Speaker Clone()
    {
        Speaker newSpeaker = new Speaker
        {
            speakerId = speakerId,
            speakerArchetype = speakerArchetype,
            relWithPlayer = relWithPlayer,
            speakerMemories = speakerMemories,
            displayName = displayName
        };

        return newSpeaker;
    }

    /// <summary>
    /// Given the traits list in NPC data, we pick some traits and set that for that NPC.
    /// </summary>
    /// <param name="npc"></param>
    public void SetTraits(NPCData npc)
    {

    }
}

[XmlRoot("SpeakerCollection")]
public class SpeakerCollection
{
    [XmlArray("Speakers"), XmlArrayItem("Speaker")]
    public List<Speaker> Speakers = new List<Speaker>();
}