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

        public string ActivateButtonText => Plan?.IsActive == true
            ? LocalizationService.Instance["PlanActive"]
            : LocalizationService.Instance["ActivatePlan"];

        private static string GetDayShortName(DayOfWeek day) => day switch
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

        private static string GetDayFullName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => LocalizationService.Instance["DayMonFull"],
            DayOfWeek.Tuesday => LocalizationService.Instance["DayTueFull"],
            DayOfWeek.Wednesday => LocalizationService.Instance["DayWedFull"],
            DayOfWeek.Thursday => LocalizationService.Instance["DayThuFull"],
            DayOfWeek.Friday => LocalizationService.Instance["DayFriFull"],
            DayOfWeek.Saturday => LocalizationService.Instance["DaySatFull"],
            DayOfWeek.Sunday => LocalizationService.Instance["DaySunFull"],
            _ => day.ToString()
        };

        [RelayCommand]
        private async Task GoBackAsync()
            => await Shell.Current.GoToAsync("..");

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

                Plan.DaysOfWeekShortName = rawDays
                    .OrderBy(d => d.DayOfWeek)
                    .Select(d => GetDayShortName(d.DayOfWeek))
                    .ToArray();

                var items = new List<DayWithCount>();
                foreach (var d in rawDays.OrderBy(d => d.DayOfWeek))
                {
                    var count = await planService.GetExerciseCountForDayAsync(d.Id);
                    items.Add(new DayWithCount
                    {
                        Day = d,
                        ExerciseCount = count,
                        DayName = GetDayFullName(d.DayOfWeek)
                    });
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Days.Clear();
                    foreach (var item in items)
                        Days.Add(item);
                });
            });
        }

        public void ReloadIfNeeded()
        {
            if (!string.IsNullOrEmpty(PlanId))
                LoadCommand.Execute(null);
        }

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

        [RelayCommand]
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
            });
        }
    }
}