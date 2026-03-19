using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Home.Models;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Home.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly HomeService _homeService;
        private readonly ITokenService _tokenService;

        public HomeViewModel(HomeService homeService, ITokenService tokenService)
        {
            _homeService = homeService;
            _tokenService = tokenService;
            LocalizationService.Instance.PropertyChanged += (_, _) =>
                OnPropertyChanged(nameof(Greeting));
        }

        [ObservableProperty] private string? _userName;
        [ObservableProperty] private string? _profilePictureUrl;
        [ObservableProperty] private Guid? _userId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWorkoutVisible))]
        private TodayPlanDto? _todayPlan;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWorkoutVisible))]
        private bool _hasRestDay;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWorkoutVisible))]
        private bool _hasNoPlan;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWorkoutVisible))]
        private bool _hasDayWithNoExercises;

        public string Greeting => GetGreeting();

        public bool IsWorkoutVisible =>
            TodayPlan != null &&
            !HasRestDay &&
            !HasNoPlan &&
            !HasDayWithNoExercises &&
            TodayPlan.Exercises.Count > 0;

        // ── Inicializar ───────────────────────────────────────────────

        [RelayCommand]
        private async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var claims = await _tokenService.GetUserClaimsAsync();

                if (claims.TryGetValue("userName", out var name))
                    UserName = name;

                if (claims.TryGetValue("sub", out var id))
                    UserId = Guid.Parse(id);

                if (claims.TryGetValue("picture", out var pic) &&
                    !string.IsNullOrEmpty(pic))
                    ProfilePictureUrl = pic;

                if (UserId is null) return;

                var language = AppPreferences.GetLanguage();
                var today = await _homeService.GetTodayAsync(UserId.Value, language);

                TodayPlan = today is null ? null : new TodayPlanDto
                {
                    PlanName = today.PlanName,
                    DayName = today.DayName,
                    DayOfWeek = today.DayOfWeek,
                    Notes = today.Notes,
                    Exercises = today.Exercises.Select(e => new TodayExerciseDto
                    {
                        ExerciseId = e.ExerciseId,
                        ExerciseName = e.ExerciseName,
                        SetsPlanned = e.SetsPlanned,
                        RepsPlanned = e.RepsPlanned,
                        Order = e.Order
                    }).ToList()
                };

                HasNoPlan = false;
                HasRestDay = false;
                HasDayWithNoExercises = false;

                if (TodayPlan is null)
                    HasNoPlan = true;
                else if (TodayPlan.Exercises.Count == 0)
                    HasDayWithNoExercises = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HomeViewModel] ERROR: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
            => await Shell.Current.GoToAsync("///Profile");

        [RelayCommand]
        private async Task CreatePlanAsync()
            => await Shell.Current.GoToAsync("Plans/Create");

        [RelayCommand]
        private async Task StartWorkoutAsync()
            => await Shell.Current.GoToAsync("Workout/Active");

        private static string GetGreeting()
        {
            var key = DateTime.Now.Hour switch
            {
                >= 5 and < 12 => "GoodMorning",
                >= 12 and < 17 => "GoodAfternoon",
                >= 17 and < 21 => "GoodEvening",
                _ => "GoodNight"
            };
            return LocalizationService.Instance[key];
        }
    }
}