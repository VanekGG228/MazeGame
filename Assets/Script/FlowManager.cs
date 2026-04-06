using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFlowManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject winText;   // объект с текстом YOU WIN
    public GameObject loseText;  // объект с текстом YOU LOSE

    [Header("Settings")]
    public float finishDelay = 5f; // задержка перед меню
    public float loseDelay = 2f;   // задержка перед рестартом

    private bool levelEnded = false;

    void Start()
    {
        Debug.Log("[LevelFlowManager] Start: initializing texts");

        // Отключаем тексты и Animator до начала
        if (winText != null)
        {
            winText.SetActive(false);
            Animator winAnim = winText.GetComponent<Animator>();
            if (winAnim != null)
            {
                winAnim.enabled = false;
                Debug.Log("[LevelFlowManager] WinText Animator disabled at start");
            }
        }

        if (loseText != null)
        {
            loseText.SetActive(false);
            Animator loseAnim = loseText.GetComponent<Animator>();
            if (loseAnim != null)
            {
                loseAnim.enabled = false;
                Debug.Log("[LevelFlowManager] LoseText Animator disabled at start");
            }
        }
    }

    void OnEnable()
    {
        EventManager.OnLevelFinished += HandleLevelFinished;
        EventManager.OnLevelFailed += HandleLevelFailed;
        Debug.Log("[LevelFlowManager] Subscribed to events");
    }

    void OnDisable()
    {
        EventManager.OnLevelFinished -= HandleLevelFinished;
        EventManager.OnLevelFailed -= HandleLevelFailed;
        Debug.Log("[LevelFlowManager] Unsubscribed from events");
    }

    private void HandleLevelFinished()
    {
        if (levelEnded)
        {
            Debug.Log("[LevelFlowManager] HandleLevelFinished called but level already ended, skipping");
            return;
        }

        levelEnded = true;
        Debug.Log("[LevelFlowManager] Level Finished Triggered");

        if (winText != null)
        {
            winText.SetActive(true);
            Debug.Log("[LevelFlowManager] WinText object activated");

            Animator winAnim = winText.GetComponent<Animator>();
            if (winAnim != null)
            {
                winAnim.enabled = true;
                Debug.Log("[LevelFlowManager] WinText Animator enabled");

                // Проверим имя анимации в Debug
                Debug.Log("[LevelFlowManager] Playing WinText animation 'winText'");
                winAnim.Play("winText");
            }
            else
            {
                Debug.LogWarning("[LevelFlowManager] WinText Animator not found!");
            }
        }
        else
        {
            Debug.LogWarning("[LevelFlowManager] WinText object is not assigned!");
        }

        StartCoroutine(ExitToMenuAfterDelay());
    }

    private void HandleLevelFailed()
    {
        if (levelEnded)
        {
            Debug.Log("[LevelFlowManager] HandleLevelFailed called but level already ended, skipping");
            return;
        }

        levelEnded = true;
        Debug.Log("[LevelFlowManager] Level Failed Triggered");

        if (loseText != null)
        {
            loseText.SetActive(true);
            Debug.Log("[LevelFlowManager] LoseText object activated");

            Animator loseAnim = loseText.GetComponent<Animator>();
            if (loseAnim != null)
            {
                loseAnim.enabled = true;
                Debug.Log("[LevelFlowManager] LoseText Animator enabled");

                Debug.Log("[LevelFlowManager] Playing LoseText animation 'loseText'");
                loseAnim.Play("loseText");
            }
            else
            {
                Debug.LogWarning("[LevelFlowManager] LoseText Animator not found!");
            }
        }
        else
        {
            Debug.LogWarning("[LevelFlowManager] LoseText object is not assigned!");
        }

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator ExitToMenuAfterDelay()
    {
        Debug.Log($"[LevelFlowManager] Waiting {finishDelay} seconds before loading MainMenu");
        yield return new WaitForSeconds(finishDelay);
        Debug.Log("[LevelFlowManager] Loading MainMenu");
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator RestartAfterDelay()
    {
        Debug.Log($"[LevelFlowManager] Waiting {loseDelay} seconds before restarting level");
        yield return new WaitForSeconds(loseDelay);
        Debug.Log("[LevelFlowManager] Restarting current level");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}