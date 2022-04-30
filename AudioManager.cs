using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [SerializeField] Sound[] sounds;

    public static AudioManager instance;


    private void Awake()
    // Used for initializing an AudioSource for each sound in the Sounds array.
    {
        // Singleton to ensure that only one AudioManger exists.
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }



    public void Play(string name)
    // Play the sound with the given name. 
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);

        if (sound == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
        }
        else
        {
            sound.source.volume = PlayerPrefsController.GetMasterVolume();
            sound.source.Play();
        }
    }
}
