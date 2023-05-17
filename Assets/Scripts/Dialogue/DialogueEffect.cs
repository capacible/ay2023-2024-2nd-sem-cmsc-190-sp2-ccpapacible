using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class DialogueEffect
{
    [XmlArray("addEventToGlobal", IsNullable = true), XmlArrayItem("addEventToGlobalItem")]
    public string[] addEventToGlobal;

    [XmlArray("addEventToMap", IsNullable = true), XmlArrayItem("addEventToMapItem")]
    public string[] addEventToMap;

    [XmlArray("addToNPCMemory", IsNullable = true), XmlArrayItem("addToNPCMemoryItem")]
    public string[] addToNPCMemory;

    [XmlArray("addToPlayerMemory", IsNullable = true), XmlArrayItem("addToPlayerMemoryItem")]
    public string[] addToPlayerMemory;

    [XmlElement("relationshipEffect")]
    public int relationshipEffect;

    // gawing most relevant topic yung topic given
    // maximum value for the topic is 2
    // minimum is 1 -- never 0
    [XmlElement("makeMostRelevantTopic")]
    public string makeMostRelevantTopic;

    // set topic value to 1.
    [XmlElement("closeTopic")]
    public string closeTopic;

    // exit the dialogue.
    [XmlElement("exit")]
    public bool exit;
}
