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
    private const string JONATHAN_ENDING = "Jonathan_ending";
    private const string CASS_ENDING = "Cass_ending";
    private const string DIRECTOR_ENDING = "Director_ending";

    // scene names
    private const string TITLE_SCREEN = "TitleScreen";
    private const string NARRATION_SCENE = "_NarrationScene";
    // other
    private const string ACCUSED_DONE = "DONE_ACCUSE";
    // constant tags so we don't forget the specific tag.
    private const string DISPLAY_NAME_TAG = "display_name";
    private const string PORTRAIT_EMOTE_TAG = "portrait";
    private const string ARCHETYPE_TAG = "archetype";
    // effect tags
    private const string ACCUSE_TAG = "accuse";
    private const string MODIFY_REL_TAG = "effect_modify_rel";
    private const string ADD_TO_GLOBAL_TAG = "effect_add_to_global";
    private const string ADD_EVENT_TO_PLAYER_TAG = "effect_add_to_player";  // add to player memory
    private const string SET_REL_VALUE_TAG = "effect_set_rel";              // sets the relationship value
                                                                            // tag:archetype!value
    private const string SET_INK_MANAGER_TAG = "active_ink";

    public static bool isActive;

    private static string prevStory;
    private static string prevStoryFname = "";
    // we store the in-ink index of the top 3 choices, so that we can accurately remember which choice
    // was displayed.
    private static int[] displayedIndices = new int[3];

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
        { ACCUSE_TAG, "false" },
        { SET_INK_MANAGER_TAG, "" },
        { ADD_TO_GLOBAL_TAG, "" }
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
        isActive = true;
        
        if (prevStoryFname.Equals(inkJSON.name))
        {
            // we load the previous story if same yung name ng previous sa iloload sana.
            currentDialogue.state.LoadJson(prevStory);
        }
        else
        {
            prevStoryFname = inkJSON.name;

            // setting the current story
            currentDialogue = new Story(inkJSON.text);
        }


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
            string varName = memory.Replace(':', '_');  // returns original ata if there is no instance of :

            if (currentDialogue.variablesState.Contains(varName))
            {
                // replace the memory with underscore then set the variable as true
                currentDialogue.variablesState[varName] = true;
            }
        }
    }

    public static bool ChoicesAvailable()
    {
        if (currentDialogue.currentChoices.Count > 0)
            return true;

        return false;
    }

    /// <summary>
    /// Returns an NPC line.
    /// </summary>
    /// <returns>The line to be spoken, as well as the image sprite.</returns>
    public static string[] NPCLine()
    {

        currentDTags[ADD_EVENT_TO_PLAYER_TAG] = ""; // reset
        currentDTags[PORTRAIT_EMOTE_TAG] = "neutral";
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
                Debug.Log(currentDialogue.variablesState["MUST_SOLVE_MYSTERY"]);

            } while (!ChoicesAvailable() && currentDialogue.canContinue 
                && (currentDTags.All(pair => currentDialogue.currentTags.Contains(pair.Key.Concat(pair.Value)))) );
            
            // we join all the acquired lines.
            return new string[] { string.Join("\n", lines),  currentDTags[PORTRAIT_EMOTE_TAG], currentDTags[ARCHETYPE_TAG] };
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

            // selects 3 choices that ARE NOT EMPTY through their index
            displayedIndices = choices.Where(choice => choice.text != "").Select(choice => choice.index).Take(3).ToArray();

            return choices.Where(choice => displayedIndices.Contains(choice.index)).Select(choice => choice.text).Take(3).ToList();
        }

        displayedIndices = new int[] { 0, 1, 2};

        // else we return the whole thing
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
        currentDTags = new Dictionary<string, string>
        {
            { DISPLAY_NAME_TAG, "" },
            { PORTRAIT_EMOTE_TAG, "" },
            { ADD_EVENT_TO_PLAYER_TAG, "" },
            { MODIFY_REL_TAG, "" },
            { SET_REL_VALUE_TAG, "" },
            { ARCHETYPE_TAG, "" },
            { ACCUSE_TAG, "false" },
            { SET_INK_MANAGER_TAG, "true" },
            { ADD_TO_GLOBAL_TAG, "" }
        };


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
            if (currentDTags[ADD_EVENT_TO_PLAYER_TAG]!="")
            {
                string[] split = currentDTags[ADD_EVENT_TO_PLAYER_TAG].Split('_');
                string toAdd = string.Join(':', split);

                // add the said event to the player's memory
                Director.AddToSpeakerMemory(
                    DirectorConstants.PLAYER_STR,
                    toAdd
                );

                Debug.Log("added to player memories: " + toAdd);
            }

            // add an event globally
            if (currentDTags[ADD_TO_GLOBAL_TAG]!="")
            {
                string[] split = currentDTags[ADD_TO_GLOBAL_TAG].Split('_');
                string toAdd = string.Join(':', split);
                // add the said event to the player's memory
                Director.AddEventString(toAdd);

                Debug.Log("added to global events: " + toAdd);
            }

            // set relationship to specific value
            if (currentDTags[SET_REL_VALUE_TAG]!="")
            {
                // 0 is the archetype, 1 is the effect value
                string[] effectVal = currentDTags[SET_REL_VALUE_TAG].Split('!');

                // modifies the value of the archetype
                if (int.TryParse(effectVal[1], out int value))
                {
                    Director.speakerDefaults[effectVal[0]].relWithPlayer = value;

                    Debug.Log("relationship with " + effectVal[0] + " has been modified to " + value);
                }
            }

            // modify (+/-) relationship effect
            if (currentDTags[MODIFY_REL_TAG]!="")
            {
                string[] effectVal = currentDTags[MODIFY_REL_TAG].Split('!');

                if(int.TryParse(effectVal[1], out int mod))
                {
                    Director.speakerDefaults[effectVal[0]].relWithPlayer += mod;
                    Debug.Log("relationship with " + effectVal[0] + " has been modified by " + mod);
                }
            }
            
            if (bool.TryParse(currentDTags[SET_INK_MANAGER_TAG], out bool active))
                isActive = active;
            else
                Debug.LogWarning("The value for " + SET_INK_MANAGER_TAG + " tag is invalid.");
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
        // get the ACTUAL CHOICE INDEX from selection
        int actualChoiceIdx = displayedIndices[selection];

        // sets the new path of the "story" based on ur choice
        currentDialogue.ChooseChoiceIndex(actualChoiceIdx);

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
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, CASS_ENDING, TITLE_SCREEN });
        }
        else if (selection == 1)
        {
            // jonathan
            // load narration scene
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, JONATHAN_ENDING, TITLE_SCREEN });
        }
        else if ( selection == 2)
        {
            // jonathan
            // load narration scene
            EventHandler.Instance.LoadUi(NARRATION_SCENE, new object[] { false, DIRECTOR_ENDING, TITLE_SCREEN });
        }
    }

    public static void Deactivate()
    {
        isActive = false;

        currentDialogue.state.ForceEnd();
        currentDialogue.state.GoToStart();

        // save the storystate
        prevStory = currentDialogue.state.ToJson();
        
    }
}
