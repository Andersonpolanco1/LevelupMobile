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
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }
    }
}