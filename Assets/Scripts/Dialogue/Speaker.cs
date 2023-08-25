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

    [XmlIgnore]
    public Dictionary<string, double> topics = new Dictionary<string, double>();

    [XmlIgnore]
    public List<int> queriedMemories = new List<int>();

    [XmlIgnore]
    public int currentRelStatus;
    
    public void InitializeTopics(IdCollection topicColl, double initialVal)
    {
        foreach(string topic in topicColl.allIds)
        {
            topics.Add(topic, initialVal);
        }
    }

    public void PrioritizeTopics(params string[] topicarr)
    {
        foreach(string topic in topicarr)
            topics[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_HIGH;
    }

    public Speaker Clone()
    {
        Speaker newSpeaker = new Speaker
        {
            speakerId = speakerId,
            speakerArchetype = speakerArchetype,
            relWithPlayer = relWithPlayer,
            speakerMemories = speakerMemories,
            displayName = displayName,
            isFillerCharacter = isFillerCharacter,
            topics = topics
        };

        newSpeaker.currentRelStatus = newSpeaker.RelationshipStatus();

        return newSpeaker;
    }

    public void OverrideTraits(NPCData npc)
    {
        Debug.Log("overriding traits... of character "+speakerArchetype+" is filler? "+isFillerCharacter);
        if (isFillerCharacter)
        {
            // randomize a trait.
            // get the speaker traits in
            // lookup id of traits from director.
            // exclude last index which is the NONE str
            speakerTrait = Director.NumKeyLookUp(npc.speakerTraits[Random.Range(0, npc.speakerTraits.Count - 1)], fromTraits: true);
            Debug.Log("npc: " + speakerArchetype + " has the trait id of " + speakerTrait);
        }
        else
        {
            speakerTrait = Director.NumKeyLookUp(DirectorConstants.NONE_STR, fromTraits:true);
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
        if(relWithPlayer >= (int)DirectorConstants.REL_STATUS_NUMS.GOOD_THRESH)
        {
            return (int)DirectorConstants.REL_STATUS_NUMS.GOOD;
        }
        else if(relWithPlayer <= (int) DirectorConstants.REL_STATUS_NUMS.BAD_THRESH)
        {
            return (int)DirectorConstants.REL_STATUS_NUMS.BAD;
        }

        return (int)DirectorConstants.REL_STATUS_NUMS.NEUTRAL;
    }
}

[XmlRoot("SpeakerCollection")]
public class SpeakerCollection
{
    [XmlArray("Speakers"), XmlArrayItem("Speaker")]
    public Speaker[] Speakers;
}