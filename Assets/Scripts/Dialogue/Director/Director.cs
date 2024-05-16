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
    public static readonly double TOPIC_RELEVANCE_PRIO = 2.0;
    public static readonly double TOPIC_RELEVANCE_HIGH = 1.5;
    public static readonly double TOPIC_RELEVANCE_BASE = 0.5;
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
    public static readonly string NAME_PLACEHOLDER_ACTIVE = "activeNPC";

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
    public static string activeNPC = "";                                 // the current NPC speaking
    public static string currentMap = "";                               // current location
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

    // debugging
    public static string debugTopics;
    public static bool startUp = true;

    
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
            s.InitializeTopics(topicIds, (double) DirectorConstants.TOPIC_RELEVANCE_CLOSE);
            s.PrioritizeTopics("MissingArtifact");

            // set the values of the below topics as 1
            s.topics["LookingForDirector"] = DirectorConstants.TOPIC_RELEVANCE_BASE;
            s.topics["CameraInStorage"] = DirectorConstants.TOPIC_RELEVANCE_BASE;
        }

        // initialize model
        model = new DirectorModel(allEvents.Count, allTraits.Count, LineDB.Count, DirectorConstants.MAX_REL_STATUS, new Microsoft.ML.Probabilistic.Algorithms.ExpectationPropagation());

        // start by setting appropriate data and loading the CPT
        model.Start();

        // add gamestart event
        AddEventString(DirectorConstants.GAME_IS_ACTIVE);

        // add the player
        AddNewSpeaker(null, DirectorConstants.PLAYER_STR, "You");

        // add to player memory
        AddToSpeakerMemory(DirectorConstants.PLAYER_STR, "ArtifactNotFound");
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
            s.LoadSpeakerDefaultCPT();
            // add topics to the speaker default
            Debug.Log($"adding speaker {s} with archetype {s.speakerArchetype}");
        }
        
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
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = DirectorConstants.TOPIC_RELEVANCE_CLOSE;

        // remove item from memory
        if(activeHeldItem!= "")
            allSpeakers[DirectorConstants.PLAYER_STR].speakerMemories.Remove("ShowItem:" + activeHeldItem);
        
        // all topics return to default value for the NPC only.
        List<string> topics = allSpeakers[activeNPC].topics.Keys.ToList();
        foreach(string topic in topics)
        {
            // if we haven't closed the topic then we should return to default
            if(allSpeakers[activeNPC].topics[topic] != (double)DirectorConstants.TOPIC_RELEVANCE_CLOSE)
                allSpeakers[activeNPC].topics[topic] = (double)DirectorConstants.TOPIC_RELEVANCE_BASE;
        }
        // remove active npc
        activeNPC = "";
    }

    public static void AddToShortTermMemory(DialogueLine line)
    {
        line.isSaid = true;
        shortTermMemory.Enqueue(line);

        // if maximum short term memory na, we permanently make sure that the line is already said.
        if (shortTermMemory.Count >= MAX_SHORT_TERM_MEMORY)
        {
            shortTermMemory.Dequeue().isSaid = true;    // permanently turn the line dequeued to issaid = true
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

    /// <summary>
    /// Sets the topic value of a particular topic for all speakers and reduces all other prioritized topics by some amount
    /// </summary>
    /// <param name="topic"></param>
    public static void PrioritizeTopic_AllSpeakers(string topic)
    {
        foreach(Speaker s in allSpeakers.Values)
        {
            s.PrioritizeTopics(topic);
        }

        Debug.Log("Prioritizing done");
    }

    /// <summary>
    /// Add speaker to the speaker trackers
    /// </summary>
    public static void AddNewSpeaker(NPCData npc, string npcObjId, string displayName)
    {

        if (npc != null)
        {
            // clone the default of that speaker archetype and add to actual speaker tracker
            allSpeakers.Add(npcObjId, speakerDefaults[npc.speakerArchetype].Clone());
            // if filler speaker, we will randomize the trait
            allSpeakers[npcObjId].OverrideTraits(npc);

            if (npc.speakerArchetype.ToLower().Contains("custodian"))
            {
                allSpeakers[npcObjId].relWithPlayer = 100;
            }
            
        }
        else
        {
            //player
            allSpeakers.Add(npcObjId, speakerDefaults[DirectorConstants.PLAYER_STR].Clone());
            allSpeakers[npcObjId].relWithPlayer = null;
        }

        // set the speaker id to be the object id.
        allSpeakers[npcObjId].speakerId = npcObjId;
        
        // if the given display name is not empty, then we will override the speaker's display name with what is given
        allSpeakers[npcObjId].OverrideDisplayName(displayName);

        // initialize the speaker cpts to its default values
        allSpeakers[npcObjId].InitializeSpeakerCPT(model, globalEvs: globalEvents);

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

        if(globalEvents.Count == 0 && mapEvents[currentMap].Count == 0 && allSpeakers[DirectorConstants.PLAYER_STR].speakerMemories.Count == 0)
        {
            return false;
        }

        int[] allEvs = CollectMemories();

        Debug.Log("Collected the memories");

        List<string> test = new List<string>();
        foreach(int i in allEvs)
        {
            test.Add(allEvents[i]);
        }
        Debug.Log("all events are: " + string.Join(" ,", test));
        
        if(allEvs.Length > 0 && allEvs.Contains(eventId))
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

        shortTermMemory.Clear();
        
        // set current relevant topic to be startconversation
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_START_CONVO] = (double)DirectorConstants.TOPIC_RELEVANCE_PRIO;
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = DirectorConstants.TOPIC_RELEVANCE_CLOSE;

        // start with 0 mood -- neutral
        mood = 0;
        
        if (activeHeldItem != "")
        {
            Debug.Log("active item: " + activeHeldItem);
            AddToSpeakerMemory(DirectorConstants.PLAYER_STR, "ShowItem:" + activeHeldItem);
        }

        Debug.Log("Starting convo...");

        UpdateProbabilitiesFromWholeMemory(activeNPC);
        
        return GetNPCLine();
    }

    /// <summary>
    /// Merges all global, map, and memory events into one list.
    /// </summary>
    /// <param name="npc"> 
    /// the id of the npc speaker to access the memory of. by default, it's player, but when npc is speaking, it's
    /// the current active npc.
    /// </param>
    public static int[] CollectMemories(string npc="player")
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

        List<int> globalAndMap = new List<int>();
        if (currentMap == "")
        {
            // if map is empty aka start of game plng tayo then global events only
            globalAndMap = globalEvents.ToList();
        }
        else
        {
            // if we are on map

            // we combine all events together
            globalAndMap = globalEvents.Union(mapEvents[currentMap]).ToList();
        }

        // no event? return { none }
        if (globalAndMap.Count == 0 && npcMemory.Count == 0)
        {
            EventHandler.Instance.UpdateDebugDisplay(new string[] { "no unqueried events" });
            return null;
        }

        // testing -- this outputs all the values entered into the model
        /*
        List<string> allEvs = new List<string>();
        npcMemory.Union(globalAndMap).ToList().ForEach(
            mem => allEvs.Add(allEvents[mem]));
        EventHandler.Instance.UpdateDebugDisplay(new string[] { string.Concat($"TRAIT OF NPC: {allTraits[allSpeakers[activeNPC].speakerTrait]}\n", string.Join(',', allEvs)) });
        */
        Debug.Log("COLLECTED THE FF MEMORIES:");
        Debug.Log(string.Join('/', npcMemory.Union(globalAndMap).ToList()));

        return npcMemory.Union(globalAndMap).Distinct().ToArray();
    }

    public static void UpdateProbabilitiesFromWholeMemory(string npc = "player")
    {
        // update the dialogue probabilities based on our current knowledge


        int[] evs = CollectMemories(npc);
        int[] traitarr = new int[evs.Length];
        int[] relarr = new int[evs.Length];


        if (activeNPC == "")
        {
            // set traitarr and relarr to be null
            traitarr = null;
            relarr = null;
        }
        else
        {
            for (int i = 0; i < evs.Length; i++)
            {
                traitarr[i] = allSpeakers[activeNPC].speakerTrait;
                relarr[i] = allSpeakers[activeNPC].RelationshipStatus();
            }

        }

        model.UpdateSpeakerDialogueProbs(evs,
                traitarr,
                relarr,
                ref allSpeakers[npc].currentPosteriors,
                ref allSpeakers[npc].currentDialogueCPT);
    }


    /// <summary>
    /// Uses the inference engine to infer the best possible line
    /// </summary>
    /// <param name="playerChoice">The lline chosen by player</param>
    /// <returns>DialogueLine.string dialogue selected by the engine</returns>
    public static string[] GetNPCLine(int buttonPressed=-1, int topChoiceIdx=0)
    {
        Debug.Log("==== NPC TURN ====");

        Debug.Log("Talking to: " + activeNPC);

        string playerChoiceStr = "n/a => start of dialogue";
        // if we have selected some choice...
        if (buttonPressed != -1)
        {
            // we add the internal index of the topmost choice displayed with the index of the button we selected
            int playerChoice = buttonPressed + topChoiceIdx;

            playerChoiceStr = playerChoices[playerChoice].dialogue;
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

        //UpdateProbabilitiesFromWholeMemory(activeNPC);

        // our selected npc line will be prevline -- it will be remembered.
        int lineId = model.SelectNPCLine(
            allSpeakers[activeNPC].speakerTrait,
            allSpeakers[activeNPC].RelationshipStatus(),
            allSpeakers[activeNPC].topics,
            mood,
            currentMap,
            allSpeakers[activeNPC].speakerArchetype,
            allSpeakers[activeNPC].currentPosteriors);

        Debug.Log("selected line: " + lineId);


        prevLine = LineDB[lineId];

        // get the topic relevances for the debug
        GetRelatedTopicRelevances();
        
        // update debug window
        EventHandler.Instance.UpdateDebugDisplay(new string[] { $"\n======NPC TURN======\nplayer-selected line: {playerChoiceStr}\n", model.debugProbStr });

        // update player data and NPC data given acquired line of NPC
        UpdatePlayerData(prevLine);
        UpdateNPCData(prevLine);
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_START_CONVO] = 0.0;

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

        if (!prevLine.effect.exit)
        {
            Debug.Log("==== PLAYER TURN ====");
            // clear player lines
            playerChoices.Clear();


            // select some player lines.
            List<int> lineIds = model.SelectPlayerLines(
                allSpeakers[activeNPC].speakerTrait,
                allSpeakers[activeNPC].RelationshipStatus(),
                allSpeakers[activeNPC].topics,
                mood,
                currentMap,
                allSpeakers[activeNPC].speakerArchetype,
                allSpeakers[DirectorConstants.PLAYER_STR].currentPosteriors);

            foreach (int i in lineIds)
            {
                playerChoices.Add(LineDB[i]);
            }

            // update the relevant topics
            GetRelatedTopicRelevances(npcTurn: false);

            // update debug window -- WE INCLUDE THE MINIMUM THRESHOLD
            EventHandler.Instance.UpdateDebugDisplay(new string[] { $"======PLAYER TURN======\nMINIMUM THRESHOLD BASED ON HIGHEST VALUE: {model.minThreshold.ToString("F16").TrimEnd('0')}\n", model.debugProbStr });

            var retLine = new List<string>();
            // replace active placeholder with name of active npc
            foreach (string dialogue in playerChoices.Select(s => s.dialogue).ToList())
            {
                if (dialogue.Contains(NAME_PLACEHOLDER_ACTIVE))
                {
                    dialogue.Replace("{" + NAME_PLACEHOLDER_ACTIVE + "}", allSpeakers[activeNPC].displayName);

                }
                retLine.Add(dialogue);
            }

            // from the selected player choices, we return the dialogue TEXT only.
            return retLine;
        }

        return null;
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

        // we remember the line that it's already chosen before and if it's not a generic convo starter
        if (!(line.relatedTopics[0].Equals(DirectorConstants.TOPIC_START_CONVO)))
        {
            AddToShortTermMemory(line);
        }

        UpdateSpeakerData(line);
    }

    public static void UpdateSpeakerData(DialogueLine line)
    {
        mood = line.ResponseStrToInt();

        // access reationship with active npc and update it.
        UpdateRelationship(line.effect.relationshipEffect);
        
        // update topic relevance table
        // set topic relevance to be the maximum.
        if(line.effect.makeMostRelevantTopic != "" || line.effect.makeMostRelevantTopic != null)
        {
            foreach (string topic in line.effect.makeMostRelevantTopic.Split('/'))
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
    }

    public static void UpdateRelationship(int value)
    {
        allSpeakers[activeNPC].relWithPlayer += value;
    }
    

    /// <summary>
    /// Topics that aren't related to the dialogue of choice will be updated to degrade.
    /// </summary>
    /// <param name="choice"></param>
    public static void UpdateTopics(DialogueLine choice)
    {
        // all topics that are not in related topics of the line and not in the topics to makee most relevant
        // will be reduced value only if di siya 0.
        foreach (string topic in allSpeakers[activeNPC].topics.Keys.Where(t => !choice.relatedTopics.Contains(t) && !choice.effect.makeMostRelevantTopic.Split('/').Contains(t) && allSpeakers[activeNPC].topics[t] != 0).ToList())
        {
            allSpeakers[activeNPC].topics[topic] -= 0.125;   // reduce by .25
            // check if the topic value is marked as closed or if it's less than base value na. if yes, we give it a minimum value
            if (allSpeakers[activeNPC].topics[topic] <= (double) DirectorConstants.TOPIC_RELEVANCE_CLOSE)
                allSpeakers[activeNPC].topics[topic] = (double)0.1;
        }

        // the bookends (start and end convo topics) will always be 0 or close
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_START_CONVO] = 0.0;
        allSpeakers[activeNPC].topics[DirectorConstants.TOPIC_END_CONVO] = DirectorConstants.TOPIC_RELEVANCE_CLOSE;
    }

    public static void CloseTopicForAll(string topic)
    {
        foreach(string s in allSpeakers.Keys)
        {
            allSpeakers[s].topics[topic] = DirectorConstants.TOPIC_RELEVANCE_CLOSE;
        }

        Debug.Log("topics are successfully closed");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speaker"> speaker ID or object id</param>
    /// <param name="eventId">event string</param>
    public static void AddToSpeakerMemory(string speaker, string eventId)
    {
        // we check if the memory already contains our said event, and if the event exists in our event db
        if (allEvents.ContainsValue(eventId))
        {
            
            int[] traitarr = new int[] { allSpeakers[speaker].speakerTrait };
            int[] relarr = new int[] { allSpeakers[speaker].RelationshipStatus() };

            if(traitarr[0] == -1)
            {
                traitarr[0] = NumKeyLookUp(DirectorConstants.NONE_STR, fromTraits: true);
            }

            // is negative only if player
            if (relarr[0] == -1)
            {
                if(activeNPC == null || activeNPC == "")
                {
                    relarr = null;
                }
                else
                {
                    relarr[0] = allSpeakers[activeNPC].RelationshipStatus();
                }
            }

            // add to memories if wla pa sa memories
            if(!(allSpeakers[speaker].speakerMemories.Contains(eventId)))
                allSpeakers[speaker].speakerMemories.Add(eventId);

            // update probability table of the speaker in question
            model.UpdateSpeakerDialogueProbs(new int[] { NumKeyLookUp( eventId, refDict: allEvents) }, 
                traitarr, 
                relarr, 
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

        if (map == DirectorConstants.DEFAULT_MAP)
        {
            // get the current map name and replace old map of we use default option
            map = currentMap;
        }

        if(eventId == -1)
        {
            Debug.Log("event " + e + " does not exist in allEvents");
            return;
        }

        if( map == "global")
        {
            if (!(globalEvents.Contains(eventId)))
                globalEvents.Add(eventId);

            // update the probability table of ALL speakers in allspeaker list since global
            foreach (Speaker s in allSpeakers.Values)
            {
                int[] traitarr = new int[] { s.speakerTrait };
                int[] relarr = new int[] { s.RelationshipStatus() };

                if (traitarr[0] == -1)
                {
                    traitarr[0] = NumKeyLookUp(DirectorConstants.NONE_STR, fromTraits: true);
                }

                // is negative only if player
                if (relarr[0] == -1)
                {
                    if (activeNPC == null || activeNPC == "")
                    {
                        relarr = null;
                    }
                    else
                    {
                        relarr[0] = allSpeakers[activeNPC].RelationshipStatus();
                    }
                }
                
                model.UpdateSpeakerDialogueProbs(new int[] { eventId },
                    traitarr,
                    relarr,
                    ref s.currentPosteriors,
                    ref s.currentDialogueCPT);
            }
        }
        else if (map != "global")
        {
            if(!(mapEvents[map].Contains(eventId)))
                mapEvents[map].Add(eventId);

            // update the probability table of ALL speakers whose spawn location is the current scene
            foreach (Speaker s in allSpeakers.Values.Where(sp => sp.spawnLocation == map))
            {
                int[] traitarr = new int[] { s.speakerTrait };
                int[] relarr = new int[] { s.RelationshipStatus() };

                if (traitarr[0] == -1)
                {
                    traitarr[0] = NumKeyLookUp(DirectorConstants.NONE_STR, fromTraits: true);
                }

                // is negative only if player
                if (relarr[0] == -1)
                {
                    if (activeNPC == null || activeNPC == "")
                    {
                        relarr = null;
                    }
                    else
                    {
                        relarr[0] = allSpeakers[activeNPC].RelationshipStatus();
                    }
                }
                
                model.UpdateSpeakerDialogueProbs(new int[] { eventId },
                    traitarr,
                    relarr,
                    ref s.currentPosteriors,
                    ref s.currentDialogueCPT);
            }

            Debug.Log("added event to map: "+map);
        }

        Debug.Log($"Event {e} added");
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

    public static void GetRelatedTopicRelevances(bool npcTurn = true)
    {
        Dictionary<string, double> relatedTopics = new Dictionary<string, double>();

        debugTopics = "";
        //if not npc turn, we get all the topics of the player choices we have.
        if (!npcTurn)
        {
            // memories of player
            debugTopics += "PLAYER MEMORIES:\n";
            CollectMemories().ToList().ForEach(id => debugTopics += $"{allEvents[id]} / ");

            // get the related topics of all the chosen lines and add their associated value from speaker into the relatedtopics dict
            foreach(string topic in playerChoices.SelectMany(line => line.relatedTopics))
            {
                if(!relatedTopics.ContainsKey(topic))
                    relatedTopics.Add(topic, allSpeakers[activeNPC].topics[topic]);
            }
            
        }
        else
        {
            // memories of npc
            debugTopics += "NPC MEMORIES:\n";
            CollectMemories(activeNPC).ToList().ForEach(id => debugTopics += $"{allEvents[id]} / ");

            foreach (string topic in prevLine.relatedTopics)
            {
                if (!relatedTopics.ContainsKey(topic))
                    relatedTopics.Add(topic, allSpeakers[activeNPC].topics[topic]);
            }
        }

        debugTopics += "\nTOPIC RELEVANCE:";
        foreach(KeyValuePair<string, double> pair in allSpeakers[activeNPC].topics) // previously relatedtopics
        {
            debugTopics += $"topic: {pair.Key} | value: {pair.Value}\n";
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
