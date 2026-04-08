using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        string path = Application.streamingAssetsPath + "/Levels";

        if (!Directory.Exists(path))
        {
            Debug.LogError("Levels folder not found: " + path);
            return;
        }

        string[] files = Directory.GetFiles(path, "*.json");

        Debug.Log(files.Length);
        
        foreach (string file in files)
        {
            CreateButton(file);
            Debug.Log(file);
        }
    }

    void CreateButton(string levelPath)
    {
        GameObject button = Instantiate(levelButtonPrefab, content);

        button.transform.localScale = Vector3.one;

        string levelName = Path.GetFileNameWithoutExtension(levelPath);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = levelName;

    
        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartLevel(levelPath);
        });
    }
    public void StartLevel(string levelFile)
    {
        LevelData.SelectedLevelFile = levelFile;  
        UnityEngine.SceneManagement.SceneManager.LoadScene("Calibration");
    }
}