using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// This class loads the level complete screen when the player reaches the end of a level
// Just loads next scene if the current level is the tutorial
public class EndLevel : MonoBehaviour
{
    [SerializeField] GameObject levelCompleteScreen;
    [SerializeField] GameObject p1Stats;
    [SerializeField] GameObject p2Stats;

    string tutorialSceneName = "Tutorial";
    bool inTutorial = false;

    private void Start()
    // Checks if the current scene is the tutorial
    {
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == tutorialSceneName)
        {
            inTutorial = true;
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    // Handles player entering the end level game object's collider
    {
        if(inTutorial)
        {
            FindObjectOfType<SceneLoader>().LoadNextScene();
        }
        else
        {
            Time.timeScale = 0f;
            levelCompleteScreen.SetActive(true);
            ShowPlayerStats();
        }
    }



    private void ShowPlayerStats()
    // Populate the stats for each player 
    {
        var players = FindObjectsOfType<Player>();

        foreach (var player in players)
        {
            player.SetIsDisabled(true);

            switch (player.GetPlayerNumber())
            {
                case 1:
                    p1Stats.SetActive(true);
                    var p1Text = GameObject.FindGameObjectWithTag("P1 Stats").GetComponent<TextMeshProUGUI>();
                    p1Text.text = "Score: " + player.GetComponent<Score>().GetScore() +
                        "\nLives Remaining: " + player.GetComponent<Health>().GetLives();
                    break;
                case 2:
                    p2Stats.SetActive(true);
                    var p2Text = GameObject.FindGameObjectWithTag("P2 Stats").GetComponent<TextMeshProUGUI>();
                    p2Text.text = "Score: " + player.GetComponent<Score>().GetScore() +
                        "\nLives Remaining: " + player.GetComponent<Health>().GetLives();
                    break;
                default:
                    Debug.LogError("No stats box found");
                    break;
            }
        }
    }

}
