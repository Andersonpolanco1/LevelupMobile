using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Workouts.Pages;

namespace LevelUp.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        // ── Pantallas pre-autenticación (sin TabBar) ──────────────────
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

        // ── Área autenticada con TabBar ───────────────────────────────
        var tabBar = new TabBar();

        tabBar.Items.Add(new Tab
        {
            Title = "Home",
            Icon = "tab_home.png",
            Items =
            {
                new ShellContent
                {
                    Route           = "Home",
                    ContentTemplate = new DataTemplate(services.GetRequiredService<HomePage>)
                }
            }
        });

        tabBar.Items.Add(new Tab
        {
            Title = "Exercises",
            Icon = "tab_exercises.png",
            Items =
            {
                new ShellContent
                {
                    Route           = "Exercises",
                    ContentTemplate = new DataTemplate(services.GetRequiredService<ExercisesPage>)
                }
            }
        });

        tabBar.Items.Add(new Tab
        {
            Title = "Workout",
            Icon = "tab_workout.png",
            Items =
            {
                new ShellContent
                {
                    Route           = "Workout",
                    ContentTemplate = new DataTemplate(services.GetRequiredService<WorkoutPage>)
                }
            }
        });

        tabBar.Items.Add(new Tab
        {
            Title = "Plans",
            Icon = "tab_plans.png",
            Items =
            {
                new ShellContent
                {
                    Route           = "Plans",
                    ContentTemplate = new DataTemplate(services.GetRequiredService<PlansPage>)
                }
            }
        });

        tabBar.Items.Add(new Tab
        {
            Title = "Profile",
            Icon = "tab_profile.png",
            Items =
            {
                new ShellContent
                {
                    Route           = "Profile",
                    ContentTemplate = new DataTemplate(services.GetRequiredService<ProfilePage>)
                }
            }
        });

        Items.Add(tabBar);

        // ── Sub-rutas (pantallas que se apilan sobre los tabs) ─────────
        Routing.RegisterRoute("Exercises/Detail", typeof(ExerciseDetailPage));
        Routing.RegisterRoute("Workout/Active", typeof(ActiveWorkoutPage));
        Routing.RegisterRoute("Workout/Summary", typeof(WorkoutSummaryPage));
        Routing.RegisterRoute("Plans/Detail", typeof(PlanDetailPage));
        Routing.RegisterRoute("Plans/Create", typeof(CreatePlanPage));
    }
}
