using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Session;
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
        private readonly ISessionService _sessionService;

        public App(
            AppShell shell,
            ISyncService sync,
            LocalDatabase database,
            ConnectivitySyncTrigger syncTrigger,
            ISessionService sessionService)
        {
            InitializeComponent();
            _shell = shell;
            _sync = sync;
            _database = database;
            _sessionService = sessionService;

            UserAppTheme = AppPreferences.GetTheme();
            var language = AppPreferences.GetLanguage();
            LocalizationService.Instance.SetLanguage(language);
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(_shell);

        protected override void OnStart()
        {
            base.OnStart();
            _ = Task.Run(async () =>
            {
                try
                {
                    await _database.EnsureInitializedAsync();

                    var hasSession = await _sessionService.TryRestoreAsync();
                    System.Diagnostics.Debug.WriteLine($">>> OnStart: sesión={hasSession}, userId={_sessionService.UserId}");

                    if (!hasSession) return;

                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        System.Diagnostics.Debug.WriteLine($">>> OnStart: full sync init...");
                        await _sync.FullSyncAsync();

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($">>> OnStart ERROR: {ex.Message}");
                }
            });
        }

    }
}