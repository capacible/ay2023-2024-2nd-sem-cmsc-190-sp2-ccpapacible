using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

/* HOW THIS WILL GO:
 * (1) we convert our csv data into an XML file
 * (2) at the start of the game, we load up the XML file
 */

public class DialogueLine
{
    /*
     * FOR INFERENCE
     */
     
    // a list of all related events that precede this line (character memory, global, map)
    [XmlArray("relatedEvents", IsNullable = true), XmlArrayItem("relatedEventsItem")]
    public string[] relatedEvents;      
    
    public string receiver;               // this will weed out all the lines to pick from
    public string speakerId;                // the specific speaker this line applies to.

    // locations needed for the line (also used to filter like the receiver)
    [XmlArray("locations", IsNullable = true), XmlArrayItem("locationsItem")]
    public string[] locations;

    // prereq relationship
    public string relPrereq;
    // prereq trait
    [XmlArray("trait", IsNullable =true), XmlArrayItem("traitItem")]
    public string[] traitPrereq;

    /* 
     * UTILITY FUNCTION RELATED
     */

    // tone weights
    // the weights determine if the line will be said given the current tone of the conversation
    //      weights range from 0 to 2;
    //      we get the tone of the convo and access the tonal weight of the dialogue line in question to be used in utility fxn
    public int posWeight;
    public int negWeight;

    // if the line is already said.
    [XmlIgnore]
    public bool isSaid = false;

    // a list of topics that determine relevance of line
    [XmlArray("relatedTopics", IsNullable = true), XmlArrayItem("relatedTopicsItem")]
    public string[] relatedTopics;

    /*
     * OUTPUT
     */
    public string dialogue;                            // actual line to display
    public string responseTone;                        // the overall effect of the line to the tone of the convo (pos, neut, neg)
    public string portrait;                            // portrait associated w the line said; ano papakita sa ui.

    [XmlElement("DialogueEffect")]
    public DialogueEffect effect;        // effect to run

    /// <summary>
    /// Converting the lines' response tone into its integer equivalent
    /// </summary>
    /// <returns></returns>
    public int ResponseStrToInt()
    {
        if (responseTone == "pos")
            return 1;
        else if (responseTone == "neg")
            return -1;

        return 0;
    }
}

/// <summary>
/// This is used to load all lines from the XML.
/// </summary>
[XmlRoot("LineCollection")]
public class DialogueLineCollection
{
    [XmlArray("DialogueLines"), XmlArrayItem("DialogueLine")]
    public DialogueLine[] DialogueLines;
    
    public static Dictionary<int, DialogueLine> LoadAll(string[] paths)
    {
        Dictionary<int, DialogueLine> all = new Dictionary<int, DialogueLine>();

        int count = 0;

        foreach(string path in paths)
        {
            UnityEngine.Debug.Log("loading dialogue at path: " + path);
            // we load individually then access each individual dialogue line to add to the final collection.
            foreach(DialogueLine line in XMLUtility.LoadFromPath<DialogueLineCollection>(path).DialogueLines)
            {
                UnityEngine.Debug.Log("loaded this line: " + line.receiver);
                all.Add(count, line);
                count++;
            }
        }

        return all;
    }
}