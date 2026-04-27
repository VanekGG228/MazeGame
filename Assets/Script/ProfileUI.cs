using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("UI")]
    public Image trajectoryImage;
    public TMP_Text statsText;
    public GameObject scrollView;
    public GameObject graphArea;

    [Header("Colors")]
    public Color lineColor = Color.blue;
    public Color levelColor = Color.gray;

    [Header("Map Settings")]
    public float mapWidth = 41f;
    public float mapHeight = 41f;
    public int lineThickness = 3;

    [Header("Attempt")]
    public int attemptId = -1;

    private LevelDataRow levelData;

    void OnEnable()
    {
        if (attemptId < 0) return;
        LoadProfile(attemptId);
    }

    public void LoadProfile(int attemptId)
    {
        int levelId = DatabaseManager.Instance.GetSessionLevelIdFromAttempt(attemptId);
        if (levelId < 0) return;

        levelData = DatabaseManager.Instance.GetLevelById(levelId);

        List<Vector2> trajectory = DatabaseManager.Instance.GetTrajectoryForAttempt(attemptId);
        if (trajectory == null || trajectory.Count < 2) return;

        DrawAll(trajectory);

        var stats = DatabaseManager.Instance.GetStatisticsForAttempt(attemptId);

        statsText.text =
            $"Attempt: {attemptId}\n" +
            $"Level ID: {levelId}\n" +
            $"Difficulty: {levelData?.difficulty}\n" +
            $"Points: {trajectory.Count}\n\n";

        if (stats != null)
        {
            statsText.text +=
                $"Distance: {stats.distance:F2}\n" +
                $"Avg Speed: {stats.avgSpeed:F2}\n" +
                $"Max Speed: {stats.maxSpeed:F2}\n" +
                $"Collisions: {stats.collisions}\n" +
                $"Duration: {FormatTime(stats.duration)}";
        }
        else
        {
            statsText.text += "\nNo statistics available";
        }
    }

    string FormatTime(float seconds)
    {
        if (seconds < 0) return "N/A";

        int min = (int)(seconds / 60);
        int sec = (int)(seconds % 60);
        return $"{min:00}:{sec:00}";
    }

    void DrawAll(List<Vector2> trajectory)
    {
        int width = (int)trajectoryImage.rectTransform.rect.width;
        int height = (int)trajectoryImage.rectTransform.rect.height;

        Texture2D tex = new Texture2D(width, height);

        Color[] bg = new Color[width * height];
        for (int i = 0; i < bg.Length; i++) bg[i] = Color.white;
        tex.SetPixels(bg);

        if (levelData != null)
            DrawLevelWalls(tex, levelData);

        for (int i = 1; i < trajectory.Count; i++)
            DrawLineWorld(tex, trajectory[i - 1], trajectory[i], lineColor);

        tex.Apply();

        trajectoryImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    void DrawLevelWalls(Texture2D tex, LevelDataRow level)
    {
        string path = Path.Combine(Application.streamingAssetsPath, level.path);
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);
        if (wrapper?.strokes == null) return;

        foreach (var stroke in wrapper.strokes)
        {
            if (stroke.points == null || stroke.points.Length < 2) continue;

            for (int i = 1; i < stroke.points.Length; i++)
            {
                Vector2 p1 = stroke.points[i - 1];
                Vector2 p2 = stroke.points[i];

                DrawLine(tex,
                    (int)((p1.x + 0.5f) * tex.width),
                    (int)((p1.y + 0.5f) * tex.height),
                    (int)((p2.x + 0.5f) * tex.width),
                    (int)((p2.y + 0.5f) * tex.height),
                    levelColor);
            }
        }
    }

    void DrawLineWorld(Texture2D tex, Vector2 a, Vector2 b, Color col)
    {
        float ax = ((a.x + mapWidth / 2f) / mapWidth) * tex.width;
        float ay = ((a.y + mapHeight / 2f) / mapHeight) * tex.height;

        float bx = ((b.x + mapWidth / 2f) / mapWidth) * tex.width;
        float by = ((b.y + mapHeight / 2f) / mapHeight) * tex.height;

        DrawLine(tex, (int)ax, (int)ay, (int)bx, (int)by, col);
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            DrawThickPixel(tex, x0, y0, col);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    void DrawThickPixel(Texture2D tex, int x, int y, Color col)
    {
        int half = lineThickness / 2;

        for (int i = -half; i <= half; i++)
        {
            for (int j = -half; j <= half; j++)
            {
                int px = x + i;
                int py = y + j;

                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, col);
            }
        }
    }

    public void ShowAttempt(int attemptId)
    {
        scrollView.SetActive(false);
        graphArea.SetActive(true);
        statsText.gameObject.SetActive(true);
        LoadProfile(attemptId);
    }
}