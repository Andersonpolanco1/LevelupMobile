using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Auth.ViewModels;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Home.ViewModels;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Plans.ViewModels;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Splash.ViewModels;
using LevelUp.Mobile.Features.Workouts.Pages;

namespace LevelUp.Mobile.Extensions
{
    public static class PagesExtensions
    {
        public static IServiceCollection AddPagesAndViewModels(this IServiceCollection services)
        {
            services.AddTransient<SplashPage>();
            services.AddTransient<SplashViewModel>();

            services.AddTransient<LoginPage>();
            services.AddTransient<LoginViewModel>();

            services.AddTransient<HomePage>();
            services.AddTransient<HomeViewModel>();

            services.AddTransient<ExercisesPage>();
            services.AddTransient<ExerciseDetailPage>();

            services.AddTransient<WorkoutPage>();
            services.AddTransient<ActiveWorkoutPage>();
            services.AddTransient<WorkoutSummaryPage>();

            services.AddTransient<PlansPage>();
            services.AddTransient<PlansViewModel>();
            services.AddTransient<PlanDetailPage>();
            services.AddTransient<CreatePlanPage>();
            services.AddTransient<CreatePlanViewModel>();

            services.AddTransient<ProfilePage>();

            return services;
        }
    }
}