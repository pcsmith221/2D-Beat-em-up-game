using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of total player lives and ends the game if it drops below zero.

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject loseScreen;

    int playerLives = 0;

    public static event Action gameEnded;
    public static event Action bossDied;


    private void Start()
    {
        loseScreen.SetActive(false);    
    }



    public void AddPlayerLives(int lives)
    {
        playerLives += lives;
    }



    public void IncrementPlayerLives()
    {
        playerLives++;
    }



    public void DecrementPlayerLives()
    {
        playerLives--;

        if (playerLives <= 0)
        {
            GameOver();
        }
    }



    public void BossDefeated()
    {
        bossDied?.Invoke();
    }



    public void GameOver()
    {
        gameEnded?.Invoke();
        loseScreen.SetActive(true);
        FindObjectOfType<MusicPlayer>().GetComponent<AudioSource>().clip = null;
        FindObjectOfType<AudioManager>().Play("Game Over");
    }

}
