using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Enums;
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
            LocalizationService.Instance.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Greeting));
        }

        [ObservableProperty] private string? _userName;
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

        public string Greeting => GetGreeting();
        public bool IsWorkoutVisible => TodayPlan != null && !HasRestDay && !HasNoPlan;

        [RelayCommand]
        private async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var claims = await _tokenService.GetUserClaimsAsync();
                if (claims.TryGetValue("userName", out var name)) UserName = name;
                if (claims.TryGetValue("sub", out var id)) UserId = Guid.Parse(id);

                if (UserId is null) return;

                var today = await _homeService.GetTodayAsync(UserId.Value, Language.Spanish);

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

                if (TodayPlan is null)
                {
                    HasNoPlan = true;
                    HasRestDay = false;
                }
                else if (TodayPlan.Exercises.Count == 0)
                {
                    HasRestDay = true;
                    HasNoPlan = false;
                }
                else
                {
                    HasRestDay = false;
                    HasNoPlan = false;
                }
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
        private async Task CreatePlanAsync()
            => await Shell.Current.GoToAsync("///Plans/Create");

        private static string GetGreeting()
        {
            var key = DateTime.Now.Hour switch
            {
                < 12 => "GoodMorning",
                < 18 => "GoodAfternoon",
                _ => "GoodEvening"
            };
            return LocalizationService.Instance[key];
        }
    }
}