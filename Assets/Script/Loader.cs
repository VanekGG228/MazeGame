using System.IO;
using UnityEngine;

public static class SaveLoadService
{
    public static void Save(StrokeRepository repo, int canvasShape)
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager not found!");
            return;
        }

        // 1. создаём запись и получаем ID
        int levelId = DatabaseManager.Instance.InsertLevel(
            "New Level",
            "Custom",
            "" // пока без пути
        );

        string fileName = levelId + ".json";

        string folderPath;

#if UNITY_EDITOR
        folderPath = Path.Combine(Application.dataPath, "StreamingAssets/Levels");
#else
        folderPath = Path.Combine(Application.persistentDataPath, "Levels");
#endif

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fullPath = Path.Combine(folderPath, fileName);


        StrokeListWrapper wrapper = new StrokeListWrapper
        {
            strokes = repo.strokes,
            canvasShape = canvasShape
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(fullPath, json);

        Debug.Log("Saved: " + fullPath);

        string relativePath = "Levels/" + fileName;

        DatabaseManager.Instance.UpdateLevelPath(levelId, relativePath);

        Debug.Log("Level saved with ID: " + levelId);
    }

    public static StrokeListWrapper Load(int levelId)
    {
        Debug.Log($"[Load] Start loading level with ID: {levelId}");

        string fileName = levelId + ".json";
        Debug.Log($"[Load] File name: {fileName}");

        string path;

#if UNITY_EDITOR
    path = Path.Combine(Application.dataPath, "StreamingAssets/Levels", fileName);
    Debug.Log("[Load] Using EDITOR path");
#else
        path = Path.Combine(Application.persistentDataPath, "Levels", fileName);
        Debug.Log("[Load] Using BUILD path");
#endif

        Debug.Log($"[Load] Full path: {path}");

        if (!File.Exists(path))
        {
            Debug.LogError($"[Load] ❌ File not found at: {path}");
            return null;
        }

        Debug.Log("[Load] File exists, reading...");

        string json = File.ReadAllText(path);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[Load] ❌ JSON is empty!");
            return null;
        }

        Debug.Log($"[Load] JSON length: {json.Length}");

        StrokeListWrapper data = JsonUtility.FromJson<StrokeListWrapper>(json);

        if (data == null)
        {
            Debug.LogError("[Load] ❌ Failed to parse JSON!");
            return null;
        }

        Debug.Log($"[Load] ✅ Successfully loaded level {levelId}");
        Debug.Log($"[Load] Strokes count: {data.strokes?.Count}");

        return data;
    }
}