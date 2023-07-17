using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NarrationHandler : MonoBehaviour
{
    public Fader fader;
    public string sceneName;
    public Image cover;

    // the following starts off deactivated
    public Button[] buttons;
    public TextMeshProUGUI[] buttonsText;
    public Image textBox;
    public TextMeshProUGUI textBoxText;
    public Button next;

    // inner stuff
    private Queue<string> dLines;
    private List<string> choices;

    private bool startOfGame;
    private string nextScene;

    public void Awake()
    {
        SceneHandler.OnUiLoaded += StartNarration;
    }

    /// <summary>
    /// Starts the narration text box stuff
    /// </summary>
    /// <param name="parameters">
    ///     0 - bool startOfGame
    ///     1 - string filename or TextAsset ink
    ///     2 - next scene when done
    /// </param>
    public void StartNarration(object[] parameters)
    {
        // unsubscribe
        SceneHandler.OnUiLoaded -= StartNarration;

        // fade in the text box
        fader.Fade(textBox, fadeOut: false);

        startOfGame = (bool)parameters[0];
        nextScene = parameters[2].ToString();
        TextAsset file;

        if (!startOfGame)
        {
            // if we're not at start of game, we use file name, because nasa end of game na tong narration na to
            // file i/o
            string txt = "test";
            // create text asset
            file = new TextAsset(txt);
        }
        else
        {
            file = (TextAsset)parameters[1];
        }
        
        StartBgNarration(file);
        
    }

    /// <summary>
    /// Initial call to start the background narration
    /// </summary>
    private void StartBgNarration(TextAsset file)
    {
        // start and get a new line
        string[] line = InkDialogueManager.StartDialogue(file);
        // line[0] is the dialogue itself, while next are the tags
        dLines = new Queue<string>(line[0].Split('\n', System.StringSplitOptions.RemoveEmptyEntries));

        ShowNarration(dLines.Dequeue());
    }

    /// <summary>
    /// Show or display the line
    /// </summary>
    /// <param name="line"></param>
    private void ShowNarration(string line = null)
    {
        if (line != null)
            textBoxText.text = line;
    }

    /// <summary>
    /// Displays choices
    /// </summary>
    public void ShowChoices()
    {
        // set buttons to be acctive
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            // change text
            buttonsText[i].text = choices[i];
        }
    }

    /// <summary>
    /// Function for the next button click
    /// </summary>
    public void NextButton()
    {
        // try to dequeue
        if (dLines.TryDequeue(out string line))
        {
            // show narration
            ShowNarration(line);
            next.gameObject.SetActive(true);
        }
        else if (!InkDialogueManager.isActive)
        {
            // exiting dialogue now
            // transitions to next scene
            EventHandler.Instance.TransitionToScene(nextScene);
            // unload this scene
            EventHandler.Instance.UnloadUi(sceneName);
        }
        else
        {
            // get player choice
            choices = InkDialogueManager.GetPlayerChoices();
            // display them
            ShowChoices();
        }
    }

    /// <summary>
    /// Given player's choice, we process the tags.
    ///     The tags are placed inside the NEXT non-choice line that is selected in response to the choice.
    /// </summary>
    /// <param name="index"></param>
    public void ProcessChoice(int index)
    {
        // hide buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }

        // get the next line
        string[] nextLine = InkDialogueManager.BranchOutGivenChoice(index);

        // show narration
        dLines = new Queue<string>(nextLine[0].Split('\n', System.StringSplitOptions.RemoveEmptyEntries));

        ShowNarration(dLines.Dequeue());
    }
}
