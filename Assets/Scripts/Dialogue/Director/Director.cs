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
    // remembering the tone or mood of current convo
    private static int mood;

    /*
     * STUFF LOADED AT THE START OF GAME
     */
     
    public static Dictionary<int, DialogueLine> lineDB { get; set; }    // dialogue line database
    // xml array "events", xml item "event"
    // actually we might also need a collection for this to handle the things
    private static Dictionary<int, string> allEvents = new Dictionary<int, string>();
    // xml array "traits", xml item "trait"
    private static Dictionary<int, string> allTraits = new Dictionary<int, string>();
    public static int allRelStatusCount = 3;

    // models
    private static DirectorTraining trainingModel;
    private static DirectorPredictor predictionModel;
    private static DirectorData data;


    public static void Start()
    {

        // load all events and count unique
        // temp
        allEvents.Add(0, "1");
        allEvents.Add(1, "2");
        allTraits.Add(0, "1");
        allTraits.Add(1, "2");
        // load possible traits and count unique

        LoadLines();
        LoadSpeakers();
        
        // the models
        trainingModel = new DirectorTraining(allEvents.Count, allTraits.Count, lineDB.Count, allRelStatusCount);
        predictionModel = new DirectorPredictor(allEvents.Count, allTraits.Count, lineDB.Count, allRelStatusCount);
        data = DirectorData.SetDataUniform(allEvents.Count, allTraits.Count, allRelStatusCount, lineDB.Count);

    }
    
    /// <summary>
    /// Loads all the lines from XML files upon startup
    /// </summary>
    public static void LoadLines()
    {
        // we can have a text file here describing the file names of all dialogue XMLs.
        lineDB = DialogueLineCollection.LoadAll(new string[] {
            "Data/XML/dialogue/dialoguePlayer.xml"
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

    /// <summary>
    /// Looks up the numerical key from the reference list of strings
    /// </summary>
    /// <param name="fromEvents">Look up from events dict</param>
    /// <param name="fromTraits">Look up from traits dict</param>
    /// <param name="fromlineDB">Look up from line db dict</param>
    /// <param name="findVal">the string to find the key of</param>
    /// <returns></returns>
    public static int NumKeyLookUp(string findVal, bool fromEvents = false, bool fromTraits = false, bool fromlineDB = false)
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
    /// This updates the map data as well as the active NPC then requests a starting line
    /// </summary>
    public static DialogueLine StartAndGetLine(string npcId, string mapId)
    {
        activeNPC = npcId;
        currentMap = mapId;
        
        return GetNPCLine();
    }

    /// <summary>
    /// Trains our training model and 
    /// </summary>
    public static void TrainModel()
    {
        // lets set the model priors to be the data we currently have
        // either acquired from posteriors of previous inference @ end of this fxn or
        // set by default as uniform at start of game
        trainingModel.SetDirectorData(data);

        // train the model
        int[] allEventsOccurred = new List<int>(globalEvents.Concat(mapEvents[currentMap])).ToArray();
        int[] allTraitsNPC = allSpeakers[activeNPC].speakerTraits.ToArray();
        int rel = allSpeakers[activeNPC].RelationshipStatus();

        // make new inferences based on new data
        data = trainingModel.InferPredictionPriors(allEventsOccurred, allTraitsNPC, rel);
        // these probabilities in data will then be used in directorpredictor to
        // infer the actual lines.
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
            UpdateNPCData(playerLine);
        }
        
        // train
        TrainModel();

        // make inferences on best line
        // set the model data for our prediction model
        predictionModel.SetDirectorData(data);

        // our selected npc line will be prevline
        prevLine = predictionModel.SelectBestNPCLine(topicList, mood);

        // select best NPC line.
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
        UpdatePlayerData(prevLine);

        // training
        TrainModel();

        // set model data of predictor
        predictionModel.SetDirectorData(data);

        return predictionModel.SelectBestPlayerLines(topicList, mood);
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
