using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public static SoundHandler Instance;

    public AudioSource sfxSrc;  // will be used to play sfx.
    public AudioSource musicSrc; // used to play music ACROSS scenes.

    // list of sfx that will be used OFTEN
    public AudioClip[] sfxList;
    public AudioClip[] musicList;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    /// <summary>
    /// Play sfx with given audio name
    /// </summary>
    /// <param name="audioName"></param>
    public void PlaySFX(string audioName, double sfxVolume=0.2)
    {
        sfxSrc.PlayOneShot(sfxList.First(clip => clip.name == audioName),(float) sfxVolume);
    }

    /// <summary>
    /// Play sfx with the passed clip. Used best when an action uses one unique audio.
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySFX(AudioClip clip, double sfxVolume=0.6)
    {
        sfxSrc.PlayOneShot(clip, (float) sfxVolume);
    }

    /// <summary>
    /// Play looping music
    /// </summary>
    /// <param name="clip"></param>
    public void PlayMusic(string clipName)
    {
        // set clip
        musicSrc.clip = musicList.First(clip => clip.name == clipName);

        musicSrc.Play();
    }
}
