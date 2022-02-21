using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Methods for changing scenes and quitting the game.  
public class SceneLoader : MonoBehaviour
{

    string menuScene = "MainMenu";



    public void LoadNextScene()
    // Loads the next scene in the build index. 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void RestartScene()
    // Restarts the current scene
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    public void LoadMainMenu()
    // Loads the main menu. Note that the name of the menu scene must match the menuScene variable exactly. 
    {
        SceneManager.LoadScene(menuScene);
    }



    public void QuitGame()
    // Closes the game. Has no effect in Editor. 
    {
        Debug.Log("Closed Game!");
        Application.Quit();
    }
}
