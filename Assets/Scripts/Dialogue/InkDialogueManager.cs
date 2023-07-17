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
    // FILE NAMES
    private const string JONATHAN_ENDING = "";
    private const string CASS_ENDING = "";
    private const string DIRECTOR_ENDING = "";

    // scene names
    private const string TITLE_SCREEN = "";
    private const string NARRATION_SCENE = "";
    // other
    private const string ACCUSED_DONE = "DONE_ACCUSE";
    // constant tags so we don't forget the specific tag.
    private const string DISPLAY_NAME_TAG = "display_name";
    private const string PORTRAIT_EMOTE_TAG = "portrait";
    private const string ARCHETYPE_TAG = "archetype";
    // effect tags
    private const string ACCUSE_TAG = "accuse";
    private const string MODIFY_REL_TAG = "effect_modify_rel";
    private const string ADD_EVENT_TO_PLAYER_TAG = "effect_add_to_player";  // add to player memory
    private const string SET_REL_VALUE_TAG = "effect_set_rel";              // sets the relationship value
                                                                            // tag:archetype!value

    public static bool isActive;

    // holds npc display name and all other tags needed
    // initialize to hold no values for all tags.
    public static Dictionary<string, string> currentDTags = new Dictionary<string, string>
    {
        { DISPLAY_NAME_TAG, "" },
        { PORTRAIT_EMOTE_TAG, "" },
        { ADD_EVENT_TO_PLAYER_TAG, "" },
        { MODIFY_REL_TAG, "" },
        { SET_REL_VALUE_TAG, "" },
        { ARCHETYPE_TAG, "" },
        { ACCUSE_TAG, "false" }
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
        // reset current dtag for accuse
        // this is to ensure that when we go and talk to an npc using the inkdmanager, we dont accidentally trigger
        // the ending whiler responding to them.
        currentDTags[ACCUSE_TAG] = "false";

        // if empty...
        if (currentDialogue.currentTags.Count == 0)
        {
            Debug.LogWarning("NPC Dialogue has no tags.");
        }
        else
        {
            // parsing and modifying the current dialogue tags.
            foreach (string tag in currentDialogue.currentTags)
            {
                // we get the tag in question, separate it by the colon(:)
                string[] tagValPair = tag.Split(":", System.StringSplitOptions.RemoveEmptyEntries);

                // we first have to check if it's a valid tag.
                if (currentDTags.ContainsKey(tagValPair[0]))
                {
                    // access the dict entry of the tag and overwrite its old value with the new one.
                    currentDTags[tagValPair[0]] = tagValPair[1];
                }
                else
                {
                    Debug.LogWarning("No such tag as " + tagValPair[0] + ", maybe you misspelled it or did not define it?");
                }

            }

            // parse dtag effects
            //add certain event to player's memory
            if (currentDialogue.currentTags.Contains(ADD_EVENT_TO_PLAYER_TAG))
            {
                // add the said event to the player's memory
                Director.AddToSpeakerMemory(
                    DirectorConstants.PLAYER_STR,
                    currentDTags[ADD_EVENT_TO_PLAYER_TAG]
                );
            }

            // set relationship to specific value
            if (currentDialogue.currentTags.Contains(SET_REL_VALUE_TAG))
            {
                // 0 is the archetype, 1 is the effect value
                string[] effectVal = currentDTags[SET_REL_VALUE_TAG].Split('!');

                if (int.TryParse(effectVal[1], out int value))
                    Director.allSpeakers[effectVal[0]].relWithPlayer = value;
            }

            // modify (+/-) relationship effect
            if (currentDialogue.currentTags.Contains(MODIFY_REL_TAG))
            {
                string[] effectVal = currentDTags[MODIFY_REL_TAG].Split('!');

                if(int.TryParse(effectVal[1], out int mod))
                {
                    Director.allSpeakers[effectVal[0]].relWithPlayer += mod;
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

        // check if our previous npc dialogue triggers the ACCUSE phase.
        if(currentDTags[ACCUSE_TAG]== "true")
        {
            AccusePhase(selection);
        }

        // returns the response.
        return NPCLine();
    }

    /// <summary>
    /// Calls the narration scene given pur selected choice.
    /// </summary>
    /// <param name="selection"></param>
    public static void AccusePhase(int selection)
    {

        // accuse phase
        if (selection == 0)
        {
            // jonathan
            // load narration scene
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, JONATHAN_ENDING, TITLE_SCREEN });
        }
        else if (selection == 1)
        {
            // jonathan
            // load narration scene
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, CASS_ENDING, TITLE_SCREEN });
        }
        else if ( selection == 2)
        {
            // jonathan
            // load narration scene
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, DIRECTOR_ENDING, TITLE_SCREEN });
        }
    }
}
