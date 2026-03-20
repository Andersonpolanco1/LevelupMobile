// Features/Plans/ViewModels/PlansViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Plans.ViewModels;

public partial class PlansViewModel(
    WeeklyPlanService planService,
    AppState appState) : ObservableObject
{
    public ObservableCollection<WeeklyPlan> Plans { get; } = [];

    [ObservableProperty] private bool isBusy;

    // ── Carga inteligente ─────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadIfNeededAsync()
    {
        if (!appState.PlansNeedsRefresh()) return;
        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var plans = await planService.GetPlansAsync();
            if (plans is null) return;

            Plans.Clear();
            foreach (var plan in plans)
            {
                var days = await planService.GetDaysAsync(plan.Id);
                plan.DaysOfWeekShortName = days
                    .OrderBy(d => d.DayOfWeek)
                    .Select(d => GetShortName(d.DayOfWeek))
                    .ToArray();
                Plans.Add(plan);
            }

            appState.PlansLoaded();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Navegación ────────────────────────────────────────────────────

    [RelayCommand]
    private async Task CreatePlanAsync()
        => await Shell.Current.GoToAsync("///Plans/Create");

    [RelayCommand]
    private async Task OpenPlanAsync(WeeklyPlan plan)
        => await Shell.Current.GoToAsync($"///Plans/Detail?id={plan.Id}");

    // ── Helper ────────────────────────────────────────────────────────

    private static string GetShortName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => LocalizationService.Instance["DayMon"],
        DayOfWeek.Tuesday => LocalizationService.Instance["DayTue"],
        DayOfWeek.Wednesday => LocalizationService.Instance["DayWed"],
        DayOfWeek.Thursday => LocalizationService.Instance["DayThu"],
        DayOfWeek.Friday => LocalizationService.Instance["DayFri"],
        DayOfWeek.Saturday => LocalizationService.Instance["DaySat"],
        DayOfWeek.Sunday => LocalizationService.Instance["DaySun"],
        _ => ""
    };
}