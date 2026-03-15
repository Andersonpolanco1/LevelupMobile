using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace LevelUp.Mobile.Platforms.Android
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize | ConfigChanges.Orientation |
            ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppThemeChanged(Microsoft.Maui.Controls.Application.Current?.UserAppTheme
                            ?? AppTheme.Unspecified);

            Microsoft.Maui.Controls.Application.Current!.RequestedThemeChanged +=
                (_, e) => AppThemeChanged(e.RequestedTheme);
        }

        private void AppThemeChanged(AppTheme theme)
        {
            if (Window is null) return;

            if (theme == AppTheme.Unspecified)
            {
                var uiMode = Resources?.Configuration?.UiMode
                             & global::Android.Content.Res.UiMode.NightMask;
                theme = uiMode == global::Android.Content.Res.UiMode.NightYes
                    ? AppTheme.Dark
                    : AppTheme.Light;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    var controller = Window.InsetsController;
                    if (controller is not null)
                    {
                        if (theme == AppTheme.Light)
                            controller.SetSystemBarsAppearance(
                                (int)WindowInsetsControllerAppearance.LightStatusBars,
                                (int)WindowInsetsControllerAppearance.LightStatusBars);
                        else
                            controller.SetSystemBarsAppearance(
                                0,
                                (int)WindowInsetsControllerAppearance.LightStatusBars);
                    }
                }
                else
                {
#pragma warning disable CA1422
                    var flags = Window.DecorView.SystemUiFlags;
                    if (theme == AppTheme.Light)
                        Window.DecorView.SystemUiFlags = flags | SystemUiFlags.LightStatusBar;
                    else
                        Window.DecorView.SystemUiFlags = flags & ~SystemUiFlags.LightStatusBar;
#pragma warning restore CA1422
                }
            });
        }
    }
}