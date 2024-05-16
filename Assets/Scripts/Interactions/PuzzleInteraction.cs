using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleInteraction : InteractionBase
{
    [Header("Puzzle requirement")]
    public int puzzleSteps;
    public string unsolvedMsgId;    // id of the message to display when the puzzle is unsolved
    public string[] prereqEvents;   // events that must be in player memory / map / global before completing the puzzle
    private bool[] puzzleProgress;  // array of integers, where each part of the puzzle is represented by one index here

    [Header("Puzzle reward")]
    public string solvedMsgId;
    public string[] eventOnSolved;   // event string to add to director db on solve.
    public ItemBase rewardItem;
    public string solvedNotifId;    // id of a notifier message; use this when the player solves a puzzle after interacting with a different thing.
    public string[] activateObjsOnSolved;

    public AudioClip playAudio;

    public static System.Action<string, int, bool> OnPuzzleProgress;

    private void Start()
    {
        InitializeInteraction();

        // create puzzleSteps size of puzzleprogress
        puzzleProgress = new bool[puzzleSteps];
        for(int i=0;i<puzzleSteps;i++)
        {
            puzzleProgress[i] = false;
        }

        OnPuzzleProgress += ProgressPuzzle;
        Subscribe();
    }

    private void OnDestroy()
    {
        OnPuzzleProgress -= ProgressPuzzle;
        Unsubscribe();
    }

    public override void HandleInteraction(object[] interactParams)
    {
        string id = interactParams[0].ToString();
        

        if(!(objId.Equals(id)))
        {
            // not the interaction that player is on
            return;
        }

        // handle interaction here
        if((prereqEvents.All(ev => Director.EventHasOccurred(ev)) && prereqEvents.Length > 0) || (puzzleProgress.All(p => p == true) && puzzleProgress.Length > 0))
        {
            // if all prereq events are TRUE or has occurred as noted by director or if all progress steps are now true
            
            // run puzzle complete
            // - show interact message of the complete puzzle
            EventHandler.Instance.InteractMessage(solvedMsgId, new Dictionary<string, string> {  });

            foreach (string ev in eventOnSolved)
            {
                // - add completion event to director, globally
                Director.AddEventString(ev);
            }

            foreach(string i in activateObjsOnSolved)
            {
                EventHandler.Instance.SetNewState(i, true);
            }

            if(rewardItem != null)
            {
                EventHandler.Instance.PickupItem(objId, rewardItem);
            }

            // set self to inactive or not interactable
            SetInteractableState(objId, false);
        }
        else
        {
            // show the unsolved
            EventHandler.Instance.InteractMessage(unsolvedMsgId, null);
        }
    }

    /// <summary>
    /// given the progress index, we update our puzzle progress for the puzzle in this id.
    /// </summary>
    /// <param name="puzzleId"></param>
    /// <param name="progressIndex"></param>
    private void ProgressPuzzle(string puzzleId, int progressIndex, bool isSequential=false)
    {
        if(!(puzzleId.Equals(objId)))
        {
            // this puzzle isn't what we have to progress if not same yung puzzle id we received
            return;
        }

        if(isSequential && progressIndex > 0)
        {
            // if the puzzle is sequential, we have to check previous progress index
            if(puzzleProgress[ progressIndex-1 ])
            {
                // if our previous progress is 1 (true), then we update our puzzleProgress at current index
                puzzleProgress[progressIndex] = true;
            }
            else
            {
                return;
            }
        }

        // default case -- puzzle is not sequential
        puzzleProgress[progressIndex] = true;

        //is finished? -- notify the player
        if (puzzleProgress.All(p => p == true))
        {
            if(playAudio!=null)
                SoundHandler.Instance.PlaySFX(playAudio, 0.5);

            EventHandler.Instance.InteractMessage(solvedNotifId, null);
        }
    }
}
