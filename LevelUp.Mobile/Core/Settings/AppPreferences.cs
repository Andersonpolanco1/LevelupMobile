using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Settings
{
    public static class AppPreferences
    {
        private const string LanguageKey = "preferred_language";

        public static void SetLanguage(Language language) =>
            Preferences.Set(LanguageKey, (int)language);

        public static Language GetLanguage() =>
            (Language)Preferences.Get(LanguageKey, (int)Language.English);
    }
}
