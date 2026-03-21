using LevelUp.Mobile.Core.Constants;
using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Stats.Pages;
using LevelUp.Mobile.Features.Workouts.Pages;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile;

public partial class AppShell : Shell
{
    private Tab _homeTab = null!;
    private Tab _exercisesTab = null!;
    private Tab _workoutTab = null!;
    private Tab _plansTab = null!;
    private Tab _statsTab = null!;

    // Último tab activo para detectar el cambio
    private Tab? _previousTab;

    public AppShell(IServiceProvider services)
    {
        try
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

            _statsTab = new Tab
            {
                Icon = new FontImageSource { FontFamily = "FASolid", Glyph = FA.ChartLine, Size = 20 },
                Items = { new ShellContent { Route = "Stats", ContentTemplate = new DataTemplate(services.GetRequiredService<StatsPage>) } }
            };

            tabBar.Items.Add(_homeTab);
            tabBar.Items.Add(_exercisesTab);
            tabBar.Items.Add(_workoutTab);
            tabBar.Items.Add(_plansTab);
            tabBar.Items.Add(_statsTab);

            Items.Add(tabBar);

            // ── Sub-rutas ─────────────────────────────────────────────────
            Routing.RegisterRoute("Exercises/Detail", typeof(ExerciseDetailPage));
            Routing.RegisterRoute("Workout/Active", typeof(ActiveWorkoutPage));
            Routing.RegisterRoute("Workout/Summary", typeof(WorkoutSummaryPage));
            Routing.RegisterRoute("Plans/Detail", typeof(PlanDetailPage));
            Routing.RegisterRoute("Plans/Create", typeof(CreatePlanPage));
            Routing.RegisterRoute("PlanEdit", typeof(PlanEditPage));
            Routing.RegisterRoute("PlanDayDetail", typeof(PlanDayDetailPage));
            Routing.RegisterRoute("exercisePicker", typeof(ExercisePickerPage));

            // Perfil accesible como ruta suelta (sin tab)
            Routing.RegisterRoute("Profile", typeof(ProfilePage));

            // ── Suscribir cambio de tab ───────────────────────────────────
            tabBar.PropertyChanged += OnTabBarPropertyChanged;

            UpdateTabTitles();
            LocalizationService.Instance.PropertyChanged += (_, _) => UpdateTabTitles();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> APPSHELL CRASH: {ex}");
            System.Diagnostics.Debug.WriteLine($">>> INNER: {ex.InnerException?.Message}");
            throw;
        }
    }

    // ── Detectar cambio de tab mediante PropertyChanged del TabBar ────────
    // CurrentItem del TabBar cambia exactamente cuando el usuario toca otro tab.
    // En ese momento limpiamos el stack del tab que se abandona usando
    // NavigationStack directamente, sin comparar strings de rutas.
    private void OnTabBarPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TabBar.CurrentItem)) return;
        if (sender is not TabBar tabBar) return;

        var incoming = tabBar.CurrentItem as Tab;

        if (_previousTab != null && _previousTab != incoming)
        {
            var stack = Navigation.NavigationStack.ToArray();
            for (int i = stack.Length - 1; i > 0; i--)
                Navigation.RemovePage(stack[i]);
        }

        _previousTab = incoming;
    }

    private void UpdateTabTitles()
    {
        _homeTab.Title = LocalizationService.Instance["TabHome"];
        _exercisesTab.Title = LocalizationService.Instance["TabExercises"];
        _workoutTab.Title = LocalizationService.Instance["TabWorkout"];
        _plansTab.Title = LocalizationService.Instance["TabPlans"];
        _statsTab.Title = LocalizationService.Instance["TabStats"];
    }
}