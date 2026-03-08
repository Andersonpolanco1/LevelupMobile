// Infrastructure/Repositories/WorkoutRepository.cs
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Infrastructure.Repositories;

public class WorkoutRepository(LocalDatabase db) : BaseRepository<Workout>(db)
{
    public async Task<Workout?> GetActiveWorkoutAsync(Guid userId)
    {
        var db = await GetDbAsync();
        return await db.Table<Workout>()
            .Where(w => w.UserId == userId
                     && w.WorkoutState == WorkoutState.InProgress
                     && !w.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkoutExercise>> GetExercisesAsync(Guid workoutId)
    {
        var db = await GetDbAsync();
        return await db.Table<WorkoutExercise>()
            .Where(e => e.WorkoutId == workoutId && !e.IsDeleted)
            .OrderBy(e => e.Order)
            .ToListAsync();
    }

    public async Task<List<ExerciseSet>> GetSetsAsync(Guid workoutExerciseId)
    {
        var db = await GetDbAsync();
        return await db.Table<ExerciseSet>()
            .Where(s => s.WorkoutExerciseId == workoutExerciseId && !s.IsDeleted)
            .ToListAsync();
    }
}