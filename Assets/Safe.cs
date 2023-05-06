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

    public Image currentSafeImg;
    public TextMeshProUGUI currentPassDisplay;
    public GameObject buttonParent;
    public ItemBase puzzleItem;

    // the images we willl use throughout the puzzle
    public Sprite[] safeImages;
    public string sceneName;

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

    private void Initialize(object[] examineParams)
    {
        // we simply remember the objId of the safe.
        safeObjId = examineParams[1].ToString();
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
            //currentSafeImg.sprite = safeImages[++currSafeImgCount];
            // play some unlock sound effect

            // show game message
            EventHandler.Instance.InteractMessage(OPENED_SAFE_GAME_MSG, null);

            // pickup beth item
            // no items will trigger this, we won't also destroy the safe object because it's not an ItemInteraction that listens
            // to the PickupItem delegate
            EventHandler.Instance.PickupItem(safeObjId, puzzleItem);

            // deactivate the safe puzzle.
            EventHandler.Instance.SetNewState(safeObjId, false);

            // conclude interaction
            EventHandler.Instance.ConcludeInteraction(UiType.EXAMINE_OBJECT);

            // unload
            EventHandler.Instance.UnloadUi(sceneName);
        }
    }

    public void ExitButton()
    {
        // unload the ui and conclude interaction
        EventHandler.Instance.ConcludeInteraction(UiType.EXAMINE_OBJECT);
        EventHandler.Instance.UnloadUi(sceneName);
    }
}
