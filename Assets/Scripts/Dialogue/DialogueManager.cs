using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private string activeNPC;
    private List<string> globalEvents;
    private Dictionary<string, List<string>> mapEvents;
    private Dictionary<string, Speaker> allSpeakers;
    private Dictionary<string, Speaker> fillerCharDefault; // default filler character speakers we can duplicate when adding a new filler char
    private string currentMap;
    //private Director DDirector;

    // saves the previous line
    private string prevLine;

    // Start is called before the first frame update
    void Start()
    {
        // dont destroy on load
        DontDestroyOnLoad(gameObject);

        LoadSpeakers();

        // subscribe to eventhandler delegates
        EventHandler.OnDialogueTrigger += StartConversation;
        EventHandler.OnNextDialogue += PlayerTalk;
        EventHandler.OnDialogueSelect += NPCRespond;
        EventHandler.OnFillerNPCSpawned += AddToSpeakers;
    }

    private void OnDisable()
    {
        EventHandler.OnDialogueTrigger -= StartConversation;
        EventHandler.OnNextDialogue -= PlayerTalk;
        EventHandler.OnDialogueSelect -= NPCRespond;
        EventHandler.OnFillerNPCSpawned -= AddToSpeakers;
    }

    /// <summary>
    /// loads speakers here.
    /// </summary>
    private void LoadSpeakers()
    {
        // if non-filler,
        // create a speaker class,
        // speaker id will be the speaker archetype also

        // if filler character, add them to fillerCharDefaultSpeakers.
    }

    /// <summary>
    /// When enterning a map for the first time
    /// </summary>
    /// <param name="npc"></param>
    private void AddToSpeakers(NPCData npc)
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
    /// 
    /// </summary>
    /// <param name="obj"> includes obj[0] NPC string, and obj[1] current map string </param>
    void StartConversation(object[] obj)
    {
        // set the active NPC to be npc id received
        activeNPC = obj[1].ToString();
        currentMap = obj[2].ToString();

        Debug.Log("Conversation began with" + activeNPC);

        NPCTalk();
    }

    void NPCTalk()
    {
        // request NPC line from director
        // prevLine = DDirector.GetNPCLine(new object[] {});

        prevLine = "test";

        //UpdatePlayerData(npcLine);

        // display the dialogue.
        // parameters to send: prevLine, activeNPC
        EventHandler.current.DisplayDialogue("npc", new object[] { prevLine, activeNPC });
    }

    void NPCRespond(string line)
    {
        // updates the NPC data based on the dialogue line used

        //UpdateNPCData(npcLine);

        // calls talk again, to request a new NPC line, update the player data from the npc line, and display the dialogue.
        NPCTalk();

    }

    void PlayerTalk()
    {
        // request player choices from the director, given acquired DialogueLine from previous code
        // DialogueLine playerLines = DDirector.GetPlayerChoices(new object[] {}, prevLine)
        string[] playerLines = new string[] { "hello!", "whats up?", "I'm dead inside" };

        // call eventhandler to display
        EventHandler.current.DisplayDialogue("choice", new object[] { playerLines });

    }

    // update npc-specific data
    void UpdateNPCData(DialogueLine line)
    {

        // get current goal
        SpeakerGoal goal = allSpeakers[activeNPC].speakerGoals.Find(g => g.isActive == true);

        // check if the effect goaltoachieved is same as our active goal. if yes we set the active goal to achieved.
        if (line.effect.goalToAchieved == goal.id)
        {
            goal.isAchieved = true;
            goal.isActive = false;
        }

        // check if the effect goaltoactive is the same as our active goal. if it isnt that means we have to activate a different goal
        if (line.effect.goalToActive != goal.id)
        {
            // if current goal is not yet achieved, we pause the current goal
            if (goal.isAchieved != false)
            {
                goal.isActive = false;
                goal.isPaused = true;
            }

            // get the requested goal with the goaltoactive id
            SpeakerGoal newGoal = allSpeakers[activeNPC].speakerGoals.Find(g => g.id.Equals(line.effect.goalToActive));

            // if the found goal is not yet achieved then we can reactivate it. if not, we wont reactivate it, we now have no active goal.
            if (!newGoal.isAchieved)
            {
                // if our newgoal is currently paused, stop
                if (newGoal.isPaused == true)
                {
                    newGoal.isPaused = false;
                }
                newGoal.isActive = true;
            }
        }

        foreach (string m in line.effect.npc_toRemember)
        {
            if (!allSpeakers[activeNPC].speakerMemories.Contains(m))
            {
                allSpeakers[activeNPC].speakerMemories.Add(m);
            }
        }

        UpdateSpeakerData(line);
    }

    // update player specific data
    void UpdatePlayerData(DialogueLine line)
    {

        // get current goal
        SpeakerGoal goal = allSpeakers["player"].speakerGoals.Find(g => g.isActive == true);

        // check if the effect goaltoachieved is same as our active goal. if yes we set the active goal to achieved.
        if (line.effect.goalToAchieved == goal.id)
        {
            goal.isAchieved = true;
            goal.isActive = false;
        }

        // check if the effect goaltoactive is the same as our active goal. if it isnt that means we have to activate a different goal
        if (line.effect.goalToActive != goal.id)
        {
            // if current goal is not yet achieved, we pause the current goal
            if (goal.isAchieved != false)
            {
                goal.isActive = false;
                goal.isPaused = true;
            }

            // get the requested goal with the goaltoactive id
            SpeakerGoal newGoal = allSpeakers["player"].speakerGoals.Find(g => g.id.Equals(line.effect.goalToActive));

            // if the found goal is not yet achieved then we can reactivate it. if not, we wont reactivate it, we now have no active goal.
            if (!newGoal.isAchieved)
            {
                // if our newgoal is currently paused, stop
                if (newGoal.isPaused == true)
                {
                    newGoal.isPaused = false;
                }
                newGoal.isActive = true;
            }
        }

        foreach (string m in line.effect.player_toRemember)
        {
            if (!allSpeakers["player"].speakerMemories.Contains(m))
            {
                allSpeakers["player"].speakerMemories.Add(m);
            }
        }

        UpdateSpeakerData(line);
    }

    void UpdateSpeakerData(DialogueLine line)
    {
        // access reationship with active npc and update it.
        allSpeakers[activeNPC].relWithPlayer += line.effect.relationshipEffect;

        // add to global events
        foreach (string e in line.effect.addEventToGlobal)
        {
            // only add if the global events are in the list
            if (!globalEvents.Contains(e))
            {
                globalEvents.Add(e);
            }
        }

        // remove from global events
        foreach (string e in line.effect.removeEventFromGlobal)
        {
            globalEvents.Remove(e);
        }

        // add to map events
        foreach (KeyValuePair<string, List<string>> dict in line.effect.addEventToMap)
        {
            foreach (string e in dict.Value)
            {
                if (!mapEvents[dict.Key].Contains(e))
                {
                    // add the non existent event into the dictionary
                    mapEvents[dict.Key].Add(e);
                }
            }
        }

        // remove from map eventsforeach (KeyValuePair<string, List<string>> dict in line.effect.addEventToMap)
        foreach (KeyValuePair<string, List<string>> dict in line.effect.addEventToMap)
        {
            foreach (string e in dict.Value)
            {
                mapEvents[dict.Key].Remove(e);
            }
        }
    }
}
