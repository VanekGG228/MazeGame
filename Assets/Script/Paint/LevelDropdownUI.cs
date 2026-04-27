using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelDropdownUI : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public BrushDrawer brushDrawer;

    private List<LevelDataRow> levels;

    void Start()
    {
        LoadLevels();
    }

    void LoadLevels()
    {
        levels = DatabaseManager.Instance.GetAllLevels();

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        options.Add("Clear");

        foreach (var level in levels)
        {
            options.Add(level.id.ToString());
        }

        dropdown.AddOptions(options);

        dropdown.onValueChanged.AddListener(OnLevelSelected);
    }

    void OnLevelSelected(int index)
    {

        if (index == 0)
        {
            Debug.Log("New blank canvas");

            brushDrawer.LoadBlankCanvas(); 
            return;
        }

        int levelId = levels[index - 1].id;

        brushDrawer.Load(levelId);
    }
}