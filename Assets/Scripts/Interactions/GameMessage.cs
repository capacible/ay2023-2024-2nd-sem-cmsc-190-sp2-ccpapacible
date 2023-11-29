using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Generic message id references, for finding the correct message.
/// </summary>
public static class GENERIC_MSG_ID
{
    public static readonly string ITEM_PICKUP = "item_pickup";
    public static readonly string ITEM_DROP = "item_drop";
}

/// <summary>
/// Class for handling in-game messages that are not dialogue.
/// </summary>
public class GameMessage : MonoBehaviour
{
    public static readonly string INTERACTION_FILE_PATH = "XMLs/messages/interactionMsgs";
    
    // dictionary of our interaction messages
    // this includes generic and non-generic ones.
    public Dictionary<string, InteractionMessage> interactions;

    public Canvas gmCanvas;

    // ui for regular interaction message
    [Header("Interaction Msg")]
    public TextMeshProUGUI msgText;
    public Image msgBox;
    public Animator msgAnim;

    // ui for quick
    [Header("Quick Notif")]
    public TextMeshProUGUI quickText;
    public Image quickBox;
    public Fader notifFader;
    public float waitBeforeFadeOut;

    // remembering our current Interaction
    private string current;                             // for regular interaction.
    private Dictionary<string, string> currentTags;

    private void Start()
    {
        // dont destroy on load
        DontDestroyOnLoad(this);

        // set animation to false
        msgAnim.SetBool("isActive", false);

        // load the interaction messages
        interactions = InteractionContainer.LoadAllAsDict(INTERACTION_FILE_PATH);

        // subscribe to stuff
        EventHandler.InGameMessage += ShowMessage;
        SceneHandler.OnUiLoaded += Init;
    }

    private void OnDestroy()
    {
        EventHandler.InGameMessage -= ShowMessage;
    }

    private void Init(object[] obj)
    {
        gmCanvas.worldCamera = Camera.main;
        gmCanvas.transform.position = new Vector3(gmCanvas.transform.position.x, gmCanvas.transform.position.y, 2);
    }

    /// <summary>
    /// Shows the next content of a regular interaction (INTERACT_MSG)
    /// </summary>
    public void ShowNextContent()
    {
        int currentMsg = interactions[current].currentMsg;
        int currentContent = ++interactions[current].currentMsgContent;

        // while our current msg content is less than the total number of contents.
        if (currentContent < interactions[current].messageList[currentMsg].contents.Length)
        {
            // we get the new message 
            string message = ReplaceTags(interactions[current].messageList[currentMsg].contents[currentContent], currentTags);
            // change the text.
            msgText.text = message;
        }
        else
        {
            // exit and update the current message index.
            currentTags = null;

            interactions[current].currentMsg++;
            // reset the content count.
            interactions[current].currentMsgContent = 0;

            // exit
            msgAnim.SetBool("isActive", false);

            // set the parent msg box gameobject to inactive.
            msgBox.gameObject.SetActive(false);
            
            // eventhandler lets everyone know na tapos na yung interaction
            EventHandler.Instance.ConcludeInteraction(UiType.INTERACT_DIALOGUE);
        }
        
    }

    /// <summary>
    /// Display the message given some parameters
    /// </summary>
    /// <param name="msgParams">
    ///     interactId : the id of the interaction
    ///     msgTags     : dictionary describing the tags and what to replace w the tags in original interaction msg
    ///     enum MSG_TYPE: the message type if quick or narration type
    /// </param>
    private void ShowMessage(object[] msgParams)
    {
        UiType msgType = (UiType)msgParams[0];
        string id = msgParams[1].ToString();
        Debug.Log("msg id to show: " + id);
        Dictionary<string, string> msgTags = (Dictionary<string, string>)msgParams[2];

        // get the message
        string msg = GetMsgFirstContent(interactions[id], msgTags);


        // ui logic
        if (msgType == UiType.IN_BACKGROUND)
        {
            StartCoroutine(QuickUi(msg));
        }
        else if(msgType == UiType.INTERACT_DIALOGUE)
        {

            current = id;

            // activate the msg
            msgBox.gameObject.SetActive(true);

            // turn on animator
            msgAnim.SetBool("isActive", true);
            msgText.text = msg;
        }

    }

    /// <summary>
    /// Given some message parameters, we get the interaction based on the interaction id and get the message needed
    /// </summary>
    /// <returns>The correct message</returns>
    public string GetMsgFirstContent(InteractionMessage interaction, Dictionary<string, string> msgTags)
    {
        // if the interaction's current index already the last or beyond last index, reset index to 0.
        if(interaction.currentMsg >= interaction.messageList.Length - 1)
        {
            Debug.Log("Last msg of current interaction:");
            interaction.currentMsg = 0;
            
            // access the very first message
            return ReplaceTags(interaction.messageList[0].contents[0], msgTags);
        }

        Debug.Log("For this interaction, we are currently at message # " + interaction.currentMsg);
        // if not at last index, we access the message list
        string origMsg = interaction.messageList[interaction.currentMsg].contents[0];
                
        return ReplaceTags(origMsg, msgTags);
    }

    /// <summary>
    /// Replaces the tags from the original message with the values listed in the tags dictionary
    /// passed into the msg parameters
    /// </summary>
    /// <param name="origMsg">the text.</param>
    /// <param name="tags"></param>
    /// <returns>
    ///     the correct message.
    /// </returns>
    public string ReplaceTags(string origMsg, Dictionary<string, string> tags)
    {
        Debug.Log("Replacing the tags of the message: " + origMsg);
        if (tags == null || tags.Count == 0)
        {
            Debug.Log("no tags.");
            return origMsg;
        }
        
        // our message tags are put into a dictionary with numbered keys; each tagId found in the msg SHOULD
        // have a corresponding number in our tags parameter.
        foreach (string key in tags.Keys)
        {
            Debug.Log("tag key to replace: " + key);
            if (origMsg.Contains("{"+key+"}"))
            {
                // replace all instance of key with value
                origMsg = origMsg.Replace("{" + key + "}", tags[key]);
                Debug.Log("Replaced " + key + " with the tag: " + tags[key]);
            }
            else
            {
                Debug.LogWarning("Cannot find the key " + key + " in the received message parameters.");
            }
        }

        Debug.Log("New final msg: "+ origMsg);

        return origMsg;
    }

    /// <summary>
    /// shows the quick notif ui, then hides it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator QuickUi(string msg)
    {
        // change text then fade in.
        quickText.text = msg;
        notifFader.Fade(quickBox, 0.25, false);
        notifFader.Fade(quickText, 0.25, false);

        yield return new WaitForSeconds(waitBeforeFadeOut);

        // after some seconds fade out
        notifFader.Fade(quickBox, 0.25);
        notifFader.Fade(quickText, 0.25);
        quickText.text = "";

        yield return null;
    }
}

[XmlRoot("AllInteractions")]
public class InteractionContainer
{
    // an array of interactions
    [XmlArray("Interactions"), XmlArrayItem("Interaction")]
    public InteractionMessage[] interactionList;

    public static Dictionary<string, InteractionMessage> LoadAllAsDict(string path)
    {
        // load the interaction container.
        Dictionary<string, InteractionMessage> retDict = new Dictionary<string, InteractionMessage>();

        TextAsset icAsset = (TextAsset) Resources.Load(path);
        InteractionContainer ic = XMLUtility.LoadFromText<InteractionContainer>(icAsset);

        foreach(InteractionMessage i in ic.interactionList)
        {
            retDict.Add(i.id, i);
        }

        return retDict;
    }
}

/// <summary>
/// Any form of interaction with >= 1 messages shown to the player.
/// </summary>
public class InteractionMessage
{
    [XmlAttribute("interaction_id")]
    public string id;                   // id to remember the interaction by

    [XmlArray("Messages"), XmlArrayItem("Message")]
    public Message[] messageList;

    [XmlIgnore]
    public int currentMsg = 0;      // for remembering which message we are at if player leaves scene or interacts
                                      // with smth else

    [XmlIgnore]
    public int currentMsgContent; // the current content of our current message
}

/// <summary>
/// One message is representative of a sequence of strings that may require more than one click to finish
/// </summary>
public class Message
{
    
    [XmlArray("Contents"), XmlArrayItem("Content")]
    public string[] contents;
}

/// <summary>
/// Holds keys to the dictionary of values that we will replace in the text.
/// </summary>
public static class MSG_TAG_TYPE
{
    public static readonly string ITEM_NAME = "item_name";
    public static readonly string NPC_NAME = "npc_name";
}
