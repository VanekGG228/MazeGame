using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI collisionsText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI speedText;

    [Header("Player")]
    public GameObject player;

    [Header("Recording Settings")]
    public float recordInterval = 0.1f; 

    private Rigidbody rb;
    private Vector3 lastPosition;
    private int collisions = 0;
    private float totalDistance = 0f;
    private float timer = 0f;

    private List<Vector3> trajectory = new List<Vector3>();
    private List<float> speeds = new List<float>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (player == null)
        {
            Debug.LogError("Player не назначен! Перетащи шарик в поле Player в Inspector.");
        }
        else
        {
            rb = player.GetComponent<Rigidbody>();
            if (rb == null)
                Debug.LogError("На player нет Rigidbody!");
            lastPosition = player.transform.position;
            trajectory.Add(lastPosition);
            speeds.Add(0f);
        }
    }

    void Update()
    {
        if (rb == null) return; 

        Vector3 curPos = rb.transform.position;
        float frameDist = Vector3.Distance(lastPosition, curPos);
        totalDistance += frameDist;
        lastPosition = curPos;

        if (distanceText != null)
            distanceText.text = "Distance: " + totalDistance.ToString("F2");

        if (speedText != null)
            speedText.text = "Speed: " + rb.linearVelocity.magnitude.ToString("F2");

        timer += Time.deltaTime;
        if (timer >= recordInterval)
        {
            trajectory.Add(curPos);
            speeds.Add(rb.linearVelocity.magnitude);
            timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveStatistics();
            Debug.Log($"F5 got");
        }
    }

    public void AddCollision()
    {
        collisions++;
        if (collisionsText != null)
            collisionsText.text = "Collisions: " + collisions;
    }

    public void SaveStatistics(string fileName = "BallStats.csv")
    {
        if (trajectory.Count == 0)
        {
            Debug.LogWarning("Траектория пуста, нет данных для сохранения.");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Time;X;Y;Z;Speed");

                float time = 0f;
                for (int i = 0; i < trajectory.Count; i++)
                {
                    Vector3 pos = trajectory[i];
                    float speed = speeds[i];
                    writer.WriteLine($"{time:F2};{pos.x:F3};{pos.y:F3};{pos.z:F3};{speed:F3}");
                    time += recordInterval;
                }

                writer.WriteLine();
                writer.WriteLine($"TotalDistance,{totalDistance:F3}");
                writer.WriteLine($"TotalCollisions,{collisions}");
            }

            Debug.Log($"Statistics saved successfully to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении статистики: " + e.Message);
        }
    }
}
