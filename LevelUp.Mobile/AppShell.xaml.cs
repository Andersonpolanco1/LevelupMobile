using LevelUp.Mobile.Core.Constants;
using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Workouts.Pages;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile;

public partial class AppShell : Shell
{
    private Tab _homeTab = null!;
    private Tab _exercisesTab = null!;
    private Tab _workoutTab = null!;
    private Tab _plansTab = null!;
    private Tab _profileTab = null!;

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

        _homeTab = new Tab
        {
            Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.House, Size = 20 },
            Items = { new ShellContent { Route = "Home", ContentTemplate = new DataTemplate(services.GetRequiredService<HomePage>) } }
        };

        _exercisesTab = new Tab
        {
            Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.Dumbbell, Size = 20 },
            Items = { new ShellContent { Route = "Exercises", ContentTemplate = new DataTemplate(services.GetRequiredService<ExercisesPage>) } }
        };

        _workoutTab = new Tab
        {
            Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.Play, Size = 20 },
            Items = { new ShellContent { Route = "Workout", ContentTemplate = new DataTemplate(services.GetRequiredService<WorkoutPage>) } }
        };

        _plansTab = new Tab
        {
            Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.CalendarDays, Size = 20 },
            Items = { new ShellContent { Route = "Plans", ContentTemplate = new DataTemplate(services.GetRequiredService<PlansPage>) } }
        };

        _profileTab = new Tab
        {
            Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.User, Size = 20 },
            Items = { new ShellContent { Route = "Profile", ContentTemplate = new DataTemplate(services.GetRequiredService<ProfilePage>) } }
        };

        tabBar.Items.Add(_homeTab);
        tabBar.Items.Add(_exercisesTab);
        tabBar.Items.Add(_workoutTab);
        tabBar.Items.Add(_plansTab);
        tabBar.Items.Add(_profileTab);

        Items.Add(tabBar);

        // ── Sub-rutas ─────────────────────────────────────────────────
        Routing.RegisterRoute("Exercises/Detail", typeof(ExerciseDetailPage));
        Routing.RegisterRoute("Workout/Active", typeof(ActiveWorkoutPage));
        Routing.RegisterRoute("Workout/Summary", typeof(WorkoutSummaryPage));
        Routing.RegisterRoute("Plans/Detail", typeof(PlanDetailPage));
        Routing.RegisterRoute("Plans/Create", typeof(CreatePlanPage));

        UpdateTabTitles();
        LocalizationService.Instance.PropertyChanged += (_, _) => UpdateTabTitles();
    }

    private void UpdateTabTitles()
    {
        _homeTab.Title = LocalizationService.Instance["TabHome"];
        _exercisesTab.Title = LocalizationService.Instance["TabExercises"];
        _workoutTab.Title = LocalizationService.Instance["TabWorkout"];
        _plansTab.Title = LocalizationService.Instance["TabPlans"];
        _profileTab.Title = LocalizationService.Instance["TabProfile"];
    }
}