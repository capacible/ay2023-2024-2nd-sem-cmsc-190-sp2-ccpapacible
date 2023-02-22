using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class DialogueEffect
{
    [XmlArray("addEventToGlobal"), XmlArrayItem("addEventToGlobalItem")]
    public List<string> addEventToGlobal = new List<string>();

    [XmlArray("addEventToMap"), XmlArrayItem("addEventToMapItem")]
    public List<string> addEventToMap = new List<string>();

    [XmlArray("addToNPCMemory"), XmlArrayItem("addToNPCMemoryItem")]
    public List<string> addToNPCMemory = new List<string>();

    [XmlArray("addToPlayerMemory"), XmlArrayItem("addToPlayerMemoryItem")]
    public List<string> addToPlayerMemory = new List<string>();

    [XmlElement("relationshipEffect")]
    public int relationshipEffect = 0;

    // gawing most relevant topic yung topic given
    // maximum value for the topic is 2
    // minimum is 1 -- never 0
    [XmlElement("makeMostRelevantTopic")]
    public string makeMostRelevantTopic = "";

    // set topic value to 1.
    [XmlElement("closeTopic")]
    public string closeTopic = "";
}
