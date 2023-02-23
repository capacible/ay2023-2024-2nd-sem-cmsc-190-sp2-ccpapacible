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
    
    // list of all evvents that all characters remember
    private static List<string> globalEvents = new List<string>();
    // dicct of map events that characters from that map rembr
    private static Dictionary<string, List<string>> mapEvents = new Dictionary<string, List<string>>();
    // each unique speaker in the world
    private static Dictionary<string, Speaker> allSpeakers = new Dictionary<string, Speaker>();
    // list of topics
    private static Dictionary<string, float> topicList = new Dictionary<string, float>();

    // defaults of each filler archetype to clone
    private static Dictionary<string, Speaker> speakerDefaults = new Dictionary<string, Speaker>();
    
    private static DialogueLine prevLine;                           // remembering the previous line

    // public static Inference engine

    public static void Start()
    {
        LoadLines();
        LoadSpeakers();
    }

    /// <summary>
    /// Loads all the lines from XML files upon startup
    /// </summary>
    public static void LoadLines()
    {
        // we can have a text file here describing the file names of all dialogue XMLs.
        /*
        lineDB = DialogueLineCollection.LoadAll(new string[] {
            "Data/XML/dialogue/dialoguePlayer.xml"
        });*/
    }

    /// <summary>
    /// Loading all speakers on startup.
    /// Filler characters will be loaded into defaults, while main characters will be loaded straight
    /// into the allSpeakers list.
    /// </summary>
    public static void LoadSpeakers()
    {
        // create a new speakercollection
        SpeakerCollection loadSpeakers = SpeakerCollection.LoadCollection("Data/XML/Speakers.xml");

        Debug.Log(loadSpeakers.Speakers.Length);
        TestPrintSpeakers(loadSpeakers);

        // we add each speaker into the defaults dictionary
        foreach(Speaker s in loadSpeakers.Speakers)
        {
            speakerDefaults.Add(s.speakerArchetype, s);
            Debug.Log($"adding speaker {s} with archetype {s.speakerArchetype}");
        }

        // add the player
        allSpeakers.Add("player", speakerDefaults["player"].Clone());
        
    }

    public static bool SpeakerExists(string npcObjId)
    {
        if (allSpeakers.ContainsKey(npcObjId))
        {
            return true;
        }

        return false;
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
    /// Add speaker to the speaker trackers
    /// </summary>
    public static void AddNewSpeaker(NPCData npc, string npcObjId, string displayName)
    {
        // clone the default of that speaker archetype and add to actual speaker tracker
        allSpeakers.Add(npcObjId, speakerDefaults[npc.speakerArchetype].Clone());

        // set the speaker id to be the object id.
        allSpeakers[npcObjId].speakerId = npcObjId;

        // if filler speaker, we will randomize a set of 3 traits
        allSpeakers[npcObjId].OverrideTraits(3, npc);
        allSpeakers[npcObjId].OverrideDisplayName(displayName);

        // if the given display name is not empty, then we will override the speaker's display name with what is given
        if(displayName != "")
        {
            allSpeakers[npcObjId].displayName = displayName;
        }

        Debug.Log($"Added speaker with id {npcObjId}");
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

    /// <summary>
    /// For testing if speakers are loaded successfully
    /// </summary>
    public static void TestPrintSpeakers(SpeakerCollection speakers)
    {
        foreach(Speaker s in speakers.Speakers)
        {
            Debug.Log($"Successfully loaded {s.speakerArchetype} details are:");
            Debug.Log($"displayName: {s.displayName}");
            Debug.Log($"filler: {s.isFillerCharacter}");
            Debug.Log("===============");
        }
    }
}
