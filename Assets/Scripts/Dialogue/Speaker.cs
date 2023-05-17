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

// NUMERICAL VALUE OF RELATIONSHIP STATUS
public enum REL_STATUS_NUMS
{
    BAD,        // 0
    NEUTRAL,    // 1
    GOOD,       // 2
    NONE = -1,
    BAD_THRESH = -20,
    GOOD_THRESH = 20,
}

// STRING VERSION OF RELATIONSHIP STATUS (equivalent to rel_status_nums above)
public static class REL_STATUS_STRING
{
    public static readonly string GOOD = "good";
    public static readonly string NEUTRAL = "neut";
    public static readonly string BAD = "bad";
    public static readonly string NONE = "none";    // no requirement.
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
    public int relWithPlayer = 0;                    // value relationship with player -- not the numerical rep of gud/bad/neut

    [XmlIgnore]
    public string speakerId = "";                    // id related to the game object.

    // because speaker traits can be randomized, this is not read during runtime. instead, we add this to the NPC data that
    // will be attached to the gameobject
    [XmlIgnore]
    public int speakerTrait;

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
            // randomize a trait.
            // get the speaker traits in
            // lookup id of traits from director.
            speakerTrait = Director.NumKeyLookUp(npc.speakerTraits[Random.Range(0, npc.speakerTraits.Count)], fromTraits: true);

        }
        else
        {
            speakerTrait = -1;
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
        if(relWithPlayer >= (int)REL_STATUS_NUMS.GOOD_THRESH)
        {
            return (int)REL_STATUS_NUMS.GOOD;
        }
        else if(relWithPlayer <= (int) REL_STATUS_NUMS.BAD_THRESH)
        {
            return (int)REL_STATUS_NUMS.BAD;
        }

        return (int)REL_STATUS_NUMS.NEUTRAL;
    }
}

[XmlRoot("SpeakerCollection")]
public class SpeakerCollection
{
    [XmlArray("Speakers"), XmlArrayItem("Speaker")]
    public Speaker[] Speakers;
}