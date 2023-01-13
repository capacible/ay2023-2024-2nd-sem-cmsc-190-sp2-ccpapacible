using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// in charge of showing the dialogue
public class DialogueUi : MonoBehaviour
{
    // ui elements
    public Image textBox;
    public Image charPortrait;
    public Text dialogueText;
    public Text charName;
    public Button[] choices;
    public Button nextButton;
    public Animator anim;
    public Sprite playerPortrait;

    // internal elements
    private int maxChoiceCount;         // acquired at initialization from how many buttons we added @ editor.
    private Text[] choiceText;          // used to directly reference the Text component of the choices buttons (para di na mag getComponent)
    private List<DialogueLine> playerChoices;
    //private string[] playerChoices;     // the most recent acquired player choices from manager accessed when player clicks on one button.
    private Sprite currentPortrait;

    // Start is called before the first frame update
    void Start()
    {
        // subscriptions
        EventHandler.OnDialogueTrigger += ShowUi;
        EventHandler.OnDialogueFound += ShowNPCDialogue;
        EventHandler.OnPlayerLinesFound += ShowPlayerChoices;

        Init();
    }

    private void OnDisable()
    {
        EventHandler.OnDialogueTrigger -= ShowUi;
        EventHandler.OnDialogueFound -= ShowNPCDialogue;
        EventHandler.OnPlayerLinesFound -= ShowPlayerChoices;
    }

    private void Init()
    {
        maxChoiceCount = choices.Length;
        anim.SetBool("isActive", false);

        // acquire the text component of each button and initialize + deact.
        choiceText = new Text[maxChoiceCount];
        playerChoices = new List<DialogueLine>();

        for(int i = 0; i < maxChoiceCount; i++)
        {
            choiceText[i] = choices[i].GetComponentInChildren<Text>();
            choices[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the textbox when the dialogue is triggered.
    /// </summary>
    /// <param name="obj"></param>
    public void ShowUi(object[] obj)
    {
        currentPortrait = (Sprite) obj[0];
        charPortrait.sprite = currentPortrait;

        // animate dialogue box entrance
        anim.SetBool("isActive", true);

    }

    /// <summary>
    // changes the text of the textbox to be NPC dialogue.
    /// </summary>
    /// <param name="npcLine">the dialogue line</param>
    /// <param name="npcName">display name of active npc</param>
    public void ShowNPCDialogue(string npcName, DialogueLine npcLine)
    {

        charName.text = npcName; 
        dialogueText.text = npcLine.textLine;

        // show next
        nextButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// display the player choices in buttons
    /// </summary>
    /// <param name="allChoices">a list of choice values</param>
    public void ShowPlayerChoices(List<DialogueLine> allChoices)
    {
        // set the buttons to be active
        foreach (Button b in choices)
        {
            b.gameObject.SetActive(true);
        }

        // we change the text of the choice buttons to be the choicces we acquired from the manager.
        for (int i = 0; i < allChoices.Count; i++)
        {
            Debug.Log(i);
            // get the text of ith element of player choices, assign it to the ith button
            choiceText[i].text = allChoices[i].textLine;

            // also modify the ith element of playerChoices to erpresent our most recent set of choices acquired
            playerChoices[i] = allChoices[i];
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
        charPortrait.sprite = currentPortrait;

        // when choice is selected, call event handler to trigger onDialogueSelected
        EventHandler.current.DisplayNPCLine(playerChoices[index]);
    }

    /// <summary>
    /// hide unnecessary stuff then call the next dialogue handler.
    /// </summary>
    public void NextButton()
    {
        // clear dialogue text and set new char name
        dialogueText.text = "";
        charName.text = "You";

        // set new portrait
        charPortrait.sprite = playerPortrait;

        // set the next button to be inactive
        nextButton.gameObject.SetActive(false);

        EventHandler.current.DisplayPlayerLines();
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

        EventHandler.current.ConcludeDialogue();
    }
}
