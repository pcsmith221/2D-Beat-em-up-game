using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject loseScreen;

    int playersWithLives = 0;
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


    private void Update()
    {
        if (playerLives < 0)
        {
            GameOver();
        }
    }



    public void IncrementPlayerLives()
    {
        playerLives++;
    }



    public void DecrementPlayerLives()
    {
        playerLives--;
    }



    public void GameOver()
    {
        loseScreen.SetActive(true);
    }
}
