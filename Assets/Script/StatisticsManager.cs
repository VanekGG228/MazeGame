using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    private GameObject player;
    private Rigidbody rb;

    private float timer;
    private float levelTime;

    private int collisionCount = 0;

    private float totalSpeed = 0;
    private int speedSamples = 0;
    private float maxSpeed = 0;

    private float distance = 0;
    private Vector3 lastPosition;

    private int sessionId;
    private int attemptId = -1; // теперь это attemptId

    [Header("Settings")]
    public float recordInterval = 1f;

    [Header("UI")]
    public TMP_Text speedText;
    public TMP_Text collisionText;
    public TMP_Text timeText;

    private bool recording = false;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EventManager.OnPlayerSpawned += OnPlayerSpawned;
        EventManager.OnLevelFinished += OnLevelFinished;
        EventManager.OnLevelFailed += OnLevelFailed;
    }

    void OnDisable()
    {
        EventManager.OnPlayerSpawned -= OnPlayerSpawned;
        EventManager.OnLevelFinished -= OnLevelFinished;
        EventManager.OnLevelFailed -= OnLevelFailed;
    }

    void OnPlayerSpawned(GameObject newPlayer)
    {
        player = newPlayer;
        rb = player.GetComponent<Rigidbody>();

        timer = 0;
        levelTime = 0;
        collisionCount = 0;
        totalSpeed = 0;
        speedSamples = 0;
        maxSpeed = 0;
        distance = 0;
        lastPosition = player.transform.position;

        // создаем сессию
        sessionId = DatabaseManager.Instance.CreateSession(1, 1);

        // создаем попытку сразу же
        DatabaseManager.Instance.InsertAttempt(sessionId, "ongoing", 0, 0);
        attemptId = DatabaseManager.Instance.GetLastAttemptId(sessionId);

        recording = true;
    }

    void Update()
    {
        if (!recording || player == null) return;

        levelTime += Time.deltaTime;
        timer += Time.deltaTime;

        float speed = rb.linearVelocity.magnitude;

        totalSpeed += speed;
        speedSamples++;

        if (speed > maxSpeed)
            maxSpeed = speed;

        distance += Vector3.Distance(lastPosition, player.transform.position);
        lastPosition = player.transform.position;

        UpdateUI(speed);

        // записываем точку траектории каждые recordInterval секунд
        if (timer >= recordInterval)
        {
            DatabaseManager.Instance.InsertTrajectoryPoint(attemptId, player.transform.position, levelTime);
            timer = 0;
        }
    }

    void UpdateUI(float speed)
    {
        if (speedText != null)
            speedText.text = $"Speed: {speed:F2}";
        if (collisionText != null)
            collisionText.text = $"Collisions: {collisionCount}";
        if (timeText != null)
            timeText.text = $"Time: {levelTime:F1}";
    }

    public void RegisterCollision()
    {
        collisionCount++;
    }

    void OnLevelFinished()
    {
        SaveData("win");
    }

    void OnLevelFailed()
    {
        SaveData("lose");
    }

    void SaveData(string result)
    {
        recording = false;

        float avgSpeed = totalSpeed / Mathf.Max(speedSamples, 1);

        DatabaseManager.Instance.InsertStatistics(
            sessionId,
            distance,
            avgSpeed,
            maxSpeed,
            collisionCount,
            levelTime
        );

        // обновляем результат попытки
        DatabaseManager.Instance.UpdateAttemptResult(attemptId, result, levelTime, collisionCount);

        DatabaseManager.Instance.EndSession(sessionId);

        Debug.Log("[STAT] Data saved for attempt " + attemptId);
    }
}