using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsController : MonoBehaviour
{
    // PlayerPrefs parameters.
    const float MAX_VOLUME = 1f;
    const float MIN_VOLUME = 0f;

    // PlayerPrefs keys
    const string MASTER_VOLUME_KEY = "master volume";
    const string CURRENT_LEVEL_KEY = "current level";



    public static void SetMasterVolume(float volume)
    {
        if ( (volume >= MIN_VOLUME) && (volume <= MAX_VOLUME) )
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
        }
        else
        {
            Debug.LogError("Master volume out of range");
        }
    }



    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
    }



    public static void NextLevel()
    {
        int currentLevel = GetCurrentLevel();
        currentLevel++;

        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevel);
    }



    public static void SetCurrentLevel(int level)
    // Used for debug purposes.
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, level);
    }


    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(CURRENT_LEVEL_KEY);
    }
}
