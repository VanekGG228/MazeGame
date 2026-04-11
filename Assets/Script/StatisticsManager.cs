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
    private int attemptId = -1;

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
        Debug.Log("[STAT] Awake OK");
    }

    void OnEnable()
    {
        Debug.Log("[STAT] Subscribing events");

        EventManager.OnPlayerSpawned += OnPlayerSpawned;
        EventManager.OnLevelFinished += OnLevelFinished;
        EventManager.OnLevelFailed += OnLevelFailed;
    }

    void OnDisable()
    {
        Debug.Log("[STAT] Unsubscribing events");

        EventManager.OnPlayerSpawned -= OnPlayerSpawned;
        EventManager.OnLevelFinished -= OnLevelFinished;
        EventManager.OnLevelFailed -= OnLevelFailed;
    }

    void OnPlayerSpawned(GameObject newPlayer)
    {
        Debug.Log("[STAT] OnPlayerSpawned CALLED");

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

        Debug.Log("[STAT] Reset stats done");

        try
        {
            Debug.Log("[STAT] Creating session...");

            int levelId = LevelData.SelectedLevelId;

            if (levelId <= 0)
            {
                Debug.LogError("[STAT] Invalid levelId!");
                return; // FIX: was yield break
            }

            Debug.Log("[STAT] Creating session for level: " + levelId);

            sessionId = DatabaseManager.Instance.CreateSession(1, levelId);

            Debug.Log("[STAT] Session created: " + sessionId);

            Debug.Log("[STAT] Inserting attempt...");

            DatabaseManager.Instance.InsertAttempt(sessionId, "ongoing", 0, 0);

            Debug.Log("[STAT] Attempt inserted");

            attemptId = DatabaseManager.Instance.GetLastAttemptId(sessionId);

            Debug.Log("[STAT] Attempt ID: " + attemptId);

            if (attemptId < 0)
                Debug.LogError("[STAT] INVALID attemptId!");

            recording = true;
            Debug.Log("[STAT] Recording STARTED");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[STAT] DB ERROR: " + e.Message);
        }
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

        if (timer >= recordInterval)
        {
            Debug.Log("[STAT] Saving trajectory point at t=" + levelTime);

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
        Debug.Log("[STAT] Collision: " + collisionCount);
    }

    void OnLevelFinished()
    {
        Debug.Log("[STAT] LEVEL FINISHED");
        SaveData("win");
    }

    void OnLevelFailed()
    {
        Debug.Log("[STAT] LEVEL FAILED");
        SaveData("lose");
    }

    void SaveData(string result)
    {
        Debug.Log("[STAT] Saving data... result = " + result);

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

        DatabaseManager.Instance.UpdateAttemptResult(attemptId, result, levelTime, collisionCount);

        DatabaseManager.Instance.EndSession(sessionId);

        Debug.Log("[STAT] Data saved for attempt " + attemptId);
    }
}