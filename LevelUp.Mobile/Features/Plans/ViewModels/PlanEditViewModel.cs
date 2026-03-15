using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
    [QueryProperty(nameof(PlanId), "id")]
    public partial class PlanEditViewModel(WeeklyPlanService planService) : BaseViewModel
    {
        [ObservableProperty] private string? _planId;
        [ObservableProperty] private string _planName = "";
        [ObservableProperty] private string? _planNotes;
        [ObservableProperty] private ObservableCollection<DayEditItem> _dayItems = new();

        private WeeklyPlan? _plan;
        private bool _loaded;

        private static readonly (DayOfWeek Day, string Name)[] AllDays =
        [
            (DayOfWeek.Monday,    "Lunes"),
            (DayOfWeek.Tuesday,   "Martes"),
            (DayOfWeek.Wednesday, "Miércoles"),
            (DayOfWeek.Thursday,  "Jueves"),
            (DayOfWeek.Friday,    "Viernes"),
            (DayOfWeek.Saturday,  "Sábado"),
            (DayOfWeek.Sunday,    "Domingo"),
        ];

        // ── Control de carga ──────────────────────────────────────────

        public void LoadIfNeeded()
        {
            if (_loaded) return;
            LoadCommand.Execute(null);
        }

        public void ResetLoad() => _loaded = false;

        // ── Load ──────────────────────────────────────────────────────

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(PlanId)) return;
            await RunAsync(async () =>
            {
                if (!Guid.TryParse(PlanId, out var id)) return;

                _plan = await planService.GetByIdAsync(id);
                if (_plan is null) return;

                PlanName = _plan.Name;
                PlanNotes = _plan.Notes;

                var existingDays = await planService.GetDaysAsync(id);

                var items = new List<DayEditItem>();
                foreach (var (dow, name) in AllDays)
                {
                    var match = existingDays.FirstOrDefault(d => d.DayOfWeek == dow);
                    var count = match is not null
                        ? await planService.GetExerciseCountForDayAsync(match.Id)
                        : 0;

                    items.Add(new DayEditItem
                    {
                        DayOfWeek = dow,
                        DisplayName = name,
                        IsEnabled = match is not null,
                        ExistingDayId = match?.Id,
                        Notes = match?.Notes,
                        ExerciseCount = count
                    });
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DayItems.Clear();
                    foreach (var item in items)
                        DayItems.Add(item);
                });

                _loaded = true;
            });
        }

        // ── Info del plan ─────────────────────────────────────────────

        [RelayCommand]
        private async Task SavePlanInfoAsync()
        {
            if (_plan is null || string.IsNullOrWhiteSpace(PlanName)) return;
            await RunAsync(async () =>
            {
                await planService.UpdatePlanInfoAsync(_plan.Id, PlanName, PlanNotes);
                await ShowSuccessAsync("Plan actualizado");
            });
        }

        [RelayCommand]
        private async Task DeletePlanAsync()
        {
            if (_plan is null) return;

            bool confirmed = await Shell.Current.DisplayAlertAsync(
                "Eliminar plan",
                $"¿Deseas eliminar \"{_plan.Name}\"? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (!confirmed) return;

            await RunAsync(async () =>
            {
                await planService.DeleteAsync(_plan.Id);
                await Shell.Current.GoToAsync("///Plans");
            });
        }

        // ── Días — sin RunAsync para evitar parpadeo ──────────────────

        [RelayCommand]
        private async Task TapDayAsync(DayEditItem item)
        {
            if (_plan is null) return;

            item.IsEnabled = !item.IsEnabled;

            // ─ Activar ─
            if (item.IsEnabled)
            {
                try
                {
                    var newDay = await planService.AddDayAsync(
                        _plan.Id, item.DayOfWeek, item.Notes);
                    item.ExistingDayId = newDay.Id;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TapDay ADD] {ex.Message}");
                    item.IsEnabled = false;
                }
                return;
            }

            // ─ Desactivar ─
            if (item.ExistingDayId is null) { item.IsEnabled = false; return; }

            if (item.ExerciseCount > 0)
            {
                bool confirmed = await Shell.Current.DisplayAlertAsync(
                    "Eliminar día",
                    $"El día \"{item.DisplayName}\" tiene {item.ExerciseCount} ejercicio(s). " +
                    "¿Deseas eliminarlo de todas formas? Se perderán los ejercicios.",
                    "Eliminar", "Cancelar");

                if (!confirmed) { item.IsEnabled = true; return; }

                try
                {
                    await planService.ForceRemoveDayAsync(item.ExistingDayId.Value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TapDay FORCE REMOVE] {ex.Message}");
                    item.IsEnabled = true;
                    return;
                }
            }
            else
            {
                try
                {
                    await planService.RemoveDayAsync(item.ExistingDayId.Value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TapDay REMOVE] {ex.Message}");
                    item.IsEnabled = true;
                    return;
                }
            }

            item.ExistingDayId = null;
            item.ExerciseCount = 0;
            item.Notes = null;
            item.IsExpandedNotes = false;
        }

        [RelayCommand]
        private async Task SaveDayNotesAsync(DayEditItem item)
        {
            if (item.ExistingDayId is null) return;
            try
            {
                await planService.UpdateDayNotesAsync(item.ExistingDayId.Value, item.Notes);
                item.IsExpandedNotes = false;
                await ShowSuccessAsync("Notas guardadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveDayNotes] {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleNotes(DayEditItem item)
        {
            if (!item.IsEnabled) return;
            item.IsExpandedNotes = !item.IsExpandedNotes;
        }

        // ── Navegación ────────────────────────────────────────────────

        [RelayCommand]
        private async Task GoBackAsync()
            => await Shell.Current.GoToAsync("..");
    }
}