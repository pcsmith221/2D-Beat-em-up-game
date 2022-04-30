using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{

    [SerializeField] GameObject pauseMenuUI;
    static bool isGamePaused = false;
    bool gameEnded = false;


    private void PausePressed()
    {
        if (isGamePaused)
        {
            Resume();
        }
        else if (!gameEnded)
        {
            Pause();
        }
    }



    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
    }



    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;
    }



    public void MainMenu()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        FindObjectOfType<SceneLoader>().LoadMainMenu();
    }



    public bool GetIsGamePaused()
    {
        return isGamePaused;
    }



    public void GameEnded()
    {
        gameEnded = true;
    }



    private void OnEnable()
    // Subscribes to pauseGame event in Player script
    {
        Player.pauseGame += PausePressed;
        GameManager.gameEnded += GameEnded;
    }



    private void OnDisable()
    // Unsubscribes from pauseGame event should the script become disabled
    {
        Player.pauseGame -= PausePressed;
        GameManager.gameEnded -= GameEnded;
    }
}
