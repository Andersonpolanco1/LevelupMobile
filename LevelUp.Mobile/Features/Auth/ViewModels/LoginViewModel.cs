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

#if DEBUG
            var googleToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI1MDdmNTFhZjJhMTYyNDY3MDc0ODQ2NzRhNDJhZTNjMmI2MjMxOWMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTQ3MDIxOTc3NzcyMjE4MDcwNTYiLCJlbWFpbCI6ImFuZGVyc29ucG9sYW5jb2NvbnRyZXJhc0BnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiYXRfaGFzaCI6IkVKNVBUY0dUcC1fSDZPMWI4eEdUVXciLCJuYW1lIjoiQW5kZXJzb24gUG9sYW5jbyIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BQ2c4b2NLY0tsaHoxZUZybkV2VUFQWlFUeDlWaUdRTFlhTENnRlJyUzc2MHlld04yUi1XaE1LbT1zOTYtYyIsImdpdmVuX25hbWUiOiJBbmRlcnNvbiIsImZhbWlseV9uYW1lIjoiUG9sYW5jbyIsImlhdCI6MTc3MzExMDg1NSwiZXhwIjoxNzczMTE0NDU1fQ.V56rdZzKgTh75m_eg51GAEAxaZITuPoszNADmVjSI9SsjmX3YWkSHr6gjRdXtIuLk53-HQa6UIA34OvTiJMmqcpqk8hdHBw_yMnmB3QvZc9s3WjCydZ_A2VDd47K4_6kI7foN91jLzeWZKTaKEC4SFOTmYI-6JzWfhO5icMKxJtiksfrCppBnzzNKUfDt3pLIreCZOIo6EYNw7axEvgVZaHsSQkcor8G6YY7UHV4zAsWyqJ-mAmwGQD5LsvYyxUZQgQceajSBAV6NMmaNsu6-jkQYINQApB_RKAXq4DU9W1KQPvHnIUTVWKywgRtc5bpbBQqsq0v53-1G_GELFudGw";
            result = await _authService.LoginWithGoogleAsync(googleToken);
#else
            result = await _authService.LoginWithGoogleAsync();
#endif

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