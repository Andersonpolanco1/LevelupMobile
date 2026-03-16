using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Settings
{
    public static class AppPreferences
    {
        private const string LanguageKey = "preferred_language";
        private const string ThemeKey = "AppTheme";

        // ── Idioma ────────────────────────────────────────────────────
        public static void SetLanguage(Language language) =>
            Preferences.Set(LanguageKey, (int)language);

        public static Language GetLanguage() =>
            (Language)Preferences.Get(LanguageKey, (int)Language.Spanish);

        // ── Tema ──────────────────────────────────────────────────────
        public static void SetTheme(AppTheme theme)
        {
            var value = theme switch
            {
                AppTheme.Light => "Light",
                AppTheme.Unspecified => "System",
                _ => "Dark"
            };
            Preferences.Set(ThemeKey, value);
        }

        public static AppTheme GetTheme()
        {
            var saved = Preferences.Get(ThemeKey, "Dark");
            return saved switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };
        }
    }
}