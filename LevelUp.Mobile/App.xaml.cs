using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Infrastructure.Database;
using LevelUp.Mobile.Services;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Mobile
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        public App(AppShell shell, DatabaseService database)
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

            Task.Run(async () =>
            {
                await database.InitializeAsync();
            });

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }
    }
}