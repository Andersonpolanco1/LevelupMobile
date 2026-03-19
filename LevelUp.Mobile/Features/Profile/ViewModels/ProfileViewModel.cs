using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Profile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly UserProfileRepository _profileRepo;
        private readonly ISessionService _session;
        private readonly ISyncService _syncService;

        // ── Estado ────────────────────────────────────────────────────

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSave))]
        [NotifyPropertyChangedFor(nameof(SaveButtonText))]
        private bool _isSaving;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private bool _isSaved;

        // ── Datos de solo lectura (del JWT) ───────────────────────────

        [ObservableProperty] private string? _name;
        [ObservableProperty] private string? _email;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasProfilePicture))]
        private string? _profilePictureUrl;

        // ── Configuraciones editables ─────────────────────────────────

        [ObservableProperty] private string? _bodyWeightDisplay;  // string para el Entry
        [ObservableProperty] private bool _isDarkTheme;
        [ObservableProperty] private int _selectedLanguageIndex;  // 0=English, 1=Spanish
        [ObservableProperty] private int _selectedTimezoneIndex;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeightUnitLabel))]
        private int _selectedWeightUnitIndex; // 0=Lb, 1=Kg

        /// <summary>Label calculado para mostrar junto al Entry de peso. Evita indexers en XAML.</summary>
        public string WeightUnitLabel => SelectedWeightUnitIndex == 1 ? "kg" : "lbs";

        /// <summary>True si hay foto de perfil — reemplaza NotNullConverter en XAML.</summary>
        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);

        /// <summary>True cuando no se está guardando — reemplaza InvertedBoolConverter en XAML.</summary>
        public bool CanSave => !IsSaving;

        /// <summary>Texto del botón guardar según estado.</summary>
        public string SaveButtonText => IsSaving ? "Saving…" : "Save Changes";

        // ── Picker sources ────────────────────────────────────────────

        public List<string> Languages { get; } = ["English", "Español"];

        public List<string> WeightUnits { get; } = ["lbs", "kg"];

        public List<string> Timezones { get; } =
        [
            // ── América ───────────────────────────────────────────────
            "America/New_York",
            "America/Chicago",
            "America/Denver",
            "America/Los_Angeles",
            "America/Anchorage",
            "Pacific/Honolulu",
            "America/Toronto",
            "America/Vancouver",
            "America/Mexico_City",
            "America/Santo_Domingo",
            "America/Puerto_Rico",
            "America/Bogota",
            "America/Lima",
            "America/Caracas",
            "America/Santiago",
            "America/Argentina/Buenos_Aires",
            "America/Sao_Paulo",
            // ── Europa ────────────────────────────────────────────────
            "Europe/London",
            "Europe/Lisbon",
            "Europe/Madrid",
            "Europe/Paris",
            "Europe/Berlin",
            "Europe/Rome",
            "Europe/Amsterdam",
            "Europe/Stockholm",
            "Europe/Warsaw",
            "Europe/Athens",
            "Europe/Istanbul",
            "Europe/Moscow",
            // ── África ────────────────────────────────────────────────
            "Africa/Cairo",
            "Africa/Johannesburg",
            "Africa/Lagos",
            // ── Asia ──────────────────────────────────────────────────
            "Asia/Dubai",
            "Asia/Karachi",
            "Asia/Kolkata",
            "Asia/Dhaka",
            "Asia/Bangkok",
            "Asia/Singapore",
            "Asia/Shanghai",
            "Asia/Tokyo",
            "Asia/Seoul",
            // ── Oceanía ───────────────────────────────────────────────
            "Australia/Sydney",
            "Pacific/Auckland",
        ];

        // ── Referencia al perfil cargado ──────────────────────────────

        private UserProfile? _profile;

        // ── Constructor ───────────────────────────────────────────────

        public ProfileViewModel(
            UserProfileRepository profileRepo,
            ISessionService session,
            ISyncService syncService)
        {
            _profileRepo = profileRepo;
            _session = session;
            _syncService = syncService;
        }

        // ── Inicializar ───────────────────────────────────────────────

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsLoading = true;
            HasError = false;
            IsSaved = false;
            ErrorMessage = null;

            try
            {
                // Cargar perfil local (puede no existir en primer login)
                _profile = await _profileRepo.GetByUserIdAsync(_session.UserId)
                    ?? CreateDefaultProfile();

                MapToViewModel(_profile);
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = LocalizationService.Instance["ProfileErrorLoad"];
                System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] LoadAsync error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Guardar ───────────────────────────────────────────────────

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (_profile is null) return;

            IsSaving = true;
            HasError = false;
            IsSaved = false;
            ErrorMessage = null;

            try
            {
                MapFromViewModel(_profile);

                _profile.UpdatedAt = DateTime.UtcNow;
                _profile.IsSynced = false;

                await _profileRepo.UpsertAsync(_profile);

                ApplyPreferences(_profile);

                try
                {
                    await _syncService.PushProfileAsync(_profile);
                    _profile.IsSynced = true;
                    await _profileRepo.UpsertAsync(_profile);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("[ProfileViewModel] SaveAsync: sin red, guardado local.");
                }

                IsSaved = true;

                await Task.Delay(2000);
                IsSaved = false;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = LocalizationService.Instance["ProfileErrorSave"];
                System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] SaveAsync error: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }


        private static void ApplyPreferences(UserProfile p)
        {
            AppPreferences.SetLanguage(p.PreferredLanguage);
            LocalizationService.Instance.SetLanguage(p.PreferredLanguage);

            var appTheme = p.PreferredTheme switch
            {
                ThemeMode.Light => AppTheme.Light,
                ThemeMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified   
            };

            AppPreferences.SetTheme(appTheme);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current is not null)
                    Application.Current.UserAppTheme = appTheme;
            });
        }

        private void MapToViewModel(UserProfile p)
        {
            Name = !string.IsNullOrWhiteSpace(p.Name) ? p.Name : _session.UserName;
            Email = !string.IsNullOrWhiteSpace(p.Email) ? p.Email : _session.Email;
            ProfilePictureUrl = !string.IsNullOrWhiteSpace(p.ProfilePictureUrl)
                ? p.ProfilePictureUrl
                : _session.PhotoUrl;

            if (p.CurrentBodyWeightInLb.HasValue)
            {
                var display = p.PreferredWeightUnit == WeightUnit.Kg
                    ? Math.Round(p.CurrentBodyWeightInLb.Value * 0.453592m, 1)
                    : Math.Round(p.CurrentBodyWeightInLb.Value, 1);
                BodyWeightDisplay = display.ToString("0.#");
            }
            else
            {
                BodyWeightDisplay = string.Empty;
            }

            // Tema
            IsDarkTheme = p.PreferredTheme == ThemeMode.Dark;

            // Idioma
            SelectedLanguageIndex = p.PreferredLanguage == Language.Spanish ? 1 : 0;

            // Unidad de peso
            SelectedWeightUnitIndex = p.PreferredWeightUnit == WeightUnit.Kg ? 1 : 0;

            // Timezone
            var tzIndex = Timezones.IndexOf(p.TimeZoneId ?? string.Empty);
            SelectedTimezoneIndex = tzIndex >= 0 ? tzIndex : 0;
        }

        private void MapFromViewModel(UserProfile p)
        {
            // Idioma
            p.PreferredLanguage = SelectedLanguageIndex == 1
                ? Language.Spanish
                : Language.English;

            // Tema
            p.PreferredTheme = IsDarkTheme ? ThemeMode.Dark : ThemeMode.Light;

            // Unidad de peso
            p.PreferredWeightUnit = SelectedWeightUnitIndex == 1
                ? WeightUnit.Kg
                : WeightUnit.Lb;

            // Peso corporal — convertir a Lb para almacenar
            if (decimal.TryParse(BodyWeightDisplay, out var weight) && weight > 0)
            {
                p.CurrentBodyWeightInLb = p.PreferredWeightUnit == WeightUnit.Kg
                    ? Math.Round(weight / 0.453592m, 2)
                    : weight;
            }

            // Timezone
            if (SelectedTimezoneIndex >= 0 && SelectedTimezoneIndex < Timezones.Count)
                p.TimeZoneId = Timezones[SelectedTimezoneIndex];
        }

        private UserProfile CreateDefaultProfile()
        {
            return new UserProfile
            {
                Id = _session.UserId,
                Name = _session.UserName,
                Email = _session.Email,
                ProfilePictureUrl = _session.PhotoUrl,
                CreatedAt = DateTime.UtcNow,
                PreferredLanguage = Language.English,
                PreferredTheme = ThemeMode.System,
                PreferredWeightUnit = WeightUnit.Lb
            };
        }
    }
}
