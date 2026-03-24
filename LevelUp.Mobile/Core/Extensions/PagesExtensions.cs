using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Auth.ViewModels;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Exercises.ViewModels;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Home.ViewModels;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Plans.ViewModels;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Profile.ViewModels;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Splash.ViewModels;
using LevelUp.Mobile.Features.Stats.Pages;
using LevelUp.Mobile.Features.Workouts.Pages;

namespace LevelUp.Mobile.Extensions
{
    public static class PagesExtensions
    {
        public static IServiceCollection AddPagesAndViewModels(this IServiceCollection services)
        {
            // ── Sin estado persistente → Transient ────────────────────────────
            // Se crean nuevos cada vez, correcto para páginas de autenticación
            // y páginas que siempre deben arrancar limpias
            services.AddTransient<SplashPage>();
            services.AddTransient<SplashViewModel>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LoginViewModel>();

            // ── Páginas de creación/edición → Transient ───────────────────────
            // Deben arrancar limpias cada vez que se abren
            services.AddTransient<CreatePlanPage>();
            services.AddTransient<CreatePlanViewModel>();
            services.AddTransient<PlanEditPage>();
            services.AddTransient<PlanEditViewModel>();

            // ── Páginas de detalle → Transient ────────────────────────────────
            // Reciben QueryProperty y cargan según el ID recibido
            services.AddTransient<PlanDetailPage>();
            services.AddTransient<PlanDetailViewModel>();
            services.AddTransient<PlanDayDetailPage>();
            services.AddTransient<PlanDayDetailViewModel>();

            // ── Workout → Transient ───────────────────────────────────────────
            // Datos en tiempo real, siempre frescos
            services.AddTransient<WorkoutPage>();
            services.AddTransient<ActiveWorkoutPage>();
            services.AddTransient<WorkoutSummaryPage>();

            // ── Tabs principales → Singleton ──────────────────────────────────
            // Viven durante toda la sesión, usan AppState para saber si recargar
            services.AddSingleton<HomePage>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<PlansPage>();
            services.AddSingleton<PlansViewModel>();
            services.AddSingleton<ProfilePage>();
            services.AddSingleton<ProfileViewModel>();

            // ── Ejercicios → Singleton ────────────────────────────────────────
            // Catálogo pesado (100+ items), se cachea entre navegaciones
            services.AddSingleton<ExercisesPage>();
            services.AddSingleton<ExercisesViewModel>();
            services.AddSingleton<ExercisePickerPage>();
            services.AddSingleton<ExercisePickerViewModel>();
            services.AddSingleton<ExerciseDetailPage>();
            services.AddSingleton<ExerciseDetailViewModel>();


            services.AddSingleton<StatsPage>();

            return services;
        }
    }
}