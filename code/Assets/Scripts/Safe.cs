using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Safe : MonoBehaviour
{
    public const string ANSWER = "4203";
    public const int MAX_DIGITS = 4;
    public const string OPENED_SAFE_GAME_MSG = "safe_open";

    public Canvas c;
    public Image currentSafeImg;
    public TextMeshProUGUI currentPassDisplay;
    public GameObject buttonParent;
    public ItemBase puzzleItem;

    // the images we willl use throughout the puzzle
    public Sprite[] safeImages;
    public string sceneName;
    public string[] rewardActivateNewObjects;
    public string[] rewardAddEventToGlobal;

    private string safeObjId;
    private int[] enteredPasscode = new int[4] { 0, 0, 0, 0 };
    private int currentDigit = 0;
    private int currSafeImgCount = 0;

    private void Awake()
    {
        EventHandler.Examine += Initialize;
    }

    private void OnDestroy()
    {
        EventHandler.Examine -= Initialize;
    }

    /// <summary>
    /// Initialize safe script
    /// </summary>
    /// <param name="examineParams">
    ///     [0] interaction/ui type
    ///     [1] scene name
    ///     [2] obj id of safe interactable
    ///     [3] image (none)
    /// </param>
    private void Initialize(object[] examineParams)
    {
        // scene name
        sceneName = examineParams[1].ToString();
        // we simply remember the objId of the safe.
        safeObjId = examineParams[2].ToString();
    }

    public void PressButton(int buttonNum)
    {
        // change the entered passcode at current digit
        enteredPasscode[currentDigit] = buttonNum;
        currentDigit++;

        // change the display at tmprogui
        currentPassDisplay.text = string.Join("", enteredPasscode);

        // update current digit if we're at maximum already.
        if(currentDigit >= MAX_DIGITS)
        {
            currentDigit = 0;
        }

        // check if out passcode is the correct one.
        if(string.Join("", enteredPasscode) == ANSWER)
        {
            // change image of safe
            currentSafeImg.sprite = safeImages[++currSafeImgCount];
            // play some unlock sound effect
            SoundHandler.Instance.PlaySFX("open_safe", 1);

            StartCoroutine(FinishPuzzle());

            // deactivate the safe puzzle.
            EventHandler.Instance.SetNewState(safeObjId, false);
            
        }
    }

    private IEnumerator FinishPuzzle()
    {
        // activate reward objects to be interactable
        foreach (string id in rewardActivateNewObjects)
        {
            EventHandler.Instance.SetNewState(id, true);
            yield return null;
        }

        foreach (string ev in rewardAddEventToGlobal)
        {
            Director.AddEventString(ev);
            yield return null;
        }
    }

    public void ExitButton()
    {
        // unload the ui and conclude interaction
        EventHandler.Instance.ConcludeInteraction(UiType.EXAMINE_OBJECT);
        EventHandler.Instance.UnloadUi(sceneName);

        if (string.Join("", enteredPasscode) == ANSWER)
        {
            // show display message
            // show game message
            EventHandler.Instance.InteractMessage(OPENED_SAFE_GAME_MSG, null);
            // pickup beth item
            // no items will trigger this, we won't also destroy the safe object because it's not an ItemInteraction that listens
            // to the PickupItem delegate
            EventHandler.Instance.PickupItem(safeObjId, puzzleItem);
        }
    }
}
