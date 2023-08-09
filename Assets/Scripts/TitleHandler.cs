using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleHandler : MonoBehaviour
{
    public string nextMapScene;
    public Canvas canvas;

    private void Start()
    {
        canvas.worldCamera = Camera.main;
    }

    public void GoToStart(string scene)
    {
        EventHandler.Instance.LoadUi(scene, new object[] { true, null, nextMapScene });
    }
    
}
