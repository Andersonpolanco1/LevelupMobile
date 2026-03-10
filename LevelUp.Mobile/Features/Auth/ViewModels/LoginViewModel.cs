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

//#if DEBUG
//            var googleToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI1MDdmNTFhZjJhMTYyNDY3MDc0ODQ2NzRhNDJhZTNjMmI2MjMxOWMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTQ3MDIxOTc3NzcyMjE4MDcwNTYiLCJlbWFpbCI6ImFuZGVyc29ucG9sYW5jb2NvbnRyZXJhc0BnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiYXRfaGFzaCI6InZNMWtmZUdrZEp5Q09jWkVmLXdmRHciLCJuYW1lIjoiQW5kZXJzb24gUG9sYW5jbyIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BQ2c4b2NLY0tsaHoxZUZybkV2VUFQWlFUeDlWaUdRTFlhTENnRlJyUzc2MHlld04yUi1XaE1LbT1zOTYtYyIsImdpdmVuX25hbWUiOiJBbmRlcnNvbiIsImZhbWlseV9uYW1lIjoiUG9sYW5jbyIsImlhdCI6MTc3MzA2MzQ5NywiZXhwIjoxNzczMDY3MDk3fQ.TI35PYFB-OzurzKPS91XXpip8F0M1Snrt_DebsEWhQCfWxw3CEG7GdY0hZhKB0QlYbV-aHoPAk-iB-MQ2oBKca6-jzpGTX0fEa4dYl2Ce2BoCXKjR6S9T25KNKz_cfx5GUJY4HW061wLvuUE0zkceE3AvIxacIen9OiWygOnqkWanWeBDbTbtMDI8sXroEoyEwADWKcYMf1Kr3h3OEmk19MggrBxkEJFDjtfeNP79fSlhX-OEp_cTIbrbFvMcWfkSR6O6OEZwHLoHBwhC3uaN54XDpvBizSckbpLOHJ60oAmqw7K1dTGum7VK6sWWxdK9Q256h4NGSMkXCmN1GtzFw";
//            result = await _authService.LoginWithGoogleAsync(googleToken);
//#else
//#endif
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