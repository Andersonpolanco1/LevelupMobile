using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Splash.Pages;

namespace LevelUp.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Route = "Splash",
            Content = services.GetRequiredService<SplashPage>()
        });

        Items.Add(new ShellContent
        {
            Route = "Login",
            Content = services.GetRequiredService<LoginPage>()
        });

        Items.Add(new ShellContent
        {
            Route = "Home",
            Content = services.GetRequiredService<HomePage>()
        });

        // Ya NO uses Routing.RegisterRoute para estas páginas
    }
}