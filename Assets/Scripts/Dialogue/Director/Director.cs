using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DirectorConstants
{
    public static readonly int MAX_REL_STATUS = 3;
    public static readonly string GAME_IS_ACTIVE = "GameIsActive";
    public static readonly string DEFAULT_MAP = "current_map";

    // default data
    public static readonly string NONE_STR = "none";
    public static readonly string PLAYER_STR = "player";
    // topics
    public static readonly string TOPIC_START_CONVO = "StartConversation";
    public static readonly string TOPIC_END_CONVO = "EndConversation";

    // topic relevance
    public static readonly double TOPIC_RELEVANCE_PRIO = 3.0;
    public static readonly double TOPIC_RELEVANCE_HIGH = 2.0;
    public static readonly double TOPIC_RELEVANCE_BASE = 1.0;
    public static readonly double TOPIC_RELEVANCE_CLOSE = 0.0;

    public enum MoodThreshold
    {
        GOOD = 1,
        BAD = -1
    };

    // NUMERICAL VALUE OF RELATIONSHIP STATUS
    public enum REL_STATUS_NUMS
    {
        BAD,        // 0
        NEUTRAL,    // 1
        GOOD,       // 2
        NONE = -1,
        BAD_THRESH = -20,
        GOOD_THRESH = 20,
    };

    // STRING VERSION OF RELATIONSHIP STATUS (equivalent to rel_status_nums above)
    public static class REL_STATUS_STRING
    {
        public static readonly string GOOD = "good";
        public static readonly string NEUTRAL = "neut";
        public static readonly string BAD = "bad";
        public static readonly string NONE = "none";    // no requirement.
    };
}

/// <summary>
/// In charge of determining the lines to use.
/// </summary>
public static class Director
{
    public static readonly string EVENTS_XML_PATH = $"XMLs/DB/allEvents";
    public static readonly string TOPICS_XML_PATH = $"XMLs/DB/allTopics";
    public static readonly string TRAITS_XML_PATH = $"XMLs/DB/allTraits";
    public static readonly string SPEAKERS_XML_PATH = $"XMLs/Speakers";
    public static readonly string[] DIALOGUE_XML_PATH = new string[]
    {
        $"XMLs/dialogue/dialoguePlayer",
        $"XMLs/dialogue/dialogueJonathan",
        $"XMLs/dialogue/dialogueCassandra",
        $"XMLs/dialogue/dialogueFiller_Custodian",
        $"XMLs/dialogue/dialogueFiller_Assistant"
    };

    // director is active? -- currently being used?
    public static bool isActive;

    // tracking events and speakers.
    public static string activeNPC;                                 // the current NPC speaking
    public static string currentMap;                               // current location
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
    public static Dictionary<string, Speaker> allSpeakers = new Dictionary<string, Speaker>();
    // list of topics
    private static Dictionary<string, double> topicList = new Dictionary<string, double>();
    // defaults of each filler archetype to clone
    public static Dictionary<string, Speaker> speakerDefaults = new Dictionary<string, Speaker>();
    
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

    private static List<int> queriedMemories = new List<int>();

    private static Queue<DialogueLine> shortTermMemory = new Queue<DialogueLine>();
    private static readonly int MAX_SHORT_TERM_MEMORY = 5; 
    // when max, dequeue and change the isSaid to true permanently

    
    #region INITIALIZATION

    /// <summary>
    /// Upon starting the game -- not the save
    /// </summary>
    public static void Start()
    {
        // load all events and count unique
        allEvents = IdCollection.LoadArrayAsDict(EVENTS_XML_PATH);
        allTraits = IdCollection.LoadArrayAsDict(TRAITS_XML_PATH);
        IdCollection topicIds = LoadTopics();


        LoadLines();
        LoadSpeakers();

        // add topics to the speakers
        foreach(Speaker s in speakerDefaults.Values)
        {
            s.InitializeTopics(topicIds, (double) DirectorConstants.TOPIC_RELEVANCE_BASE);
            s.PrioritizeTopics("MissingArtifact");
        }

        // initialize model
        model = new DirectorModel(allEvents.Count, allTraits.Count, LineDB.Count, DirectorConstants.MAX_REL_STATUS, new Microsoft.ML.Probabilistic.Algorithms.ExpectationPropagation());

        // start by setting appropriate data and loading the CPT
        model.Start();

        allSpeakers[DirectorConstants.PLAYER_STR].InitializeSpeakerCPT(model);

        // add to player memory
        AddToSpeakerMemory(DirectorConstants.PLAYER_STR, "ArtifactNotFound");
        // add gamestart event
        AddEventString(DirectorConstants.GAME_IS_ACTIVE);

    }
    
    public static IdCollection LoadTopics()
    {
        TextAsset topicAsset = (TextAsset)Resources.Load(TOPICS_XML_PATH);

        return XMLUtility.LoadFromText<IdCollection>(topicAsset);
    }

    /// <summary>
    /// Loads all the lines from XML files upon startup
    /// </summary>
    public static void LoadLines()
    {
        // we can have a text file here describing the file names of all dialogue XMLs.
        LineDB = DialogueLineCollection.LoadAll( DIALOGUE_XML_PATH );
        
        Debug.Log("success");
    }

    /// <summary>
    /// Loading all speakers on startup.
    /// Filler characters will be loaded into defaults, while main characters will be loaded straight
    /// into the allSpeakers list.
    /// </summary>
    public static void LoadSpeakers()
    {
        TextAsset speakerResource = (TextAsset) Resources.Load(SPEAKERS_XML_PATH);
        // create a new speakercollection
        SpeakerCollection loadSpeakers = XMLUtility.LoadFromText<SpeakerCollection>(speakerResource);

        Debug.Log(loadSpeakers.Speakers.Length);
        TestPrintSpeakers(loadSpeakers);

        // we add each speaker into the defaults dictionary
        foreach(Speaker s in loadSpeakers.Speakers)
        {
            speakerDefaults.Add(s.speakerArchetype, s);
            // add topics to the speaker default
            Debug.Log($"adding speaker {s} with archetype {s.speakerArchetype}");
        }

        // add the player
        allSpeakers.Add("player", speakerDefaults["player"].Clone());

        Debug.Log("added player -- length of memories: " + allSpeakers["player"].speakerMemories.Count);
        
    }
    #endregion

    #region TOOLS OR UTILITY

    /// <summary>
    /// For ending or concluding the dialogue
    /// </summary>
    public static void Deactivate()
    {
        // set isactive to false
        isActive = false;

        // set end conversation to default value
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = (double)DirectorConstants.TOPIC_RELEVANCE_CLOSE;

        // remove item from memory
        if(activeHeldItem!= "")
            allSpeakers[DirectorConstants.PLAYER_STR].speakerMemories.Remove("ShowItem:" + activeHeldItem);
        
        // all topics return to default value
        List<string> topics = allSpeakers[activeNPC].topics.Keys.ToList();
        foreach(string topic in topics)
        {
            // if we haven't closed the topic then we should return to default
            if(allSpeakers[activeNPC].topics[topic] != (double)DirectorConstants.TOPIC_RELEVANCE_CLOSE)
                topicList[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_BASE;
        }
    }

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

    public static void PrioritizeTopic_AllSpeakers(string topic)
    {
        foreach(Speaker s in allSpeakers.Values)
        {
            s.PrioritizeTopics(topic);
        }
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

        // if filler speaker, we will randomize the trait
        allSpeakers[npcObjId].OverrideTraits(npc);

        // if the given display name is not empty, then we will override the speaker's display name with what is given
        allSpeakers[npcObjId].OverrideDisplayName(displayName);

        allSpeakers[npcObjId].InitializeSpeakerCPT(model);

        Debug.Log($"Added speaker with id {npcObjId}");
        Debug.Log($"Checking if the probabilities are not empty: {allSpeakers[npcObjId].currentPosteriors.Count}");
    }

    /// <summary>
    /// First load of scene, we create a list and add it to the mapevents dict
    /// </summary>
    /// <param name="name"></param>
    public static void SceneFirstLoad()
    {
        mapEvents.Add(SceneUtility.currentScene, new List<int>());
        Debug.Log("Added the map: " + SceneUtility.currentScene);
    }

    /// <summary>
    /// Checks if an event has occurred globally
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public static bool EventHasOccurred(string eventName)
    {
        int eventId = NumKeyLookUp(eventName, refDict: allEvents);

        if(globalEvents.Count == 0 && mapEvents[currentMap].Count == 0)
        {
            return false;
        }

        if(globalEvents.Count >= 0 && mapEvents[currentMap].Count >= 0 && globalEvents.Union(mapEvents[currentMap]).Contains(eventId))
        {
            Debug.Log("event " + eventName + " has occurred in game");
            return true;
        }

        return false;
    }
    #endregion

    #region DIRECTING
    /// <summary>
    /// This updates the map data as well as the active NPC then requests a starting line
    /// </summary>
    public static string[] StartAndGetLine(string npcId, string mapId)
    {
        activeNPC = npcId;

        // reset the values of isSaid in short term memory
        foreach(DialogueLine line in shortTermMemory)
        {
            line.isSaid = false;
        }
        
        // set current relevant topic to be startconversation
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_START_CONVO] = (double)DirectorConstants.TOPIC_RELEVANCE_PRIO;
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = (double)DirectorConstants.TOPIC_RELEVANCE_CLOSE;
        // start with 0 mood -- neutral
        mood = 0;

        if (activeHeldItem != "")
        {
            // add currently held item to list of events
            AddToSpeakerMemory(activeNPC, "ShowItem:" + activeHeldItem);
            AddToSpeakerMemory(DirectorConstants.PLAYER_STR, "ShowItem:" + activeHeldItem);
        }
        
        Debug.Log("Starting convo...");
        
        return GetNPCLine();
    }

    /// <summary>
    /// Merges all global, map, and memory events into one list.
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

        // remove the events that have already been queried -- unremoved yung irereturn
        queriedMemories.ForEach(mem => globalAndMap.Remove(mem));
        // update queried memories
        globalAndMap.ForEach(item => queriedMemories.Add(item));

        // remove events queried in memory of npc
        allSpeakers[activeNPC].queriedMemories.ForEach(mem => npcMemory.Remove(mem));
        npcMemory.ForEach(item => allSpeakers[activeNPC].queriedMemories.Add(item));

        // no event? return { none }
        if (globalAndMap.Count == 0 && npcMemory.Count == 0)
        {
            EventHandler.Instance.UpdateDebugDisplay(new string[] { "no unqueried events" });
            return null;
        }

        // testing -- this outputs all the values entered into the model
        List<string> allEvs = new List<string>();
        npcMemory.Union(globalAndMap).ToList().ForEach(
            mem => allEvs.Add(allEvents[mem]));
        EventHandler.Instance.UpdateDebugDisplay(new string[] { string.Join(',', allEvs) });

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
            Debug.Log("Selected line: " + playerChoices[playerChoice].dialogue);
            DialogueLine choice = playerChoices[playerChoice];

            CheckExit(choice);
            
            // we also update topic relevance -- all topics that are not in choice.relatedTopics will have a reduced relevance.
            UpdateTopics(choice);
            // update data of NPC given what the player chose.
            // if player choice has an exit condition, then exit tayo kaagad from within NPCData
            UpdateNPCData(choice);
            UpdatePlayerData(choice);
        }
        
        int[] allKnownEvents = null;
        
        // our selected npc line will be prevline -- it will be remembered.
        int lineId = model.SelectNPCLine(
            allKnownEvents,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].RelationshipStatus(),
            allSpeakers[activeNPC].topics,
            mood,
            currentMap,
            allSpeakers[activeNPC].speakerArchetype,
            allSpeakers[activeNPC].currentPosteriors);

        Debug.Log("selected line: " + lineId);

        prevLine = LineDB[lineId];
        
        // update player data and NPC data given acquired line of NPC
        UpdatePlayerData(prevLine);
        UpdateNPCData(prevLine);

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

        CheckExit(prevLine);

        Debug.Log("==== PLAYER TURN ====");
        // clear player lines
        playerChoices.Clear();

        // select some player lines.
        int[] lineIds = model.SelectPlayerLines(
            null,
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].RelationshipStatus(),
            allSpeakers[activeNPC].topics,
            mood,
            currentMap,
            allSpeakers[activeNPC].speakerArchetype,
            allSpeakers[activeNPC].currentPosteriors);

        foreach(int i in lineIds)
        {
            playerChoices.Add(LineDB[i]);
        }
        
        // from the selected player choices, we return the dialogue TEXT only.
        return playerChoices.Select(s => s.dialogue).ToList();
    }
    #endregion

    #region UPDATING INFORMATION

    public static void CheckExit(DialogueLine line)
    {

        // if the line effect is to exit, we need to wait until the player scrolls through all lines before concluding the dialogue
        // what we can do is to deactivate the director early.
        if (line.effect.exit)
        {
            Debug.Log("DEACTIVATING DIRECTOR: The resulting DialogueLine has an exit effect.");

            EventHandler.Instance.ConcludeDialogue();
        }
    }

    // update npc-specific data
    public static void UpdateNPCData(DialogueLine line)
    {
       
        foreach (string m in line.effect.addToNPCMemory)
        {
            Debug.Log("adding " + m + " to the memory of " + allSpeakers[activeNPC].speakerId);
            AddToSpeakerMemory(activeNPC, m);
        }
        

        UpdateSpeakerData(line);
    }

    // update player specific data
    public static void UpdatePlayerData(DialogueLine line)
    {

        foreach (string m in line.effect.addToPlayerMemory)
        {
            AddToSpeakerMemory("player", m);
        }

        UpdateSpeakerData(line);
    }

    public static void UpdateSpeakerData(DialogueLine line)
    {
        // set this line to be said already -- IF ITS NOT A GENERIC STARTER
        if (line.relatedTopics.Length == 1 && (line.relatedTopics[0].Equals(DirectorConstants.TOPIC_START_CONVO)))
        {
            line.isSaid = false;    // generic starter will always be false sa is said.
            Debug.Log("line isSaid chosen is FALSE");
        }
        else if (line.speakerId == DirectorConstants.PLAYER_STR)
        {
            // add the line into the short term memory
            line.isSaid = true;
            shortTermMemory.Enqueue(line);
            
            if(shortTermMemory.Count == MAX_SHORT_TERM_MEMORY)
            {
                // dequeue and permanently make the line isSaid to be true.
                shortTermMemory.Dequeue().isSaid = true;
            }
        }

        mood = line.ResponseStrToInt();

        // access reationship with active npc and update it.
        UpdateRelationship(line.effect.relationshipEffect);
        
        // update topic relevance table
        // set topic relevance to be the maximum.
        if(line.effect.makeMostRelevantTopic != "" || line.effect.makeMostRelevantTopic != null)
        {
            foreach(string topic in line.effect.makeMostRelevantTopic.Split('/'))
                allSpeakers[activeNPC].topics[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_HIGH;
        }
        else
        {
            // we use the related topic of the selected line that isn't StartConvo
            // the idea is, the topic that the line addresses will become the most relevant.
            foreach(string topic in line.relatedTopics)
            {
                if(topic != DirectorConstants.TOPIC_START_CONVO || topic != DirectorConstants.TOPIC_END_CONVO)
                    allSpeakers[activeNPC].topics[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_HIGH;
            }
        }

        if (line.effect.closeTopic != "" || line.effect.closeTopic != null)
        {
            // set teh topic listed to 1 (default value)
            allSpeakers[activeNPC].topics[line.effect.closeTopic] = (float)DirectorConstants.TOPIC_RELEVANCE_CLOSE;
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

        if(line.effect.item != "")
        {
            EventHandler.Instance.PickupItem(line.effect.item);
        }

        //TestPrintEventTrackers();
        GetAllTopicRelevance();
    }

    public static void UpdateRelationship(int value)
    {
        allSpeakers[activeNPC].relWithPlayer += value;

        if (allSpeakers[activeNPC].currentRelStatus != allSpeakers[activeNPC].RelationshipStatus())
        {
            // update currentrel
            allSpeakers[activeNPC].currentRelStatus = allSpeakers[activeNPC].RelationshipStatus();
            // update model
            model.UpdateSpeakerDialogueProbs(null, 
                null, 
                new int[] { allSpeakers[activeNPC].currentRelStatus },
                ref allSpeakers[activeNPC].currentPosteriors,
                ref allSpeakers[activeNPC].currentDialogueCPT);
        }
    }
    

    /// <summary>
    /// Topics that aren't related to the dialogue of choice will be updated to degrade.
    /// </summary>
    /// <param name="choice"></param>
    public static void UpdateTopics(DialogueLine choice)
    {
        // all topics that are not in related topics of the line and not in the topics to makee most relevant
        // will be BASE value
        foreach (string topic in allSpeakers[activeNPC].topics.Keys.Where(t => !choice.relatedTopics.Contains(t) && !choice.effect.makeMostRelevantTopic.Split('/').Contains(t)).ToList())
        {
            // check if the topic should be closed -- if yes, don't increase to base
            if(allSpeakers[activeNPC].topics[topic] != (double) DirectorConstants.TOPIC_RELEVANCE_CLOSE)
                allSpeakers[activeNPC].topics[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_BASE;
        }

        // the bookends (start and end convo topics) will always be 0.
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_START_CONVO] = 0.0;
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = (double)DirectorConstants.TOPIC_RELEVANCE_CLOSE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speaker"> speaker ID or object id</param>
    /// <param name="eventId">event string</param>
    public static void AddToSpeakerMemory(string speaker, string eventId)
    {
        // we check if the memory already contains our said event, and if the event exists in our event db
        if (!(allSpeakers[speaker].speakerMemories.Contains(eventId)) && allEvents.ContainsValue(eventId))
        {
            allSpeakers[speaker].speakerMemories.Add(eventId);
            // update probability table of the speaker in question
            model.UpdateSpeakerDialogueProbs(new int[] { NumKeyLookUp( eventId, refDict: allEvents) }, 
                null, 
                null, 
                ref allSpeakers[speaker].currentPosteriors,
                ref allSpeakers[speaker].currentDialogueCPT);
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

            // update the probability table of ALL speakers in allspeaker list since global
            foreach (Speaker s in allSpeakers.Values)
            {
                model.UpdateSpeakerDialogueProbs(new int[] { eventId },
                    null,
                    null,
                    ref s.currentPosteriors,
                    ref s.currentDialogueCPT);
            }
        }
        else if (map != "global" && !(mapEvents[map].Contains(eventId)))
        {
            mapEvents[map].Add(eventId);

            // update the probability table of ALL speakers whose spawn location is the current scene
            foreach (Speaker s in allSpeakers.Values.Where(sp => sp.spawnLocation == SceneUtility.currentScene))
            {
                model.UpdateSpeakerDialogueProbs(new int[] { eventId },
                    null,
                    null,
                    ref s.currentPosteriors,
                    ref s.currentDialogueCPT);
            }
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
        foreach(KeyValuePair<string, double> pair in allSpeakers[activeNPC].topics)
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
