using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.Linq;

/// <summary>
/// Script that is like the director; only it uses Ink to get lines and stuff.
/// Called explicitly by the eventhandler to figure out wtf the player or characters will say.
/// </summary>
public static class InkDialogueManager
{
    // constant tags so we don't forget the specific tag.
    private const string DISPLAY_NAME_TAG = "display_name";
    private const string PORTRAIT_EMOTE_TAG = "portrait";
    private const string ADD_EVENT_TO_PLAYER_TAG = "add_to_player";
    private const string ARCHETYPE_TAG = "archetype";

    public static bool isActive;

    // holds npc display name and all other tags needed
    // initialize to hold no values for all tags.
    public static Dictionary<string, string> currentDTags = new Dictionary<string, string>
    {
        { DISPLAY_NAME_TAG, "" },
        { PORTRAIT_EMOTE_TAG, "" },
        { ADD_EVENT_TO_PLAYER_TAG, "" },
        { ARCHETYPE_TAG, "" }
    };

    public static Story currentDialogue;   // holds our current dialogue.
    public static bool inDialogue;         // if the game is currently in an ink dialogue

    public static string ActiveNPCDisplayName()
    {
        return currentDTags[DISPLAY_NAME_TAG];
    }
    
    /// <summary>
    /// Called by the event handler at the beginning of a dialogue trigger.
    /// </summary>
    /// <param name="inkJSON"></param>
    public static string[] StartDialogue(TextAsset inkJSON)
    {
        // setting the current story
        currentDialogue = new Story(inkJSON.text);

        // initialize the variable states of each dialogue
        InitVariables();

        // returns the starting NPC line and the portrait result (string form)
        return NPCLine() ;
    }

    /// <summary>
    /// sets the values of each variable in the ink file according to the player memories
    /// </summary>
    public static void InitVariables()
    {
        // we iterate through each memory of the player
        foreach(string memory in Director.allSpeakers[DirectorConstants.PLAYER_STR].speakerMemories)
        {
            if (currentDialogue.variablesState.Contains(memory))
            {
                string varName = memory.Replace(':', '_');  // returns original ata if there is no instance of :
                // replace the memory with underscore then set the variable as true
                currentDialogue.variablesState[varName] = true;
            }
        }
    }

    /// <summary>
    /// Returns an NPC line.
    /// </summary>
    /// <returns>The line to be spoken, as well as the image sprite.</returns>
    public static string[] NPCLine()
    {
        currentDTags[ADD_EVENT_TO_PLAYER_TAG] = ""; // reset
        if (currentDialogue.canContinue)
        {
            List<string> lines = new List<string>();

            // parsing the tags of current line and getting the line itself
            do
            {
                // while the current point of the story is not yet with choices, we do this
                // we ensure that this is done at least once (if kunwari doing continue() already follows with choices.)
                lines.Add(currentDialogue.Continue());
                ParseTags();

            } while (currentDialogue.currentChoices.Count == 0 && currentDialogue.canContinue);

            // add certain event to player memory
            if(currentDTags[ADD_EVENT_TO_PLAYER_TAG] != "")
            {
                Director.AddToSpeakerMemory(DirectorConstants.PLAYER_STR, currentDTags[ADD_EVENT_TO_PLAYER_TAG]);
            }

            // we join all the acquired lines.
            return new string[] { string.Join("\n", lines),  currentDTags[PORTRAIT_EMOTE_TAG] };
        }

        return new string[] { "", currentDTags[PORTRAIT_EMOTE_TAG], currentDTags[ARCHETYPE_TAG] };
    }

    /// <summary>
    /// Returns a list of player choices. If the choices are greater than three, we get the first 3 choices
    /// </summary>
    /// <returns></returns>
    public static List<string> GetPlayerChoices()
    {
        List<Choice> choices = currentDialogue.currentChoices;
        
        if(choices.Count > 3)
        {
            Debug.LogWarning("Too many choices, we're gonna cut off to the first 3 choices only.");

            // selects 3 string choices.
            return choices.Select(choice => choice.text).Take(3).ToList();
        }

        // else we return the whole thing as string.
        return choices.Select(choice => choice.text).ToList();
    }

    /// <summary>
    /// Gets the tags and returns them,
    /// </summary>
    /// <returns></returns>
    public static void ParseTags()
    {
        // if empty...
        if (currentDialogue.currentTags.Count == 0)
        {
            Debug.LogWarning("NPC Dialogue has no tags.");
        }
        else
        {

            foreach (string tag in currentDialogue.currentTags)
            {
                // we get the tag in question, separate it by the colon(:)
                string[] tags = tag.Split(":");

                // we first have to check if it's a valid tag.
                if (currentDTags.ContainsKey(tags[0]))
                {
                    // access the dict entry of the tag and overwrite its old value with the new one.
                    currentDTags[tags[0]] = tags[1];
                }
                else
                {
                    Debug.LogWarning("No such tag as " + tags[0] + ", maybe you misspelled it or did not define it?");
                }

            }

        }
    }

    /// <summary>
    /// Given the index of the player's selection, we move onto the part of the story that corresponds to that choice
    /// then returns the NPC line in response to that choice.
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    public static string[] BranchOutGivenChoice(int selection)
    {
        // sets the new path of the "story" based on ur choice
        currentDialogue.ChooseChoiceIndex(selection);

        // returns the response.
        return NPCLine();
    }
}
