using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum MoodThreshold
{
    GOOD = 1,
    BAD = -1
}

/// <summary>
/// In charge of determining the lines to use.
/// </summary>
public static class Director
{
    public const string EVENTS_XML_PATH = "Data/XML/DB/allEvents.xml";
    public const string TOPICS_XML_PATH = "Data/XML/DB/allTopics.xml";
    public const string TRAITS_XML_PATH = "Data/XML/DB/allTraits.xml";
    public const string SPEAKERS_XML_PATH = "Data/XML/Speakers.xml";

    // director is active? -- currently being used?
    public static bool isActive;

    // tracking events and speakers.
    private static string activeNPC;                                 // the current NPC speaking
    private static string currentMap;                               // current location

    /*
     * TRACKING STUFF; gets edited
     */
    // list of all evvents that all characters remember -- BY INTEGER ID
    private static List<int> globalEvents = new List<int>();
    // dicct of map events that characters from that map rembr
    private static Dictionary<string, List<int>> mapEvents = new Dictionary<string, List<int>>();
    // each unique speaker in the world
    private static Dictionary<string, Speaker> allSpeakers = new Dictionary<string, Speaker>();
    // list of topics
    private static Dictionary<string, float> topicList = new Dictionary<string, float>();
    // defaults of each filler archetype to clone
    private static Dictionary<string, Speaker> speakerDefaults = new Dictionary<string, Speaker>();
    
    // remember the previous line said by NPC
    private static DialogueLine prevLine;
    private static List<DialogueLine> playerChoices = new List<DialogueLine>();  // max of 3 choices.

    // remembering the tone or mood of current convo
    private static int mood;

    /*
     * STUFF LOADED AT THE START OF GAME
     */
     
    public static Dictionary<int, DialogueLine> LineDB { get; private set; }    // dialogue line database
    // xml array "events", xml item "event"
    // actually we might also need a collection for this to handle the things
    private static Dictionary<int, string> allEvents = new Dictionary<int, string>();
    // xml array "traits", xml item "trait"
    private static Dictionary<int, string> allTraits = new Dictionary<int, string>();
    public static int allRelStatusCount = 4;

    // models
    private static DirectorModel model;
    private static DirectorData data;

    #region INITIALIZATION
    public static void Start()
    {
        // load all events and count unique
        allEvents = IdCollection.LoadArrayAsDict(EVENTS_XML_PATH);
        allTraits = IdCollection.LoadArrayAsDict(TRAITS_XML_PATH);
        //LoadTopics();

        // temp
        allEvents.Add(0, "1");
        allTraits.Add(0, "1");
        allTraits.Add(1, "2");
        // load possible traits and count unique

        // we also need to initialize the mapevents to have all the scenes in the dictionary.

        LoadLines();
        LoadSpeakers();

        // initialize model
        model = new DirectorModel(allEvents.Count, allTraits.Count, LineDB.Count, allRelStatusCount);

        // start by setting appropriate data.
        model.Start();
    }

    public static void LoadTopics()
    {
        IdCollection topicIds = XMLUtility.LoadFromPath<IdCollection>(TOPICS_XML_PATH);

        // add topic ids to topic list, with default topic number
        foreach(string topic in topicIds.allIds)
        {
            topicList.Add(topic, 1);
        }
    }

    /// <summary>
    /// Loads all the lines from XML files upon startup
    /// </summary>
    public static void LoadLines()
    {
        // we can have a text file here describing the file names of all dialogue XMLs.
        LineDB = DialogueLineCollection.LoadAll(new string[] {
            "Data/XML/dialogue/dialoguePlayer.xml",
            "Data/XML/dialogue/dialogueJonathan.xml"
        });

        Debug.Log("success");
    }

    /// <summary>
    /// Loading all speakers on startup.
    /// Filler characters will be loaded into defaults, while main characters will be loaded straight
    /// into the allSpeakers list.
    /// </summary>
    public static void LoadSpeakers()
    {
        // create a new speakercollection
        SpeakerCollection loadSpeakers = XMLUtility.LoadFromPath<SpeakerCollection>(SPEAKERS_XML_PATH);

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
    #endregion

    #region TOOLS OR UTILITY

    /// <summary>
    /// Looks up the numerical key from the reference list of strings
    /// </summary>
    /// <param name="fromEvents">Look up from events dict</param>
    /// <param name="fromTraits">Look up from traits dict</param>
    /// <param name="fromlineDB">Look up from line db dict</param>
    /// <param name="findVal">the string to find the key of</param>
    /// <returns></returns>
    public static int NumKeyLookUp(string findVal, bool fromEvents = false, bool fromTraits = false, bool fromlineDB = false, Dictionary<int, string> refDict = null)
    {
        if (fromTraits == true)
        {
            foreach (KeyValuePair<int, string> p in allTraits.Where(pair => pair.Value == findVal))
            {
                // return first pair
                return p.Key;
            }
        }
        else if(fromEvents == true)
        {
            foreach(KeyValuePair<int, string> p in allEvents.Where(pair => pair.Value == findVal))
            {
                return p.Key;
            }
        }
        // we use a reference dictionary to get the corresponding key from a value.
        else if(refDict != null)
        {
            foreach (KeyValuePair<int, string> p in refDict.Where(pair => pair.Value == findVal))
            {
                return p.Key;
            }
        }

        Debug.LogWarning($"Was not able to find the key corresponding to {findVal}. Returning -1");
        return -1;
    }

    /// <summary>
    /// Checks if the speaker object exists when talking to them.
    /// </summary>
    /// <param name="npcObjId"></param>
    /// <returns></returns>
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
        return allSpeakers[activeNPC].displayName;
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
    /// First load of scene, we create a list and add it to the mapevents dict
    /// </summary>
    /// <param name="name"></param>
    public static void SceneFirstLoad()
    {
        mapEvents.Add(SceneUtility.currentScene, new List<int>());
    }
    #endregion

    #region DIRECTING
    /// <summary>
    /// This updates the map data as well as the active NPC then requests a starting line
    /// </summary>
    public static string[] StartAndGetLine(string npcId, string mapId)
    {
        activeNPC = npcId;
        currentMap = mapId;
        
        return GetNPCLine();
    }

    /// <summary>
    /// initializes DirectorData as well as merges all global, map, and memory events into one list.
    /// </summary>
    public static int[] InitData()
    {
        data = model.SetData();

        List<int> npcMemory = new List<int>();
        // for each element of speaker memory, we convert that to its respective number id and add to npc memory.
        allSpeakers[activeNPC].speakerMemories.ForEach(
            memory => npcMemory.Add(
                NumKeyLookUp(memory, fromlineDB: true)));

        // we combine all events together
        List<int> globalAndMap = globalEvents.Union(mapEvents[currentMap]).ToList();

        return npcMemory.Union(globalAndMap).ToArray();
    }


    /// <summary>
    /// Uses the inference engine to infer the best possible line
    /// </summary>
    /// <param name="playerChoice">The lline chosen by player</param>
    /// <returns>DialogueLine.string dialogue selected by the engine</returns>
    public static string[] GetNPCLine(int playerChoice=-1)
    {
        Debug.Log("Talking to: " + activeNPC);
        // TESTING FOR JONATHAN ONLY
        globalEvents.Add(NumKeyLookUp( allEvents[0], fromEvents:true));

        // if we have selected some choice...
        if(playerChoice != -1)
        {
            // show player choice.
            Debug.Log("Selected line: " + playerChoices[playerChoice]);

            // update data of NPC given what the player chose.
            UpdateNPCData(playerChoices[playerChoice]);
        }
        
        // train
        int[] allKnownEvents = InitData();

        // our selected npc line will be prevline -- it will be remembered.
        int lineId = model.SelectNPCLine(
            allKnownEvents,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].relWithPlayer,
            data,
            topicList,
            mood);

        prevLine = LineDB[lineId];

        // update player data given acquired line of NPC
        UpdatePlayerData(prevLine);

        // we get the line text itself + the resulting "image" or portrait to accompany it.
        return new string[] { prevLine.dialogue, prevLine.portrait };
    }

    /// <summary>
    /// Selects a maximum of 3 lines from the database.
    /// </summary>
    /// <param name="globalEvents"></param>
    /// <param name="mapEvents"></param>
    /// <param name="activeNPC"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<string> GetPlayerLines()
    {
        // clear player lines
        playerChoices.Clear();

        /// train
        int[] allKnownEvents = InitData();

        // select some player lines.
        int[] lineIds = model.SelectPlayerLines(
            allKnownEvents,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].relWithPlayer,
            data,
            topicList,
            mood);

        foreach(int i in lineIds)
        {
            playerChoices.Add(LineDB[i]);
        }
        
        // from the selected player choices, we return the dialogue TEXT only.
        return playerChoices.Select(s => s.dialogue).ToList();
    }
    #endregion

    #region UPDATING INFORMATION

    // update npc-specific data
    public static void UpdateNPCData(DialogueLine line)
    {
       
        foreach (string m in line.effect.addToNPCMemory)
        {
            AddToSpeakerMemory(activeNPC, m);
        }

        UpdateSpeakerData(line);

        // if the line effect is to exit, we need to wait until the player scrolls through all lines before concluding the dialogue
        // what we can do is to deactivate the director early.
        if (line.effect.exit)
        {
            Debug.Log("DEACTIVATING DIRECTOR: The resulting DialogueLine has an exit effect.");

            isActive = false;
        }
    }

    // update player specific data
    public static void UpdatePlayerData(DialogueLine line)
    {

        foreach (string m in line.effect.addToPlayerMemory)
        {
            AddToSpeakerMemory("player", m);
        }

        UpdateSpeakerData(line);

        // the exit effect differs from player and npc line.
        // we know that there's no other follow up when selecting a player line. thus, we call conclude dialogue agad
        // if exit
        if (line.effect.exit)
        {
            Debug.Log("EXIT VIA DIALOGUE");
            EventHandler.Instance.ConcludeDialogue();
        }
    }

    public static void UpdateSpeakerData(DialogueLine line)
    {
        // set this line to be said already.
        line.isSaid = true;

        mood = line.ResponseStrToInt();

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
            int eventId = NumKeyLookUp(e, fromEvents:true);
            // only add if the global events are in the list
            if (!globalEvents.Contains(eventId))
            {
                globalEvents.Add(eventId);
            }
        }

        // add to map events
        foreach (string e in line.effect.addEventToMap)
        {
            int eventId = NumKeyLookUp(e, fromEvents:true);
            if (!mapEvents[currentMap].Contains(eventId))
            {
                mapEvents[currentMap].Add(eventId);
            }
        }
    }

    public static void AddToSpeakerMemory(string speaker, string eventId)
    {
        if (!allSpeakers[speaker].speakerMemories.Contains(eventId))
        {
            allSpeakers[speaker].speakerMemories.Add(eventId);
        }
    }
    #endregion

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
