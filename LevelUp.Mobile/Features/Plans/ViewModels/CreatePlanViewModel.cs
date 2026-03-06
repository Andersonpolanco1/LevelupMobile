using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
    public partial class CreatePlanViewModel : BaseViewModel
    {
        private readonly WeeklyPlanService _planService;

        public CreatePlanViewModel(WeeklyPlanService planService)
        {
            _planService = planService;
        }

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string? notes;

        [RelayCommand]
        private async Task CreatePlan()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Error", "El nombre es obligatorio", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                var plan = await _planService.CreateWeeklyPlanAsync(Name, Notes);

                if (plan == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo crear el plan", "OK");
                    return;
                }

                await Shell.Current.DisplayAlert("Plan creado", "Ahora agrega los días del plan", "OK");

                // navegar a agregar días
                //await Shell.Current.GoToAsync($"plandays?planId={plan.Id}");
                await Shell.Current.GoToAsync("///Plans");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
