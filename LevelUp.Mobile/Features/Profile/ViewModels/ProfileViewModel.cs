using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Profile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly UserProfileRepository _profileRepo;
        private readonly ISessionService _session;
        private readonly ISyncService _syncService;
        private readonly AuthService _authService;

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

        [ObservableProperty] private string? _bodyWeightDisplay;
        [ObservableProperty] private bool _isDarkTheme;
        [ObservableProperty] private int _selectedLanguageIndex;  // 0=English, 1=Spanish
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeightUnitLabel))]
        private int _selectedWeightUnitIndex; // 0=Lb, 1=Kg

        public string WeightUnitLabel => SelectedWeightUnitIndex == 1 ? "kg" : "lbs";
        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);
        public bool CanSave => !IsSaving;
        public string SaveButtonText => IsSaving ? "Saving…" : "Save Changes";

        // ── Picker sources ────────────────────────────────────────────

        public List<string> Languages { get; } = ["English", "Español"];
        public List<string> WeightUnits { get; } = ["lbs", "kg"];

        // ── Referencia al perfil cargado ──────────────────────────────

        private UserProfile? _profile;

        // ── Constructor ───────────────────────────────────────────────

        public ProfileViewModel(
            UserProfileRepository profileRepo,
            ISessionService session,
            ISyncService syncService,
            AuthService authService)
        {
            _profileRepo = profileRepo;
            _session = session;
            _syncService = syncService;
            _authService = authService;
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

                // Capturar timezone del dispositivo automáticamente
                _profile.TimeZoneId = TimeZoneInfo.Local.Id;

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

        // ── Logout ────────────────────────────────────────────────────

        [RelayCommand]
        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            await _session.ClearUserDataAsync();
            _session.Clear();
            await Shell.Current.GoToAsync("//Login");
        }

        // ── Helpers privados ──────────────────────────────────────────

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
            ProfilePictureUrl = !string.IsNullOrWhiteSpace(p.ProfilePictureUrl) ? p.ProfilePictureUrl : _session.PhotoUrl;

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

            IsDarkTheme = p.PreferredTheme == ThemeMode.Dark;
            SelectedLanguageIndex = p.PreferredLanguage == Language.Spanish ? 1 : 0;
            SelectedWeightUnitIndex = p.PreferredWeightUnit == WeightUnit.Kg ? 1 : 0;
        }

        private void MapFromViewModel(UserProfile p)
        {
            p.PreferredLanguage = SelectedLanguageIndex == 1 ? Language.Spanish : Language.English;
            p.PreferredTheme = IsDarkTheme ? ThemeMode.Dark : ThemeMode.Light;
            p.PreferredWeightUnit = SelectedWeightUnitIndex == 1 ? WeightUnit.Kg : WeightUnit.Lb;

            if (decimal.TryParse(BodyWeightDisplay, out var weight) && weight > 0)
            {
                p.CurrentBodyWeightInLb = p.PreferredWeightUnit == WeightUnit.Kg
                    ? Math.Round(weight / 0.453592m, 2)
                    : weight;
            }
        }

        private UserProfile CreateDefaultProfile() => new()
        {
            Id = _session.UserId,
            Name = _session.UserName,
            Email = _session.Email,
            ProfilePictureUrl = _session.PhotoUrl,
            CreatedAt = DateTime.UtcNow,
            PreferredLanguage = Language.English,
            PreferredTheme = ThemeMode.System,
            PreferredWeightUnit = WeightUnit.Lb,
            TimeZoneId = TimeZoneInfo.Local.Id
        };
    }
}