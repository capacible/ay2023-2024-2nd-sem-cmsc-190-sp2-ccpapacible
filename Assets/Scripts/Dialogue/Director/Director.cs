using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DirectorConstants
{
    public static readonly int MAX_REL_STATUS = 3;

    public enum MoodThreshold
    {
        GOOD = 1,
        BAD = -1
    };

    public enum TopicRelevance
    {
        MAX = 3,
        DEFAULT = 1,
        MIN = 0
    };
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
    public static string activeHeldItem { private get; set; }       // the currently held item, to be added at start 
                                                                    // and removed at end of convo
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

    // models
    private static DirectorModel model;

    #region INITIALIZATION

    /// <summary>
    /// Upon starting the game -- not the save
    /// </summary>
    public static void Start()
    {
        // load all events and count unique
        allEvents = IdCollection.LoadArrayAsDict(EVENTS_XML_PATH);
        allTraits = IdCollection.LoadArrayAsDict(TRAITS_XML_PATH);
        LoadTopics();
        
        LoadLines();
        LoadSpeakers();

        // initialize model
        model = new DirectorModel(allEvents.Count, allTraits.Count, LineDB.Count, DirectorConstants.MAX_REL_STATUS);

        // start by setting appropriate data and loading the CPT
        model.Start();
    }

    public static void LoadTopics()
    {
        IdCollection topicIds = XMLUtility.LoadFromPath<IdCollection>(TOPICS_XML_PATH);

        // add topic ids to topic list
        // UNREFERENCED TOPIC START AT 0.5
        foreach(string topic in topicIds.allIds)
        {
            topicList.Add(topic, (float)0.5);
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
            "Data/XML/dialogue/dialogueJonathan.xml",
            "Data/XML/dialogue/dialogueFiller_Custodian.xml"
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
        Debug.Log("added player -- length of memories: " + allSpeakers["player"].speakerMemories.Count);
        
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
        Debug.Log("finding key: " + findVal);
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
                Debug.Log("returning " + p.Value + " with key " + p.Key);
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

        // set current relevant topic to be startconversation
        topicList["StartConversation"] = (float)DirectorConstants.TopicRelevance.MAX;
        // start with 0 mood -- neutral
        mood = 0;

        // add currently held item to list of events
        AddToSpeakerMemory(activeNPC, "ShowItem:"+activeHeldItem);

        Debug.Log("Starting convo...");
        
        return GetNPCLine();
    }

    /// <summary>
    /// initializes DirectorData as well as merges all global, map, and memory events into one list.
    /// </summary>
    /// <param name="npc"> 
    /// the id of the npc speaker to access the memory of. by default, it's player, but when npc is speaking, it's
    /// the current active npc.
    /// </param>
    public static int[] InitData(string npc="player")
    {
        List<int> npcMemory = new List<int>();
        if(allSpeakers[npc].speakerMemories.Count > 0)
        {
            Debug.Log("we have memories.");
            // for each element of speaker memory, we convert that to its respective number id and add to npc memory.
            allSpeakers[npc].speakerMemories.ForEach(
                memory => npcMemory.Add(
                    NumKeyLookUp(memory, refDict: allEvents)));
        }

        // we combine all events together
        List<int> globalAndMap = globalEvents.Union(mapEvents[currentMap]).ToList();

        // no event? return { none }
        if(globalAndMap.Count == 0 && npcMemory.Count == 0)
        {
            return null;
        }

        return npcMemory.Union(globalAndMap).ToArray();
    }


    /// <summary>
    /// Uses the inference engine to infer the best possible line
    /// </summary>
    /// <param name="playerChoice">The lline chosen by player</param>
    /// <returns>DialogueLine.string dialogue selected by the engine</returns>
    public static string[] GetNPCLine(int playerChoice=-1)
    {
        Debug.Log("==== NPC TURN ====");

        Debug.Log("Talking to: " + activeNPC);

        // if we have selected some choice...
        if(playerChoice != -1)
        {
            // show player choice.
            Debug.Log("Selected line: " + playerChoices[playerChoice]);
            DialogueLine choice = playerChoices[playerChoice];
            
            // we also update topic relevance -- all topics that are not in choice.relatedTopics will have a reduced relevance.
            UpdateTopics(choice);
            // update data of NPC given what the player chose.
            UpdateNPCData(choice);
        }
        
        int[] allKnownEvents = InitData(activeNPC);

        // our selected npc line will be prevline -- it will be remembered.
        int lineId = model.SelectNPCLine(
            allKnownEvents,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].RelationshipStatus(),
            topicList,
            mood);

        Debug.Log("selected line: " + lineId);

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
        Debug.Log("==== PLAYER TURN ====");
        // clear player lines
        playerChoices.Clear();

        /// train
        int[] allKnownEvents = InitData();

        // select some player lines.
        int[] lineIds = model.SelectPlayerLines(
            allKnownEvents,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].RelationshipStatus(),
            topicList,
            mood,
            allSpeakers[activeNPC].speakerArchetype);

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
            Debug.Log("adding " + m + " to the memory of " + allSpeakers[activeNPC]);
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
            topicList[line.effect.makeMostRelevantTopic] = (float)DirectorConstants.TopicRelevance.MAX;
        }
        else
        {
            // we use the related topic of the selected line that isn't StartConvo
            // the idea is, the topic that the line addresses will become the most relevant.
            foreach(string topic in line.relatedTopics)
            {
                topicList[topic] = (float)DirectorConstants.TopicRelevance.MAX;
            }
        }

        if (line.effect.closeTopic != "")
        {
            // set teh topic listed to 1 (default value)
            topicList[line.effect.closeTopic] = (float)DirectorConstants.TopicRelevance.DEFAULT;
        }

        // add to global events
        foreach (string e in line.effect.addEventToGlobal)
        {
            AddEventString(e);
        }

        // add to map events
        foreach (string e in line.effect.addEventToMap)
        {
            int eventId = NumKeyLookUp(e, refDict: allEvents);
            AddEventString(e, currentMap);
        }

        //TestPrintEventTrackers();
        GetAllTopicRelevance();
    }
    

    /// <summary>
    /// Topics that aren't related to the dialogue of choice will be updated to degrade.
    /// </summary>
    /// <param name="choice"></param>
    public static void UpdateTopics(DialogueLine choice)
    {
        // for all topics that aren't in the choice's related topics
        // start conversation should also degrade as a topic even if it's in the related topics
        // the logic is that after starting the conversation, a conversation-starter topic is less relevant now.
        foreach(string topic in topicList.Keys.Where(t => !choice.relatedTopics.Contains(t)).ToList())
        {
            Debug.Log("Updating topic relevance for: " + topic);
            if (topicList[topic] - 0.3 <= 0)    
            {
                // if our topic is in the negatives na after computation, we reset the relevance to its default
                topicList[topic] = (float)DirectorConstants.TopicRelevance.DEFAULT;
            }
            else
            {
                topicList[topic] -= (float)0.3;
            }
        }

        topicList["StartConversation"] = (float)0.0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speaker"> speaker ID or object id</param>
    /// <param name="eventId">event string</param>
    public static void AddToSpeakerMemory(string speaker, string eventId)
    {
        // we check if the memory already contains our said event, and if the event exists in our event db
        if (!allSpeakers[speaker].speakerMemories.Contains(eventId) && allEvents.ContainsValue(eventId))
        {
            allSpeakers[speaker].speakerMemories.Add(eventId);
        }
    }

    /// <summary>
    /// Add an event to global or map
    /// </summary>
    /// <param name="e"></param>
    public static void AddEventString(string e, string map="global")
    {
        int eventId = NumKeyLookUp(e, refDict: allEvents);

        if(eventId == -1)
        {
            Debug.Log("event " + e + " does not exist in allEvents");
            return;
        }

        if( map == "global" && !(globalEvents.Contains(eventId)))
        {
            globalEvents.Add(eventId);
        }
        else if (map != "global" && !(mapEvents[map].Contains(eventId)))
        {
            mapEvents[map].Add(eventId);
        }
    }
    #endregion

    public static void TestPrintEventTrackers()
    {
        Debug.Log("Printing global events");
        foreach(int ev in globalEvents)
        {
            // event with id ev
            Debug.Log(allEvents[ev]);
        }

        Debug.Log("Printing map event");
        foreach (int ev in mapEvents[currentMap])
        {
            Debug.Log(allEvents[ev]);
        }


        Debug.Log("Printing npc memory of "+activeNPC);
        foreach (string ev in allSpeakers[activeNPC].speakerMemories)
        {
            Debug.Log(allEvents[NumKeyLookUp(ev, refDict:allEvents)]);
        }
    }

    public static string GetAllTopicRelevance()
    {
        string output = "TOPIC RELEVANCE:";
        foreach(KeyValuePair<string, float> pair in topicList)
        {
            output += $"topic: {pair.Key} | value: {pair.Value}\n";
        }

        return output;
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
