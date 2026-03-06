using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
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
        }

        // ── Estado ────────────────────────────────────────────────────────
        [ObservableProperty] private string? _userName;

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

        // Propiedad calculada que decide si se muestra la sección "Plan de hoy"
        public bool IsWorkoutVisible => TodayPlan != null && !HasRestDay && !HasNoPlan;

        // ── Init ──────────────────────────────────────────────────────────
        [RelayCommand]
        private async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Nombre desde JWT
                var claims = await _tokenService.GetUserClaimsAsync();
                if (claims.TryGetValue("userName", out var name))
                {
                    UserName = name;
                }

                // Obtener Plan (Maneja el 204 devolviendo null)
                TodayPlan = await _homeService.GetTodayPlanAsync();

                if (TodayPlan is null)
                {
                    HasNoPlan = true;
                    HasRestDay = false;
                }
                else if (TodayPlan.Exercises == null || TodayPlan.Exercises.Count == 0)
                {
                    // Si el objeto existe pero no hay ejercicios, es día de descanso
                    HasRestDay = true;
                    HasNoPlan = false;
                }
                else
                {
                    // Todo en orden, hay ejercicios que mostrar
                    HasRestDay = false;
                    HasNoPlan = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeViewModel error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreatePlanAsync()
        {
            await Shell.Current.GoToAsync("///Plans/Create");
        }

        private static string GetGreeting()
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Buenos días",
                < 18 => "Buenas tardes",
                _ => "Buenas noches"
            };
        }
    }
}