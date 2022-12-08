using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data container of the NPC to be attached into the NPC object
public class Speaker
{
    public string speakerId;
    public string speakerArchetype;
    public List<SpeakerGoal> speakerGoals;
    public int relWithPlayer;
    public List<string> speakerMemories;
    public string speakerName;

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