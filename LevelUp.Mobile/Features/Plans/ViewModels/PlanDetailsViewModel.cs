using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
    [QueryProperty(nameof(PlanId), "id")]
    public partial class PlanDetailViewModel(
        WeeklyPlanService planService,
        ITokenService tokenService) : BaseViewModel
    {
        // ── Query param ───────────────────────────────────────────────
        [ObservableProperty] private string? _planId;

        // ── Plan data ─────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotActive))]
        [NotifyPropertyChangedFor(nameof(ActivateButtonText))]
        private WeeklyPlan? _plan;

        [ObservableProperty] private ObservableCollection<WeeklyPlanDay> _days = new();

        // ── Derived ───────────────────────────────────────────────────
        public bool IsNotActive => Plan is not null && !Plan.IsActive;
        public string ActivateButtonText => Plan?.IsActive == true ? "Plan activo" : "Activar plan";

        // ── Load ──────────────────────────────────────────────────────
        partial void OnPlanIdChanged(string? value)
        {
            if (!string.IsNullOrEmpty(value))
                LoadCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(PlanId)) return;

            await RunAsync(async () =>
            {
                if (!Guid.TryParse(PlanId, out var id)) return;

                Plan = await planService.GetByIdAsync(id);
                if (Plan is null) return;

                var days = await planService.GetDaysAsync(id);

                Days.Clear();
                foreach (var day in days.OrderBy(d => d.DayOfWeek))
                    Days.Add(day);
            });
        }


        [RelayCommand(CanExecute = nameof(IsNotActive))]
        private async Task ActivatePlanAsync()
        {
            if (Plan is null) return;

            var claims = await tokenService.GetUserClaimsAsync();
            if (!claims.TryGetValue("sub", out var idStr) || !Guid.TryParse(idStr, out var userId))
                return;

            await RunAsync(async () =>
            {
                await planService.ActivateAsync(userId, Plan.Id);

                // ✅ Recargar desde DB — garantiza que la UI refleja el estado real
                Plan = await planService.GetByIdAsync(Plan.Id);

                await ShowSuccessAsync("¡Plan activado!");
            });

            ActivatePlanCommand.NotifyCanExecuteChanged();
        }

        // ── Delete ────────────────────────────────────────────────────
        [RelayCommand]
        private async Task DeletePlanAsync()
        {
            if (Plan is null) return;

            bool confirmed = await Shell.Current.DisplayAlert(
                "Eliminar plan",
                $"¿Deseas eliminar \"{Plan.Name}\"? Esta acción no se puede deshacer.",
                "Eliminar",
                "Cancelar");

            if (!confirmed) return;

            await RunAsync(async () =>
            {
                await planService.DeleteAsync(Plan.Id);
                await Shell.Current.GoToAsync("///Plans");
            });
        }
    }
}