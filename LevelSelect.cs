using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] Button[] levels;
    [SerializeField] int currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        // currentLevel = PlayerPrefsController.GetCurrentLevel();

        foreach(Button level in levels)
        {
            level.interactable = false;
            level.GetComponentInChildren<Image>().color = Color.gray;
        }

        ShowUnlockedLevels();
    }




    private void ShowUnlockedLevels()
    {
        for (int levelIndex = 0; levelIndex <= currentLevel; levelIndex++)
        {
            var level = levels[levelIndex];
            level.interactable = true;
            level.GetComponentInChildren<Image>().color = Color.white;
        }
    }




    
}
