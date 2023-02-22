using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// In charge of determining the lines to use.
/// </summary>
public static class Director
{
    private static DialogueLineCollection lineDB;

    // tracking events and speakers.
    public static string activeNPC;                                 // the current NPC speaking
    private static string currentMap;                               // current location

    private static List<string> globalEvents;                       // list of all evvents that all characters remember
    private static Dictionary<string, List<string>> mapEvents;      // dicct of map events that characters from that map rembr
    private static Dictionary<string, Speaker> allSpeakers;         // all unique speakers
    private static Dictionary<string, float> topicList;       // a list of topics, SORTED BY HIGHEST RELEVANCE

    private static Dictionary<string, Speaker> fillerCharDefault;   // defaults of each filler archetype to clone
    private static DialogueLine prevLine;                           // remembering the previous line

    // public static Inference engine

    /// <summary>
    /// Loads all the lines from XML file upon startup
    /// </summary>
    public static void LoadLines()
    {

    }

    // if non-filler, create a new speaker class and add to all speakers
    // else, create a new speaker and add to fillerCharDefault.
    public static void LoadSpeakers()
    {
        // iterate through each speaker in database
    }

    /// <summary>
    ///  just gives other programs access to the display name of a given npc id/object id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string ActiveNPCDisplayName()
    {
        //return allSpeakers[activeNPC].speakerName;
        return "testName";
    }

    /// <summary>
    /// This adds a filler speaker of a given archetype if it is not yet in the speakers.
    /// </summary>
    public static void NewFillerSpeaker(NPCData npc)
    {
        // if the speaker dict currently doesnt have this npc.
        if (!allSpeakers.ContainsKey(npc.npcId))
        {
            // clone the default speaker of the filler character archetype of the npc and add it to allspeakers
            allSpeakers[npc.npcId] = fillerCharDefault[npc.speakerArchetype].Clone(); // deepcopies the speaker
            allSpeakers[npc.npcId].speakerId = npc.npcId;
        }
    }

    /// <summary>
    /// This updates the map data as well as the active NPC then requests a starting line
    /// </summary>
    public static DialogueLine StartAndGetLine(string npcId, string mapId)
    {
        activeNPC = npcId;
        currentMap = mapId;
        
        return GetNPCLine();
    }

    /// <summary>
    /// Uses the inference engine to infer the best possible line
    /// </summary>
    /// <param name="playerLine">The lline chosen by player</param>
    /// <returns>DialogueLine selected by the engine</returns>
    public static DialogueLine GetNPCLine(DialogueLine playerLine=null)
    {
        if(playerLine != null)
        {
            // update data of NPC
            //UpdateNPCData(playerLine);
        }

        // proceed to get the npc line.
        DialogueLine prevLine = new DialogueLine();
        prevLine.dialogue = "Hello there.";

        return prevLine;
    }

    /// <summary>
    /// Selects a maximum of 3 lines from the database.
    /// </summary>
    /// <param name="globalEvents"></param>
    /// <param name="mapEvents"></param>
    /// <param name="activeNPC"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<DialogueLine> GetPlayerLines()
    {
        // updates player data given the acquired line.
        //UpdatePlayerData(prevLine);

        List<DialogueLine> acquiredLines = new List<DialogueLine>();

        acquiredLines.Add(new DialogueLine() { dialogue = "Ahoy" });

        acquiredLines.Add(new DialogueLine() { dialogue = "Ahoy 2" });

        acquiredLines.Add(new DialogueLine() { dialogue = "Ahoy 3" });

        return acquiredLines;
    }


    // update npc-specific data
    public static void UpdateNPCData(DialogueLine line)
    {
       
        foreach (string m in line.effect.addToNPCMemory)
        {
            if (!allSpeakers[activeNPC].speakerMemories.Contains(m))
            {
                allSpeakers[activeNPC].speakerMemories.Add(m);
            }
        }

        UpdateSpeakerData(line);
    }

    // update player specific data
    public static void UpdatePlayerData(DialogueLine line)
    {

        foreach (string m in line.effect.addToPlayerMemory)
        {
            if (!allSpeakers["player"].speakerMemories.Contains(m))
            {
                allSpeakers["player"].speakerMemories.Add(m);
            }
        }

        UpdateSpeakerData(line);
    }

    public static void UpdateSpeakerData(DialogueLine line)
    {
        // access reationship with active npc and update it.
        allSpeakers[activeNPC].relWithPlayer += line.effect.relationshipEffect;

        // update topic relevance table
        // set topic relevance to be the maximum.
        if(line.effect.makeMostRelevantTopic != "")
        {
            topicList[line.effect.makeMostRelevantTopic] = 2;
        }

        if (line.effect.closeTopic != "")
        {
            // set teh topic listed to 1 (default value)
            topicList[line.effect.closeTopic] = 1;
        }

        // add to global events
        foreach (string e in line.effect.addEventToGlobal)
        {
            // only add if the global events are in the list
            if (!globalEvents.Contains(e))
            {
                globalEvents.Add(e);
            }
        }

        // add to map events
        foreach (string eventId in line.effect.addEventToMap)
        {
            if (!mapEvents[currentMap].Contains(eventId))
            {
                mapEvents[currentMap].Add(eventId);
            }
        }
        
    }
}
