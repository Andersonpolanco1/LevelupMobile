namespace LevelUp.Mobile.Settings
{
    public class ApiSettings
    {
        public const string SettingPath = "ApiSettings";
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
