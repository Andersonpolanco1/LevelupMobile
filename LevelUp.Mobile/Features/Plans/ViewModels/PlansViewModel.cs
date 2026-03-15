// Features/Plans/ViewModels/PlansViewModel.cs
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LevelUp.Mobile.Features.Plans.ViewModels;

public class PlansViewModel
{
    private readonly WeeklyPlanService _service;

    public ObservableCollection<WeeklyPlan> Plans { get; } = new();
    public ICommand LoadPlansCommand { get; }
    public ICommand CreatePlanCommand { get; }
    public ICommand OpenPlanCommand { get; }

    public PlansViewModel(WeeklyPlanService service)
    {
        _service = service;

        LoadPlansCommand = new Command(async () => await LoadPlansAsync());

        CreatePlanCommand = new Command(async () =>
            await Shell.Current.GoToAsync("///Plans/Create"));

        OpenPlanCommand = new Command<WeeklyPlan>(async plan =>
            await Shell.Current.GoToAsync($"///Plans/Detail?id={plan.Id}"));
    }

    private async Task LoadPlansAsync()
    {
        Plans.Clear();
        var plans = await _service.GetPlansAsync();
        if (plans is null) return;

        foreach (var plan in plans)
        {
            var days = await _service.GetDaysAsync(plan.Id);
            plan.DaysOfWeekShortName = days
                .OrderBy(d => d.DayOfWeek)
                .Select(d => GetShortName(d.DayOfWeek))
                .ToArray();
            Plans.Add(plan);
        }
    }

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