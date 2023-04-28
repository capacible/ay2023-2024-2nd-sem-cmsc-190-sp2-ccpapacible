using System.Collections;
using System.Collections.Generic;
using System.IO;
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

public enum REL_STATUS
{
    BAD,
    NEUTRAL,
    GOOD,
    BAD_THRESH = -20,
    GOOD_THRESH = 20,
}
// data container of the NPC to be attached into the NPC object
public class Speaker
{
    public string speakerArchetype;             // archetype of speaker aka speaker tag

    public string displayName;                  // display name of speaker

    public bool isFillerCharacter;
    
    /*
     * The following are not a part of the speaker sheet; this is updated during runtime
     */
    [XmlIgnore]
    public List<string> speakerMemories = new List<string>();

    [XmlIgnore]
    public int relWithPlayer = 0;                    // value relationship with player

    [XmlIgnore]
    public string speakerId = "";                    // id related to the game object.

    // because speaker traits can be randomized, this is not read during runtime. instead, we add this to the NPC data that
    // will be attached to the gameobject
    [XmlIgnore]
    public List<int> speakerTraits = new List<int>();

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

    public void OverrideTraits(int numTraits, NPCData npc)
    {
        if (isFillerCharacter)
        {
            // randomize a set of traits THREE TIMES.
            for (int count = 0; count < numTraits; count++)
            {
                // get the speaker traits in
                // lookup id of traits from director.
                speakerTraits.Add( 
                    Director.NumKeyLookUp(npc.speakerTraits[Random.Range(0, npc.speakerTraits.Count)], fromTraits:true)
                );
            }
        }
        else
        {
            // set the defailt trauts
            foreach(string trait in npc.speakerTraits)
            {
                speakerTraits.Add(Director.NumKeyLookUp(trait, fromTraits:true));
            }
        }
    }

    public void OverrideDisplayName(string displayNameOverride)
    {
        if(displayNameOverride != "")
        {
            displayName = displayNameOverride;
        }
    }

    /// <summary>
    /// This returns a numerical version of good/bad/neutral based on thresholds
    /// </summary>
    /// <returns></returns>
    public int RelationshipStatus()
    {
        // good
        if(relWithPlayer >= (int)REL_STATUS.GOOD_THRESH)
        {
            return (int)REL_STATUS.GOOD;
        }
        else if(relWithPlayer <= (int) REL_STATUS.BAD_THRESH)
        {
            return (int)REL_STATUS.BAD;
        }

        return (int)REL_STATUS.NEUTRAL;
    }
}

[XmlRoot("SpeakerCollection")]
public class SpeakerCollection
{
    [XmlArray("Speakers"), XmlArrayItem("Speaker")]
    public Speaker[] Speakers;
}