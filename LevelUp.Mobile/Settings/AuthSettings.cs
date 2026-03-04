namespace LevelUp.Mobile.Settings
{
    public class AuthSettings
    {
        public const string SettingPath = "AuthSettings";
        public string CallbackScheme { get; set; } = "levelup";
        public string CallbackHost { get; set; } = "callback";
        public string GoogleAuthPath { get; set; } = "mobileauth/google";

        // URL completa construida
        public string GoogleAuthUrl(string baseUrl) => $"{baseUrl}{GoogleAuthPath}";
        public Uri CallbackUri => new($"{CallbackScheme}://{CallbackHost}");
    }

}
