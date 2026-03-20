using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Repositories;
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
        private readonly UserProfileRepository _userProfileRepository;

        public App(
            AppShell shell,
            ISyncService sync,
            LocalDatabase database,
            ConnectivitySyncTrigger syncTrigger,
            ISessionService sessionService,
            UserProfileRepository profileRepo)
        {
            InitializeComponent();
            _shell = shell;
            _sync = sync;
            _database = database;
            _sessionService = sessionService;
            _userProfileRepository = profileRepo;

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
                    if (!hasSession) return;

                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                        await _sync.FullSyncAsync();

                    // ── Aplicar preferencias del perfil sincronizado ───────
                    var profile = await _userProfileRepository.GetByUserIdAsync(_sessionService.UserId);
                    if (profile is not null)
                    {
                        AppPreferences.SetTheme(profile.PreferredTheme);
                        AppPreferences.SetLanguage(profile.PreferredLanguage);
                        LocalizationService.Instance.SetLanguage(profile.PreferredLanguage);

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (Application.Current is not null)
                                Application.Current.UserAppTheme = profile.PreferredTheme;
                        });
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