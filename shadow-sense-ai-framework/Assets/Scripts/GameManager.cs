using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Global game state manager.
/// Handles the Pause system (ESC), Win conditions, and high-level game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject winScreenPanel;

    private bool isPaused = false;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find and subscribe to player death
        PlayerStatsManager player = FindFirstObjectByType<PlayerStatsManager>();
        if (player != null)
        {
            player.OnPlayerDeath += CleanupAndGameOver;
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void CleanupAndGameOver()
    {
        isGameOver = true;
        CleanupActivePools();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void OnBossDefeated()
    {
        isGameOver = true;
        CleanupActivePools();
        StartCoroutine(ShowWinScreen());
    }

    /// <summary>
    /// Forces all active entities back to their respective pools to ensure zero memory leaks.
    /// </summary>
    private void CleanupActivePools()
    {
        Debug.Log("Cleaning up active entities for endgame state...");
        
        // Return all enemies to pool
        EnemyHealth[] activeEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var enemy in activeEnemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                // We use a generalized cleanup here. 
                // In a production scenario, we'd maintain a registry of active pooled objects.
                enemy.TakeDamage(99999f, gameObject); // Force death logic
            }
        }

        // Return all lasers to pool
        BossLaser[] activeLasers = FindObjectsByType<BossLaser>(FindObjectsSortMode.None);
        foreach (var laser in activeLasers)
        {
            if (laser.gameObject.activeInHierarchy)
            {
                ObjectPoolManager.Instance.ReturnToPool(laser.poolTag, laser.gameObject);
            }
        }
    }

    private System.Collections.IEnumerator ShowWinScreen()
    {
        yield return new WaitForSeconds(2.0f);
        if (winScreenPanel != null) winScreenPanel.SetActive(true);

        Time.timeScale = 0.5f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame() => TogglePause();

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu_Scene");
    }
}
