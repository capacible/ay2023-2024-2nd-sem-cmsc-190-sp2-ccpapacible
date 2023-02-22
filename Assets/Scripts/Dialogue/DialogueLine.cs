using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

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
    [XmlArray("relatedEvents"), XmlArrayItem("relatedEventsItem")]
    public List<string> relatedEvents = new List<string>();      
    
    public string receiver;               // this will weed out all the lines to pick from

    // list of possible relationship statuses
    [XmlArray("relPrereqs"), XmlArrayItem("relPrereqsItem")]
    public List<string> relPrereqs = new List<string>();

    /* 
     * UTILITY FUNCTION RELATED
     */

    // tone weights
    // the weights determine if the line will be said given the current tone of the conversation
    //      weights range from 0 to 2;
    //      we get the tone of the convo and access the tonal weight of the dialogue line in question to be used in utility fxn
    public int posWeight = 1;
    public int negWeight = 1;
    public int neutWeight = 1;

    // a list of topics that determine relevance of line
    [XmlArray("relatedTopics"), XmlArrayItem("relatedTopicsItem")]
    public List<string> relatedTopics = new List<string>();

    /*
     * OUTPUT
     */
    public string dialogue = "";                                // actual line to display
    public string responseTone = "neut";                        // the overall effect of the line to the tone of the convo (pos, neut, neg)

    [XmlElement("DialogueEffect")]
    public DialogueEffect effect = new DialogueEffect();        // effect to run
}

/// <summary>
/// This is used to load all lines from the XML.
/// </summary>
[XmlRoot("LineCollection")]
public class DialogueLineCollection
{
    [XmlArray("DialogueLines"), XmlArrayItem("DialogueLine")]
    public List<DialogueLine> DialogueLines = new List<DialogueLine>();
}