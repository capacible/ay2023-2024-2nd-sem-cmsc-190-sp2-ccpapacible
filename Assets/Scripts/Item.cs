using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // unique id for the object attached.
    public string objId = "";

    // storage of item data, 
    public ItemData data;
    

    [ContextMenu("Generate Item Object Id")]
    public void GenerateObjectId()
    {
        if (data != null)
        {
            // if we have an attached itemdata, generate id based on itemId + some guid
            objId = data.itemId + System.Guid.NewGuid().ToString();
        }
        else
        {
            objId = "TEMP_" + System.Guid.NewGuid().ToString();
        }
    }
}
