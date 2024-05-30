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
        SoundHandler.Instance.PlayMusic("holizna_quiet_village_3");

        canvas.worldCamera = Camera.main;
    }

    public void GoToStart(string scene)
    {
        SoundHandler.Instance.PlaySFX("select_button_2");
        EventHandler.Instance.LoadUi(scene, null);
        //EventHandler.Instance.UnloadUi("TitleScreen");
    }
    
}
