using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controller for the Main Menu, managing asynchronous scene loading and application exit.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Exact name of the scene to load when starting the game.")]
    public string gameplaySceneName = "Gameplay_Scene";

    /// <summary>
    /// Initiates asynchronous loading of the gameplay scene.
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Safely exits the application or stops Editor play mode.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
