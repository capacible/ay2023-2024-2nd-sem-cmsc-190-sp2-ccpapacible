using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEffect
{
    public List<string> addEventToGlobal;
    public List<string> removeEventFromGlobal;
    public Dictionary<string, List<string>> addEventToMap;
    public Dictionary<string, List<string>> removeEventFromMap;
    public int relationshipEffect;
    public string goalToAchieved;
    public string goalToActive;
    public List<string> npc_toRemember;
    public List<string> player_toRemember;
}
