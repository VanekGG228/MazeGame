using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttemptItemUI : MonoBehaviour
{
    public TMP_Text label;
    private int attemptId;
    private ProfileUI profileUI;

    public void Init(int id, string text, ProfileUI ui)
    {
        attemptId = id;
        label.text = text;
        profileUI = ui;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log("CLICKED " + attemptId);
        profileUI.ShowAttempt(attemptId);
    }
}