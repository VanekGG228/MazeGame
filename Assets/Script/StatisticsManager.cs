using UnityEngine;
using TMPro;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    private GameObject player;
    private Rigidbody rb;

    private float levelTime;
    private float recordTimer;

    private int collisionCount = 0;

    private float totalSpeed = 0;
    private int speedSamples = 0;
    private float maxSpeed = 0;

    private float distance = 0;
    private Vector3 lastPosition;

    private int sessionId;
    private int attemptId = -1;

    [Header("Settings")]
    public float recordInterval = 0.1f;

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

        levelTime = 0f;
        recordTimer = 0f;

        collisionCount = 0;
        totalSpeed = 0f;
        speedSamples = 0;
        maxSpeed = 0f;
        distance = 0f;

        lastPosition = rb.position;

        int levelId = LevelData.SelectedLevelId;
        if (levelId <= 0) return;

        sessionId = DatabaseManager.Instance.CreateSession(1, levelId);
        DatabaseManager.Instance.InsertAttempt(sessionId, "ongoing", 0, 0);

        attemptId = DatabaseManager.Instance.GetLastAttemptId(sessionId);

        recording = true;
    }

    void FixedUpdate()
    {
        if (!recording || player == null) return;

        levelTime += Time.fixedDeltaTime;

        float speed = rb.linearVelocity.magnitude;

        totalSpeed += speed;
        speedSamples++;

        if (speed > maxSpeed)
            maxSpeed = speed;

 
        distance += Vector3.Distance(lastPosition, rb.position);
        lastPosition = rb.position;

        UpdateUI(speed);

        recordTimer += Time.fixedDeltaTime;

        if (recordTimer >= recordInterval)
        {
            DatabaseManager.Instance.InsertTrajectoryPoint(
                attemptId,
                rb.position,
                levelTime
            );

            recordTimer = 0f;
        }
    }

    void UpdateUI(float speed)
    {
        if (speedText != null)
        {
            string speedLabel = L.Get("UI", "speed");
            speedText.text = $"{speedLabel}: {speed:F2}";
        }

        if (collisionText != null)
        {
            string collisionLabel = L.Get("UI", "collisions");
            collisionText.text = $"{collisionLabel}: {collisionCount}";
        }

        if (timeText != null)
        {
            string timeLabel = L.Get("UI", "time");
            timeText.text = $"{timeLabel}: {levelTime:F1}";
        }
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
            attemptId,
            distance,
            avgSpeed,
            maxSpeed,
            collisionCount,
            levelTime
        );

        DatabaseManager.Instance.UpdateAttemptResult(
            attemptId,
            result,
            levelTime,
            collisionCount
        );

        DatabaseManager.Instance.EndSession(sessionId);
    }

    public void ForceExit()
    {
        if (!recording) return;

        SaveData("exit");
        recording = false;
    }
}