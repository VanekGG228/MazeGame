using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LoadLanguage : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        int langIndex = PlayerPrefs.GetInt("lang", 0);

        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[langIndex];
    }
}