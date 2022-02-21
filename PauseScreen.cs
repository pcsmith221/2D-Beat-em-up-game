using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{

    [SerializeField] GameObject pauseMenuUI;
    static bool isGamePaused = false;

    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
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
}
