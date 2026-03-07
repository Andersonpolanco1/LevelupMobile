using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Models;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell;

            var saved = Preferences.Get("AppTheme", "Dark");
            UserAppTheme = saved switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };

            // Restaurar idioma guardado al arrancar
            var language = AppPreferences.GetLanguage();
            LocalizationService.Instance.SetLanguage(language);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }
    }
}