// Features/Auth/ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Services;

namespace LevelUp.Mobile.Features.Auth.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ISessionService _session;
    private readonly ISyncService _sync;

    public LoginViewModel(AuthService authService, ISessionService session, ISyncService sync)
    {
        _authService = authService;
        _session = session;
        _sync = sync;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoggedIn))]
    private bool _isLoggedIn;

    [ObservableProperty] private Guid? _userId;
    [ObservableProperty] private string? _userName;
    [ObservableProperty] private string? _userEmail;
    [ObservableProperty] private string? _profilePhotoUrl;

    public bool IsNotLoggedIn => !IsLoggedIn;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginWithGoogleAsync()
    {
        IsBusy = true;
        try
        {
            AuthResult result;

                        result = await _authService.LoginWithGoogleAsync();

            if (!result.IsSuccess) return;

            await _session.TryRestoreAsync();

            UserId = _session.UserId;
            UserName = _session.UserName;
            UserEmail = _session.Email;
            ProfilePhotoUrl = _session.PhotoUrl;
            IsLoggedIn = true;

            _ = Task.Run(() => _sync.FullSyncAsync());

            await Shell.Current.GoToAsync("//Home");
        }
        finally
        {
            IsBusy = false;
            LoginWithGoogleCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await _session.ClearUserDataAsync();  // ← limpia SQLite
        _session.Clear();                     // ← limpia memoria

        IsLoggedIn = false;
        UserId = null;
        UserName = null;
        UserEmail = null;
        ProfilePhotoUrl = null;

        await Shell.Current.GoToAsync("//Login");
    }

    private bool CanLogin() => !IsBusy;
}