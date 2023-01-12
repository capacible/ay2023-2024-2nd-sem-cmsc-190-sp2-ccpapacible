using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In charge of determining the lines to use.
/// </summary>
public static class Director
{
    private static Dictionary<string, DialogueLine> lineDB;

    // tracking events and speakers.
    private static string activeNPC;
    private static string currentMap;
    private static List<string> globalEvents;
    private static Dictionary<string, List<string>> mapEvents;
    private static Dictionary<string, Speaker> allSpeakers;
    private static Dictionary<string, Speaker> fillerCharDefault; // default filler character speakers we can duplicate when adding a new filler char
    private static DialogueLine prevLine;

    // public static Inference engine

    /// <summary>
    /// Loads all the lines from SQLite DB into the Dictionary upon startup.
    /// </summary>
    public static void LoadLines()
    {
        lineDB = new Dictionary<string, DialogueLine>();
    }

    // if non-filler, create a new speaker class and add to all speakers
    // else, create a new speaker and add to fillerCharDefault.
    public static void LoadSpeakers()
    {

    }

    /// <summary>
    /// This adds a filler speaker of a given archetype if it is not yet in the speakers.
    /// </summary>
    public static void AddFillerSpeaker()
    {

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
    /// <returns>DialogueLine selected by the engine</returns>
    public static DialogueLine GetNPCLine()
    {
        DialogueLine prevLine = new DialogueLine();

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
    public static List<DialogueLine> GetPlayerLines(List<string> globalEvents, List<string> mapEvents, Speaker activeNPC, Speaker player)
    {
        List<DialogueLine> acquiredLines = new List<DialogueLine>();

        return acquiredLines;
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
