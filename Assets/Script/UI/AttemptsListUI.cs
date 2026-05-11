using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public class AttemptsListUI : MonoBehaviour
{
    [Header("UI")]
    public Transform content;         
    public GameObject itemPrefab;      
    public ProfileUI profileUI;

    void Start()
    {
        LoadAttempts();
    }

    public void LoadAttempts()
    {
        Debug.Log("LoadAttempts called");

        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<AttemptData> attempts = DatabaseManager.Instance.GetAllAttemptsWithTrajectory();

        Debug.Log("Attempts found: " + attempts.Count);

        foreach (var attempt in attempts)
        {
            GameObject obj = Instantiate(itemPrefab, content);

            AttemptItemUI item = obj.GetComponent<AttemptItemUI>();

            string attemptLabel = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "attempt");

            string text = $" #{attempt.id}";

            item.Init(attempt.id, text, profileUI);
        }

        if (attempts.Count > 0)
        {
            profileUI.LoadProfile(attempts[0].id);
        }
    }
}