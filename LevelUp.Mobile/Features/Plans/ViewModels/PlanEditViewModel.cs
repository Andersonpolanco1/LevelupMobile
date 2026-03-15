// Features/Plans/ViewModels/PlanEditViewModel.cs
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

        // Días en orden Lun→Dom
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

        //partial void OnPlanIdChanged(string? value)
        //{
        //    if (!string.IsNullOrEmpty(value))
        //        LoadCommand.Execute(null);
        //}

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(PlanId)) return;
            await RunAsync(async () =>
            {
                if (!Guid.TryParse(PlanId, out var id)) return;

                // Limpieza temporal — puedes quitarla después de correr una vez
                await planService.DeduplicateDaysAsync(id);

                _plan = await planService.GetByIdAsync(id);
                if (_plan is null) return;

                PlanName = _plan.Name;
                PlanNotes = _plan.Notes;

                var existingDays = await planService.GetDaysAsync(id);

                DayItems.Clear();
                foreach (var (dow, name) in AllDays)
                {
                    var match = existingDays.FirstOrDefault(d => d.DayOfWeek == dow);
                    var count = match is not null
                        ? await planService.GetExerciseCountForDayAsync(match.Id)
                        : 0;

                    DayItems.Add(new DayEditItem
                    {
                        DayOfWeek = dow,
                        DisplayName = name,
                        IsEnabled = match is not null,
                        ExistingDayId = match?.Id,
                        Notes = match?.Notes,
                        ExerciseCount = count
                    });
                }
            });
        }

        // ── Guardar nombre/notas del plan ─────────────────────────────
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

        // ── Toggle día con lógica de confirmación ────────────────────
        // Cambia el nombre del comando para reflejar que es un tap, no un toggle de Switch
        [RelayCommand]
        private async Task TapDayAsync(DayEditItem item)
        {
            if (_plan is null) return;

            // Invertir el estado manualmente
            item.IsEnabled = !item.IsEnabled;

            // ─ Activar día ─
            if (item.IsEnabled)
            {
                var newDay = await planService.AddDayAsync(_plan.Id, item.DayOfWeek, item.Notes);
                item.ExistingDayId = newDay.Id;
                return;
            }

            // ─ Desactivar día ─
            if (item.ExistingDayId is null) return;

            if (item.ExerciseCount > 0)
            {
                bool confirmed = await Shell.Current.DisplayAlert(
                    "Eliminar día",
                    $"El día \"{item.DisplayName}\" tiene {item.ExerciseCount} ejercicio(s) asignado(s). " +
                    "¿Deseas eliminarlo de todas formas? Se perderán los ejercicios.",
                    "Eliminar", "Cancelar");

                if (!confirmed)
                {
                    item.IsEnabled = true; // revertir
                    return;
                }

                await planService.ForceRemoveDayAsync(item.ExistingDayId.Value);
            }
            else
            {
                await planService.RemoveDayAsync(item.ExistingDayId.Value);
            }

            item.ExistingDayId = null;
            item.ExerciseCount = 0;
            item.Notes = null;
            item.IsExpandedNotes = false;
        }

        // ── Guardar notas de un día ───────────────────────────────────
        [RelayCommand]
        private async Task SaveDayNotesAsync(DayEditItem item)
        {
            if (item.ExistingDayId is null) return;
            await planService.UpdateDayNotesAsync(item.ExistingDayId.Value, item.Notes);
            item.IsExpandedNotes = false;
            await ShowSuccessAsync("Notas guardadas");
        }

        [RelayCommand]
        private void ToggleNotes(DayEditItem item)
        {
            if (!item.IsEnabled) return;
            item.IsExpandedNotes = !item.IsExpandedNotes;
        }

        // ── Volver ────────────────────────────────────────────────────
        [RelayCommand]
        private async Task GoBackAsync()
            => await Shell.Current.GoToAsync("..");
    }
}