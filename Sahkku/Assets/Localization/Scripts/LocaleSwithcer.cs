using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleSwitcher : MonoBehaviour
{
    private int currentIndex = 0;

    private void Start()
    {
        currentIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        if (currentIndex < 0) currentIndex = 0;
    }

    public void Next()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        currentIndex = (currentIndex + 1) % locales.Count;
        LocalizationSettings.SelectedLocale = locales[currentIndex];
    }

    public void Previous()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        currentIndex = (currentIndex - 1 + locales.Count) % locales.Count;
        LocalizationSettings.SelectedLocale = locales[currentIndex];
    }
}