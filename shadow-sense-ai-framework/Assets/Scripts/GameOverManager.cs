using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the game over failure state, UI fading, and returning to the main menu.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStatsManager playerStats;
    public CanvasGroup gameOverGroup;
    public string mainMenuSceneName = "MainMenu_Scene";

    [Header("Settings")]
    public float fadeDuration = 2.0f;
    public float delayBeforeMenu = 2.0f;

    private void Start()
    {
        if (gameOverGroup != null)
        {
            gameOverGroup.alpha = 0;
            gameOverGroup.blocksRaycasts = false;
        }

        if (playerStats != null)
        {
            playerStats.OnPlayerDeath += HandleGameOver;
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath -= HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // 1. Fade to Black
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (gameOverGroup != null)
            {
                gameOverGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            }
            yield return null;
        }

        if (gameOverGroup != null)
        {
            gameOverGroup.alpha = 1;
            gameOverGroup.blocksRaycasts = true;
        }

        // 2. Wait
        yield return new WaitForSeconds(delayBeforeMenu);

        // 3. Load Main Menu
        SceneManager.LoadSceneAsync(mainMenuSceneName);
    }
}
