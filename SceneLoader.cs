using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Methods for changing scenes and quitting the game.  
public class SceneLoader : MonoBehaviour
{
    [SerializeField] Animator transition;
    [Tooltip("Number of seconds for transition animation")]
    [SerializeField] float transitionTime = 1f;



    // Store scene indices in one place 
    int menuSceneIndex = 1;



    public void LoadNextScene()
    // Loads the next scene in the build index. 
    {
        // Ensure time is running normally after time stops in level complete screen
        Time.timeScale = 1f;
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }



    IEnumerator LoadScene(int sceneIndex)
    // Coroutine to give scene transition time to play. Coroutines can only be called by other functions, not from the editor 
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneIndex);
    }



    IEnumerator LoadScene(string sceneName)
    // LoadScene overload that loads scene using string instead of build index 
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);
    }



    public void RestartScene()
    // Restarts the current scene
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }



    public void LoadMainMenu()
    // Loads the main menu. 
    {
        StartCoroutine(LoadScene(menuSceneIndex));
    }



    public void LoadSceneByName(string sceneName)
    // Calls overload of LoadScene coroutine that takes a string parameter
    {
        StartCoroutine(LoadScene(sceneName));
    }



    public void QuitGame()
    // Closes the game. Has no effect in Editor. 
    {
        Debug.Log("Closed Game!");
        Application.Quit();
    }
}
