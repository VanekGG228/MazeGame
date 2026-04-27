using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadLanguage());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator LoadLanguage()
    {
        yield return LocalizationSettings.InitializationOperation;

        int index = PlayerPrefs.GetInt("lang", 0);

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
    }

    public void ToggleLanguage()
    {
        int currentIndex = LocalizationSettings.AvailableLocales.Locales
            .IndexOf(LocalizationSettings.SelectedLocale);

        int newIndex = (currentIndex == 0) ? 1 : 0;

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[newIndex];

        PlayerPrefs.SetInt("lang", newIndex);
        PlayerPrefs.Save();
    }
}