// Features/Plans/ViewModels/CreatePlanViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Plans.ViewModels;

public partial class CreatePlanViewModel(
    WeeklyPlanService planService,
    ITokenService tokenService,
    AppState appState) : BaseViewModel
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string? notes;

    public string? NameError => GetFieldError("Name");

    [RelayCommand]
    private async Task CreatePlanAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            FieldErrors = new Dictionary<string, string> { { "Name", "El nombre es obligatorio" } };
            NotifyFieldErrorsChanged();
            return;
        }

        var claims = await tokenService.GetUserClaimsAsync();
        if (!claims.TryGetValue("sub", out var idStr) ||
            !Guid.TryParse(idStr, out var userId)) return;

        await RunAsync(async () =>
        {
            await planService.CreateAsync(userId, Name, Notes);

            appState.InvalidatePlans();

            await Shell.Current.GoToAsync("///Plans");
        });

        NotifyFieldErrorsChanged();
    }

    protected override void NotifyFieldErrorsChanged()
        => OnPropertyChanged(nameof(NameError));
}