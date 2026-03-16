using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Infrastructure.Sync;

namespace LevelUp.Mobile.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<LocalDatabase>();

            services.AddSingleton<ExerciseRepository>();
            services.AddSingleton<WeeklyPlanRepository>();
            services.AddSingleton<WorkoutRepository>();
            services.AddSingleton<MuscleRepository>();

            services.AddSingleton<ISyncQueue, SyncQueue>();
            services.AddSingleton<ISyncService, SyncService>();

            return services;
        }
    }
}