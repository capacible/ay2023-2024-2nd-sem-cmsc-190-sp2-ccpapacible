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
            float i = 1;
            while (toFade.color.a >= 0)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i -= Time.deltaTime / duration;
                yield return null;
            }
        }
        // fade in
        else
        {
            float i = 0;
            while (toFade.color.a <= 1)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i += Time.deltaTime / duration;
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
            float i = 1;
            while (toFade.color.a >= 0)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i -= Time.deltaTime / duration;
                yield return null;
            }
        }
        // fade in
        else
        {
            float i = 0;
            while (toFade.color.a <= 1)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i += Time.deltaTime / duration;
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
            float i = 1;
            while (toFade.color.a >= 0)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i -= Time.deltaTime / duration;
                yield return null;
            }
        }
        // fade in
        else
        {
            float i = 0;
            while (toFade.color.a <= 1)
            {
                // set color alpha
                toFade.color = new Color(toFade.color.r, toFade.color.g, toFade.color.b, i);
                i += Time.deltaTime / duration;
                yield return null;
            }
        }
    }

    #endregion
}
