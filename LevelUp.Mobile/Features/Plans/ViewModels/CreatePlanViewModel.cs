using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Plans.ViewModels
{
    public partial class CreatePlanViewModel(WeeklyPlanService planService, ITokenService tokenService) : BaseViewModel
    {
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string? notes;

        public string? NameError => GetFieldError("Name");

        [RelayCommand]
        private async Task CreatePlan()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                FieldErrors = new Dictionary<string, string> { { "Name", "El nombre es obligatorio" } };
                NotifyFieldErrorsChanged();
                return;
            }

            Guid userId = new();
            var claims = await tokenService.GetUserClaimsAsync();
            if(claims.TryGetValue("sub", out var id))
            {
                userId = Guid.Parse(id);
            }


            await RunAsync(async () =>
            {
                await planService.CreateAsync(userId, Name, Notes);
                await ShowSuccessAsync("Plan creado correctamente");
                await Shell.Current.GoToAsync("///Plans");
            });

            NotifyFieldErrorsChanged();
        }

        protected override void NotifyFieldErrorsChanged()
        {
            OnPropertyChanged(nameof(NameError));
        }
    }
}