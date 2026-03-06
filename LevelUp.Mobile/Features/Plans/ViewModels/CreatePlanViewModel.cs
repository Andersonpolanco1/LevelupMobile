using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
    public partial class CreatePlanViewModel(WeeklyPlanService planService) : BaseViewModel
    {
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string? notes;

        public string? NameError => GetFieldError("Name");

        [RelayCommand]
        private async Task CreatePlan()
        {
            // Validación local
            if (string.IsNullOrWhiteSpace(Name))
            {
                FieldErrors = new Dictionary<string, string> { { "Name", "El nombre es obligatorio" } };
                NotifyFieldErrorsChanged();
                return;
            }

            await RunAsync(async () =>
            {
                await planService.CreateWeeklyPlanAsync(Name, Notes);
                await Shell.Current.GoToAsync("///Plans");
            });

            // Notifica errores de campo que pudo haber puesto RunAsync
            NotifyFieldErrorsChanged();
        }

        protected override void NotifyFieldErrorsChanged()
        {
            OnPropertyChanged(nameof(NameError));
        }
    }
}