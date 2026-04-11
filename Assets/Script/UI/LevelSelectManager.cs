using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    public Transform content;
    public GameObject levelButtonPrefab;

    void Start()
    {
        LoadLevels();
    }

    void LoadLevels()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager not found!");
            return;
        }

        List<LevelDataRow> levels = DatabaseManager.Instance.GetAllLevels();

        Debug.Log("[LevelSelect] Levels count: " + levels.Count);

        foreach (var level in levels)
        {
            CreateButton(level);
        }
    }

    void CreateButton(LevelDataRow level)
    {
        GameObject button = Instantiate(levelButtonPrefab, content);
        button.transform.localScale = Vector3.one;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = $"{level.id}";

        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartLevel(level);
        });
    }

    public void StartLevel(LevelDataRow level)
    {
        LevelData.SelectedLevelId = level.id;
        LevelData.SelectedLevelPath = level.path;

        Debug.Log($"[LevelSelect] Loading level ID: {level.id}, path: {level.path}");

        UnityEngine.SceneManagement.SceneManager.LoadScene("Calibration");
    }
}