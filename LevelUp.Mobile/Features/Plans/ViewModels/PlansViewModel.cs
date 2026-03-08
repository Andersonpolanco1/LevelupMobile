using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
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

            LoadPlansCommand = new Command(async () => await LoadPlans());

            CreatePlanCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("///Plans/Create");
            });

            OpenPlanCommand = new Command<WeeklyPlan>(async plan =>
            {
                await Shell.Current.GoToAsync($"///Plans/Detail?id={plan.Id}");
            });
        }

        private async Task LoadPlans()
        {
            Plans.Clear();

            var plans = await _service.GetPlansAsync();

            if (plans == null)
                return;

            foreach (var plan in plans)
                Plans.Add(plan);
        }
    }
}
