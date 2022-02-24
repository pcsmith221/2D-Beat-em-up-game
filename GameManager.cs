using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of total player lives and ends the game if it drops below zero.

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject loseScreen;

    int playerLives = 0;

    private void Start()
    {
        var playerHealthArray = FindObjectsOfType<Health>();
        foreach (Health playerHealth in playerHealthArray)
        {
            playerLives += playerHealth.GetLives();
        }



        loseScreen.SetActive(false);    
    }



    public void IncrementPlayerLives()
    {
        playerLives++;
    }



    public void DecrementPlayerLives()
    {
        playerLives--;

        if (playerLives < 0)
        {
            GameOver();
        }
    }



    public void GameOver()
    {
        loseScreen.SetActive(true);
    }
}
