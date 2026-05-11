using UnityEngine;
using UnityEngine.Localization.Settings;
using System;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public static event Action OnLanguageChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Init());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Init()
    {
        yield return LocalizationSettings.InitializationOperation;

        int index = PlayerPrefs.GetInt("lang", 0);

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
    }

    public void SetLanguage(int index)
    {
        StartCoroutine(SetLanguageRoutine(index));
    }

    IEnumerator SetLanguageRoutine(int index)
    {
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];

        PlayerPrefs.SetInt("lang", index);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }

    public void ToggleLanguage()
    {
        int currentIndex = LocalizationSettings.AvailableLocales.Locales
            .IndexOf(LocalizationSettings.SelectedLocale);

        int newIndex = (currentIndex == 0) ? 1 : 0;

        SetLanguage(newIndex);
    }
}