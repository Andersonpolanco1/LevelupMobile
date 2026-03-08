using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Sync;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile
{
    // App.xaml.cs
    public partial class App : Application
    {
        private readonly AppShell _shell;
        private readonly ISyncService _sync;
        private readonly LocalDatabase _database;

        public App(
            AppShell shell,
            ISyncService sync,
            LocalDatabase database,
            ConnectivitySyncTrigger syncTrigger)
        {
            InitializeComponent();
            _shell = shell;
            _sync = sync;
            _database = database;

            var saved = Preferences.Get("AppTheme", "Dark");
            UserAppTheme = saved switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };

            var language = AppPreferences.GetLanguage();
            LocalizationService.Instance.SetLanguage(language);
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(_shell);

        // App.xaml.cs
        protected override void OnStart()
        {
            base.OnStart();

            _ = Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(">>> OnStart: inicializando DB...");
                    await _database.EnsureInitializedAsync();
                    System.Diagnostics.Debug.WriteLine(">>> OnStart: DB lista");

                    System.Diagnostics.Debug.WriteLine(">>> OnStart: verificando conectividad...");
                    var network = Connectivity.NetworkAccess;
                    System.Diagnostics.Debug.WriteLine($">>> OnStart: red = {network}");

                    if (network == NetworkAccess.Internet)
                    {
                        System.Diagnostics.Debug.WriteLine(">>> OnStart: iniciando FullSync...");
                        await _sync.FullSyncAsync();
                        System.Diagnostics.Debug.WriteLine(">>> OnStart: FullSync completado");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(">>> OnStart: sin red, sync omitido");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($">>> OnStart ERROR: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($">>> OnStart INNER: {ex.InnerException?.Message}");
                    System.Diagnostics.Debug.WriteLine($">>> OnStart STACK: {ex.StackTrace}");
                }
            });
        }
    }
}