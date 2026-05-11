using UnityEngine.Localization.Settings;

public static class L
{
    public static string Get(string table, string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }

    public static string Get(string table, string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
    }
}