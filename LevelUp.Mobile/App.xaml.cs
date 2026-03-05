namespace LevelUp.Mobile
{
    public partial class App : Application
    {
        public App(AppShell shell)
        {
            InitializeComponent();

            // Dark por defecto, pero si el usuario ya eligió uno, respetarlo
            var saved = Preferences.Get("AppTheme", "Dark");

            Application.Current!.UserAppTheme = saved switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };

            MainPage = shell;
        }
    }
}