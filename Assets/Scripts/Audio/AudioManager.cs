using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;
using Unity.VisualScripting;

//Header
//Audio Manager Script. Helps with audio big scale management
//Script from this youtube video Link:https://www.youtube.com/watch?v=rdX7nhH6jdM&t

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds;  //two arrays from sound class
    public AudioSource musicSource, sfxSource; //Audio Sources (children of the audioManager gameObject)

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayMusic(string name) //searches the music from musicSounds and plays it when called with music name
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null) //If there is no music with needed name it will give you error
        {
            Debug.Log("Error. Sound Not Found");
        }


        else //Plays the audioclip what is needed
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }
    private void Start() //mMin theme starts playing when you open the game
    {
        PlayMusic("Base Theme");
    }

    public void PlaySFX(string name) //searches the music from SFXSounds and plays it when called with SFXSoundEffect name
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null) //If there is no soundeffect with needed name it will give you error 
        {
            Debug.Log("Error. Sound Not Found");
        }
        else //Plays the audioclip what is needed
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }


}