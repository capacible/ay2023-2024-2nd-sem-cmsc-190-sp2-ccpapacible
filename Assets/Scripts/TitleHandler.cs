using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleHandler : MonoBehaviour
{
    public TextAsset startGameFile;
    public string nextMapScene;
    
    /// <summary>
    /// Loads the required scene
    /// </summary>
    /// <param name="index"></param>
    public void GoToInstruction(string scene)
    {
        EventHandler.Instance.LoadUi(scene, null);
    }

    public void GoToStart(string scene)
    {
        EventHandler.Instance.LoadUi(scene, new object[] { true, startGameFile, nextMapScene });
    }
    
}
