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
        [ObservableProperty] private TodayPlanDto? _todayPlan;
        [ObservableProperty] private bool _hasRestDay;
        [ObservableProperty] private bool _hasNoPlan;

        public string Greeting => GetGreeting();

        // ── Init ──────────────────────────────────────────────────────────
        [RelayCommand]
        private async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Nombre desde JWT — sin llamada extra a la API
                var claims = await _tokenService.GetUserClaimsAsync();
                claims.TryGetValue("userName", out var name);
                UserName = name;

                // Plan de hoy
                TodayPlan = await _homeService.GetTodayPlanAsync();

                if (TodayPlan is null)
                {
                    HasNoPlan = true;
                    HasRestDay = false;
                }
                else if (TodayPlan.Exercises.Count == 0)
                {
                    HasRestDay = true;
                    HasNoPlan = false;
                    TodayPlan = null; // limpiar para que el IsNotNullConverter oculte esa sección
                }
                else
                {
                    HasRestDay = false;
                    HasNoPlan = false;
                }
            }
            catch (Exception ex)
            {
                // Manejo de error — puedes mostrar un mensaje
                System.Diagnostics.Debug.WriteLine($"HomeViewModel error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
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
