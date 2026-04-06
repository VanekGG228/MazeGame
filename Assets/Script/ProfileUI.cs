using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileUI : MonoBehaviour
{
    [Header("UI")]
    public Image trajectoryImage;        // UI Image для рисования траектории
    public TMP_Text statsText;
    public GameObject scrollView;      // Scroll View с попытками
    public GameObject graphArea;       // Панель с графиком

    [Header("Настройки графика")]
    public Color lineColor = Color.red;
    public int lineThickness = 2;

    [Header("Исходный размер карты (в тех же единицах, что и БД)")]
    public float mapWidth = 51f;         // X: -25…25
    public float mapHeight = 51f;        // Y: -25…25

    [Header("Выберите ID попытки")]
    public int attemptId = -1;           // Устанавливается вручную или кодом

    void OnEnable()
    {
        if (attemptId < 0)
        {
            Debug.LogError("[Profile] Attempt ID не задан!");
            statsText.text = "Select a valid attempt ID";
            return;
        }

        LoadProfile(attemptId);
    }

    public void LoadProfile(int attemptId)
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("[Profile] DatabaseManager not found!");
            statsText.text = "Database not available";
            return;
        }

        List<Vector3> trajectory = DatabaseManager.Instance.GetTrajectoryForAttempt(attemptId);

        if (trajectory.Count < 2)
        {
            Debug.Log("[Profile] Not enough points for this attempt");
            statsText.text = "Not enough data for this attempt";
            return;
        }

        // Рассчитываем статистику
        List<float> speeds = CalculateSpeed(trajectory);
        float maxSpeed = Mathf.Max(speeds.ToArray());
        float avgSpeed = CalculateAverage(speeds);

        // Обновляем текст
        if (statsText != null)
        {
            statsText.text = $"Attempt ID: {attemptId}\n" +
                             $"Points: {trajectory.Count}\n" +
                             $"Max Speed: {maxSpeed:F2}\n" +
                             $"Avg Speed: {avgSpeed:F2}";
        }

        // Рисуем траекторию на Image
        if (trajectoryImage != null)
        {
            DrawTrajectoryOnImage(trajectory);
        }
    }

    List<float> CalculateSpeed(List<Vector3> points)
    {
        List<float> speeds = new List<float>();
        for (int i = 1; i < points.Count; i++)
        {
            float dist = Vector3.Distance(points[i - 1], points[i]);
            float speed = dist / 0.1f; // предполагаемый recordInterval
            speeds.Add(speed);
        }
        return speeds;
    }

    float CalculateAverage(List<float> values)
    {
        float sum = 0;
        foreach (var v in values) sum += v;
        return sum / values.Count;
    }

    void DrawTrajectoryOnImage(List<Vector3> trajectory)
    {
        int width = (int)trajectoryImage.rectTransform.rect.width;
        int height = (int)trajectoryImage.rectTransform.rect.height;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color bg = Color.black;
        Color[] fillColor = new Color[width * height];
        for (int i = 0; i < fillColor.Length; i++) fillColor[i] = bg;
        tex.SetPixels(fillColor);

        // Нормализация координат из БД (-mapWidth/2 … mapWidth/2) -> 0…1
        for (int i = 1; i < trajectory.Count; i++)
        {
            Vector3 prev = trajectory[i - 1];
            Vector3 curr = trajectory[i];

            float prevX = ((prev.x + mapWidth / 2f) / mapWidth) * (width - 1);
            float prevY = ((prev.z + mapHeight / 2f) / mapHeight) * (height - 1);

            float currX = ((curr.x + mapWidth / 2f) / mapWidth) * (width - 1);
            float currY = ((curr.z + mapHeight / 2f) / mapHeight) * (height - 1);

            DrawLine(tex, (int)prevX, (int)prevY, (int)currX, (int)currY, lineColor);
        }

        tex.Apply();
        trajectoryImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            for (int tx = -lineThickness / 2; tx <= lineThickness / 2; tx++)
            {
                for (int ty = -lineThickness / 2; ty <= lineThickness / 2; ty++)
                {
                    int px = Mathf.Clamp(x0 + tx, 0, tex.width - 1);
                    int py = Mathf.Clamp(y0 + ty, 0, tex.height - 1);
                    tex.SetPixel(px, py, col);
                }
            }

            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public void ShowAttempt(int attemptId)
    {

        scrollView.gameObject.SetActive(false);
        graphArea.SetActive(true);
        statsText.gameObject.SetActive(true);
        LoadProfile(attemptId);
    }

}