using UnityEngine;
using System.Collections.Generic;

public class AttemptsListUI : MonoBehaviour
{
    [Header("UI")]
    public Transform content;          // Scroll → Viewport → Content
    public GameObject itemPrefab;      // prefab AttemptItem
    public ProfileUI profileUI;

    void Start()
    {
        LoadAttempts();
    }

    public void LoadAttempts()
    {
        Debug.Log("LoadAttempts called");

        // очистка
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // получаем данные из БД
        List<AttemptData> attempts = DatabaseManager.Instance.GetAllAttemptsWithTrajectory();

        Debug.Log("Attempts found: " + attempts.Count);

        foreach (var attempt in attempts)
        {
            GameObject obj = Instantiate(itemPrefab, content);

            AttemptItemUI item = obj.GetComponent<AttemptItemUI>();

            string text = $"Attempt #{attempt.id}";

            item.Init(attempt.id, text, profileUI);
        }

        // автозагрузка первой
        if (attempts.Count > 0)
        {
            profileUI.LoadProfile(attempts[0].id);
        }
    }
}