using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WINDOW
{
    DIALOGUE,
    MESSAGE
}


// in charge of showing the dialogue
public class DialogueUi : MonoBehaviour
{
    // ui elements
    public Canvas canvas;
    public Image textBox;
    public Image charPortrait;
    public Text dialogueText;
    public Text charName;
    public Button[] choices;
    public Button nextButton;
    public Animator anim;
    public Sprite playerPortrait;

    [Header("Probability Debugging")]
    public GameObject debugBg;
    public GameObject topicListBg;
    public Canvas debugCanvas;
    public TextMeshProUGUI debugTxt;
    public TextMeshProUGUI topicListTxt;

    // internal elements
    private int maxChoiceCount;         // acquired at initialization from how many buttons we added @ editor.
    private Text[] choiceText;          // used to directly reference the Text component of the choices buttons (para di na mag getComponent)
    private List<DialogueLine> playerChoices;
    private Queue<string> retLine;           // the line returned by the manager/director.

    // list of portraits that current npc will use.
    private List<Sprite> portraitList;
    private Dictionary<string, Sprite[]> npcPortraits; // string display name of npc data, and the various sprites
    private string currentArchetype;


    void Awake()
    {
        // subscriptions
        EventHandler.StartDialogue += ShowDialogueWindow;
        EventHandler.FoundNPCLine += ShowNPCDialogue;
        EventHandler.FoundPlayerLines += ShowPlayerChoices;
        EventHandler.DisplayNewDebugInfo += UpdateDebugText;

        Init();
    }

    private void OnDisable()
    {
        EventHandler.StartDialogue -= ShowDialogueWindow;
        EventHandler.FoundNPCLine -= ShowNPCDialogue;
        EventHandler.FoundPlayerLines -= ShowPlayerChoices;
        EventHandler.DisplayNewDebugInfo -= UpdateDebugText;
    }

    private void Init()
    {
        canvas.worldCamera = Camera.main;
        debugCanvas.worldCamera = Camera.main;

        maxChoiceCount = choices.Length;
        anim.SetBool("isActive", true);

        // acquire the text component of each button and initialize + deact.
        choiceText = new Text[maxChoiceCount];
        playerChoices = new List<DialogueLine>();

        for(int i = 0; i < maxChoiceCount; i++)
        {
            Debug.Log("initialized text of choice: " + i);
            choiceText[i] = choices[i].GetComponentInChildren<Text>();
            choices[i].gameObject.SetActive(false);

            // initialize player choice until max of 3 (exclude leave option)
            if (i < maxChoiceCount)
            {
                playerChoices.Add(new DialogueLine());
            }
        }
    }


    /// <summary>
    /// A makeshift constant dictionary that basically converts our more "verbose" portrait tags in writing
    /// into numerical versions that are more easily used in Unity spritesheets (spritesheet slices are named with _x at the end)
    /// </summary>
    /// <param name="portraitTag"></param>
    /// <returns></returns>
    public static int PortraitNum(string portraitTag)
    {
        switch (portraitTag)
        {
            case "neutral": return 0;
            case "happy": return 1;
            case "sad": return 2;
            case "angry": return 3;
        }

        Debug.LogWarning("No appropriate tag found");
        return 0;
    }

    /// <summary>
    /// Shows the textbox when the dialogue is triggered.
    /// </summary>
    /// <param name="obj"></param>
    public void ShowDialogueWindow(object[] obj)
    {
        Debug.Log("STARTING DIALOGUE");
        
        NPCData[] npc = (NPCData[]) obj[1];

        npcPortraits = new Dictionary<string, Sprite[]>();        
        foreach(NPCData n in npc)
        {
            npcPortraits.Add(n.speakerArchetype, n.dialoguePortraits.ToArray());
        }

        // for player-npc interaction
        if(npcPortraits.Count == 1)
        {
            // set our current sprite
            charPortrait.sprite = npcPortraits[npc[0].speakerArchetype][0];
            currentArchetype = npc[0].speakerArchetype;
        
            // animate dialogue box entrance
            anim.SetBool("isActive", true);
        }
    }
    

    /// <summary>
    // changes the text of the textbox to be NPC dialogue.
    /// </summary>
    /// <param name="data"> includes dialogue line, active npc display name, and (optional) portrait emotion </param>
    public void ShowNPCDialogue(object[] data)
    {
        string npcName = (string)data[0];
        string npcLine = (string)data[1];
        string emote = (string)data[2]; // is simple emption
        string archetype = (string)data[3]; // the archetype of the speaker.

        retLine = new Queue<string>(npcLine.Split("\n", System.StringSplitOptions.RemoveEmptyEntries));

        Debug.Log("Number of subsequent lines: " + retLine.Count);

        // set values
        charName.text = npcName;

        // convert last element of portraitFile name (w/c is the tag) to number and get the sprite that contains
        // that number.
        charPortrait.sprite = npcPortraits[archetype][PortraitNum(emote)];

        // we dequeue (fifo) the topmost line.
        // if we have no lines, conclude dialogue.
        if (retLine.TryDequeue(out string resultLine))
        {

            dialogueText.text = resultLine;
            Debug.Log(resultLine);
        }
        else
        {
            EventHandler.Instance.ConcludeDialogue();
            return;
        }
        
        // test log
        Debug.Log("set sprite to: " + charPortrait.sprite.name + " done");

        // show next
        nextButton.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// display the player choices in buttons
    /// </summary>
    /// <param name="allChoices">a list of choice values</param>
    public void ShowPlayerChoices(List<string> allChoices)
    {
        // set the buttons to be active
        // the last choice is always active though
        choices[maxChoiceCount - 1].gameObject.SetActive(true);

        Debug.Log("count of allchoices:" + allChoices.Count);

        // we change the text of the choice buttons to be the choicces we acquired from the manager.
        for (int i = 0; i < allChoices.Count; i++)
        {
            if (allChoices[i] == "")
                continue;

            Debug.Log(i);
            // set the button as active
            choices[i].gameObject.SetActive(true);
            // get the text of ith element of player choices, assign it to the ith button
            choiceText[i].text = allChoices[i];
        }
    }


    /// <summary>
    /// this will get invoked when a player makes a choice or clicks on a choice. each button (max 3) has their correspinding indices in player choices array
    /// </summary>
    /// <param name="index">the value of the button in the form of its index sa array</param>
    public void SelectChoice(int index)
    {
        // hide buttons
        foreach (Button b in choices)
        {
            b.gameObject.SetActive(false);
        }

        // change portrait to npc
        charPortrait.sprite = npcPortraits[currentArchetype][0];

        // when choice is selected, call event handler to trigger onDialogueSelected
        // we pass the index of the button selected w/c is representative of the order of the lines we return.
        EventHandler.Instance.DisplayNPCLine(index);
    }

    /// <summary>
    /// When clicking next button, we either get the next line acquired from director / ink, exit the dialogue
    /// if we meet an exit condition (neither director or ink is active), or clear dialogue and get player
    /// response lines.
    /// </summary>
    public void NextButton()
    {
        // As long as may laman pa, we keep dequeueing
        if(retLine.TryDequeue(out string line))
        {
            dialogueText.text = line;
        }
        else if (!Director.isActive && !InkDialogueManager.isActive)
        {
            Debug.Log("Exiting dialogue because neither Director nor InkDManager is active.");

            // if the director isn't active and ink dialogue manager is not active, it's obvious that there's nothing
            // more to say.
            EventHandler.Instance.ConcludeDialogue();
        }
        else if (InkDialogueManager.isActive && !InkDialogueManager.ChoicesAvailable())
        {
            // if no choices available, we get more npc lines
            EventHandler.Instance.DisplayNPCLine(-1);
        }
        else
        {
            // clear dialogue text and set new char name
            dialogueText.text = "";
            charName.text = "You";

            // set new portrait
            charPortrait.sprite = playerPortrait;

            // set the next button to be inactive
            nextButton.gameObject.SetActive(false);

            EventHandler.Instance.DisplayPlayerLines();
        }
    }

    public void ConcludeButton()
    {
        // animate out
        anim.SetBool("isActive", false);

        // inactive all buttons and the exit
        foreach (Button b in choices)
        {
            b.gameObject.SetActive(false);
        }

        EventHandler.Instance.ConcludeDialogue();
    }

    #region DEBUGGING DIRECTOR

    /// <summary>
    /// Opens debug window.
    /// </summary>
    public void DebugWindow()
    {
        // close
        if (debugBg.activeInHierarchy)
        {
            debugBg.gameObject.SetActive(false);
            topicListBg.gameObject.SetActive(false);
        }
        
        debugBg.gameObject.SetActive(true);
        topicListBg.gameObject.SetActive(true);

    }

    /// <summary>
    /// Updates the text.
    /// </summary>
    /// <param name="info"></param>
    public void UpdateDebugText(string[] info)
    {
        foreach(string i in info)
        {

            debugTxt.text = i + "\n";
        }

        // update the topic lis
        topicListTxt.text = Director.GetAllTopicRelevance();
    }
    #endregion
}
