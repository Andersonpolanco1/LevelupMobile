// Features/Plans/ViewModels/PlanDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Features.Plans.Models;
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
        [ObservableProperty] private string? _planId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotActive))]
        [NotifyPropertyChangedFor(nameof(ActivateButtonText))]
        private WeeklyPlan? _plan;

        [ObservableProperty]
        private ObservableCollection<DayWithCount> _days = new();

        public bool IsNotActive => Plan is not null && !Plan.IsActive;
        public string ActivateButtonText =>
            Plan?.IsActive == true ? "Plan activo" : "Activar plan";

        private static readonly Dictionary<DayOfWeek, string> DayNames = new()
        {
            { DayOfWeek.Monday,    "Lunes" },
            { DayOfWeek.Tuesday,   "Martes" },
            { DayOfWeek.Wednesday, "Miércoles" },
            { DayOfWeek.Thursday,  "Jueves" },
            { DayOfWeek.Friday,    "Viernes" },
            { DayOfWeek.Saturday,  "Sábado" },
            { DayOfWeek.Sunday,    "Domingo" },
        };

        //partial void OnPlanIdChanged(string? value)
        //{
        //    if (!string.IsNullOrEmpty(value))
        //        LoadCommand.Execute(null);
        //}

        // ── Load ─────────────────────────────────────────────────────

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(PlanId)) return;
            await RunAsync(async () =>
            {
                if (!Guid.TryParse(PlanId, out var id)) return;

                Plan = await planService.GetByIdAsync(id);
                if (Plan is null) return;

                var rawDays = await planService.GetDaysAsync(id);

                Days.Clear();
                foreach (var d in rawDays.OrderBy(d => d.DayOfWeek))
                {
                    var count = await planService.GetExerciseCountForDayAsync(d.Id);
                    Days.Add(new DayWithCount
                    {
                        Day = d,
                        ExerciseCount = count,
                        DayName = DayNames.TryGetValue(d.DayOfWeek, out var n)
                            ? n
                            : d.DayOfWeek.ToString()
                    });
                }
            });
        }

        // Se llama desde PlanDetailPage.xaml.cs → OnAppearing
        // Recarga al volver de PlanEditPage con días actualizados
        public void ReloadIfNeeded()
        {
            if (!string.IsNullOrEmpty(PlanId))
                LoadCommand.Execute(null);
        }

        // ── Navegación ────────────────────────────────────────────────

        [RelayCommand]
        private async Task OpenDayAsync(DayWithCount item)
        {
            await Shell.Current.GoToAsync(
                $"PlanDayDetail?dayId={item.Day.Id}&dayName={Uri.EscapeDataString(item.DayName)}");
        }

        [RelayCommand]
        private async Task EditPlanAsync()
        {
            if (Plan is null) return;
            await Shell.Current.GoToAsync($"PlanEdit?id={Plan.Id}");
        }

        // ── Acciones ──────────────────────────────────────────────────

        [RelayCommand(CanExecute = nameof(IsNotActive))]
        private async Task ActivatePlanAsync()
        {
            if (Plan is null) return;
            var claims = await tokenService.GetUserClaimsAsync();
            if (!claims.TryGetValue("sub", out var idStr) ||
                !Guid.TryParse(idStr, out var userId)) return;

            await RunAsync(async () =>
            {
                await planService.ActivateAsync(userId, Plan.Id);
                Plan = await planService.GetByIdAsync(Plan.Id);
                await ShowSuccessAsync("¡Plan activado!");
            });
            ActivatePlanCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private async Task DeletePlanAsync()
        {
            if (Plan is null) return;
            bool confirmed = await Shell.Current.DisplayAlert(
                "Eliminar plan",
                $"¿Deseas eliminar \"{Plan.Name}\"? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmed) return;

            await RunAsync(async () =>
            {
                await planService.DeleteAsync(Plan.Id);
                await Shell.Current.GoToAsync("///Plans");
            });
        }
    }
}