using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "key", menuName ="Items/Create a new Key")]
public class Key : ItemBase
{
    public override void UseItem(string useOnObj)
    {
        base.UseItem(useOnObj);

        Debug.Log("Using this to unlock door");

        // add stuff on use
        Director.AddToSpeakerMemory(DirectorConstants.PLAYER_STR, addToPlayerMemoryOnUse);
        Director.AddEventString(addToGlobalOnUse);


        // IMPORTANT: TEMPORARILY REMOVED THE TRIGGERITEMEFFECT, MOVED TO ITEMBASE SCRIPT.
    }
}
