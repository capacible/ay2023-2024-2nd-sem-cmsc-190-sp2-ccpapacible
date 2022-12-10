using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data container of the NPC to be attached into the NPC object
public class Speaker
{
    public string speakerId;                    // id related to the game object (from NPC data)
    public string speakerArchetype;             // archetype of speaker aka speaker tag
    public List<SpeakerGoal> speakerGoals;      // list of goals
    public int relWithPlayer;                   // value relationship with player
    public List<string> speakerMemories;
    public string speakerName;                  // display name of speaker

    public Speaker Clone()
    {
        Speaker newSpeaker = new Speaker
        {
            speakerId = speakerId,
            speakerArchetype = speakerArchetype,
            speakerGoals = speakerGoals,
            relWithPlayer = relWithPlayer,
            speakerMemories = speakerMemories,
            speakerName = speakerName
        };

        return newSpeaker;
    }
}

public class SpeakerGoal
{
    public string id;
    public List<string> leadingEvents;
    public bool isActive;
    public bool isAchieved;
    public bool isPaused;
}