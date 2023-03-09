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
    [XmlArray("relatedEvents"), XmlArrayItem("relatedEventsItem")]
    public string[] relatedEvents;      
    
    public string receiver;               // this will weed out all the lines to pick from

    // list of possible relationship statuses
    [XmlArray("relPrereqs"), XmlArrayItem("relPrereqsItem")]
    public string[] relPrereqs;

    /* 
     * UTILITY FUNCTION RELATED
     */

    // tone weights
    // the weights determine if the line will be said given the current tone of the conversation
    //      weights range from 0 to 2;
    //      we get the tone of the convo and access the tonal weight of the dialogue line in question to be used in utility fxn
    public int posWeight;
    public int negWeight;
    public int neutWeight;

    // a list of topics that determine relevance of line
    [XmlArray("relatedTopics"), XmlArrayItem("relatedTopicsItem")]
    public string[] relatedTopics;

    /*
     * OUTPUT
     */
    public string dialogue;                                // actual line to display
    public string responseTone;                        // the overall effect of the line to the tone of the convo (pos, neut, neg)

    [XmlElement("DialogueEffect")]
    public DialogueEffect effect;        // effect to run
}

/// <summary>
/// This is used to load all lines from the XML.
/// </summary>
[XmlRoot("LineCollection")]
public class DialogueLineCollection
{
    [XmlArray("DialogueLines"), XmlArrayItem("DialogueLine")]
    public DialogueLine[] DialogueLines;

    public static DialogueLineCollection Load(string path)
    {
        var serializer = new XmlSerializer(typeof(DialogueLineCollection));

        using (var stream = new FileStream(Path.Combine(Application.dataPath, path), FileMode.Open))
        {
            return serializer.Deserialize(stream) as DialogueLineCollection;
        }
    }

    public static Dictionary<int, DialogueLine> LoadAll(string[] paths)
    {
        Dictionary<int, DialogueLine> all = new Dictionary<int, DialogueLine>();

        int count = 0;

        foreach(string path in paths)
        {
            // we load individually then access each individual dialogue line to add to the final collection.
            foreach(DialogueLine line in Load(path).DialogueLines)
            {
                all.Add(count, line);
                count++;
            }
        }

        return all;
    }
}