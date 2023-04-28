using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades an image or sprite.
/// </summary>
public class Fader : MonoBehaviour
{

    #region FADING IMAGE

    public void Fade(Image toFade, double duration=0.5, bool fadeOut=true)
    {
        StartCoroutine(FadeObj(toFade, (float)duration, fadeOut));
    }

    IEnumerator FadeObj(Image toFade, float duration, bool fadeOut)
    {

        // disappears!
        if (fadeOut)
        {
            for(float i=1; i>=0; i -= Time.deltaTime/duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
        // fade in
        else
        {
            for (float i = 0; i <= 1; i += Time.deltaTime/duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
    }
    #endregion

    #region FADING SPRITE

    // fading a sprite
    public void Fade(SpriteRenderer toFade, double duration=0.5, bool fadeOut = true)
    {
        StartCoroutine(FadeObj(toFade, (float)duration, fadeOut));
    }

    IEnumerator FadeObj(SpriteRenderer toFade, float duration, bool fadeOut)
    {
        // disappears!
        if (fadeOut)
        {
            for (float i = 1; i >= 0; i -= Time.deltaTime/duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
        // fade in
        else
        {
            for (float i = 0; i <= 1; i += Time.deltaTime/duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
    }
    #endregion

    #region FADING TEXT

    public void Fade(TextMeshProUGUI toFade, double duration=0.5, bool fadeOut = true)
    {
        StartCoroutine(FadeObj(toFade, (float)duration, fadeOut));
    }

    IEnumerator FadeObj(TextMeshProUGUI toFade, float duration, bool fadeOut)
    {

        // disappears!
        if (fadeOut)
        {
            for (float i = 1; i >= 0; i -= Time.deltaTime / duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
        // fade in
        else
        {
            for (float i = 0; i <= 1; i += Time.deltaTime / duration)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                yield return null;
            }
        }
    }

    #endregion
}
