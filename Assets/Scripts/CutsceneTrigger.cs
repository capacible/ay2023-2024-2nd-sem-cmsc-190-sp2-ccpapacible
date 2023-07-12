using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Boxcollider trigger of cutscene
/// </summary>
public class CutsceneTrigger : MonoBehaviour
{
    public string[] validTriggers;      // the events needed to trigger cutscene
    public Cutscene cutscene;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(validTriggers.All(ev => Director.EventHasOccurred(ev)) && collision.tag.ToLower() == "player")
        {
            // trigger cutscene
            cutscene.StartCutscene();
        }

    }
    
}
