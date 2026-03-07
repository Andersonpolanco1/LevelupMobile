using LevelUp.Mobile.Core.Entities;
using SQLite;

namespace LevelUp.Mobile.Infrastructure.Database
{

    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public async Task InitializeAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(
                DatabaseConstants.DatabasePath,
                DatabaseConstants.Flags);

            await CreateTables();
            await CreateIndexes();
        }

        public SQLiteAsyncConnection Connection =>
            _database ?? throw new Exception("Database not initialized");

        private async Task CreateTables()
        {
            await _database!.CreateTableAsync<Exercise>();
            await _database.CreateTableAsync<ExerciseTranslation>();
            await _database.CreateTableAsync<ExerciseMuscle>();

            await _database.CreateTableAsync<Muscle>();
            await _database.CreateTableAsync<MuscleTranslation>();

            await _database.CreateTableAsync<MuscleGroup>();
            await _database.CreateTableAsync<MuscleGroupTranslation>();

            await _database.CreateTableAsync<WeeklyPlan>();
            await _database.CreateTableAsync<WeeklyPlanDay>();
            await _database.CreateTableAsync<WeeklyPlanExercise>();

            await _database.CreateTableAsync<Workout>();
            await _database.CreateTableAsync<WorkoutExercise>();
            await _database.CreateTableAsync<ExerciseSet>();

            await _database.CreateTableAsync<SyncQueueItem>();
            await _database.CreateTableAsync<SyncState>();
        }

        private async Task CreateIndexes()
        {
            // ==========================
            // TRANSLATIONS
            // ==========================

            await _database!.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_exercise_translation_lang ON ExerciseTranslation(ExerciseId, Language)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_muscle_translation_lang ON MuscleTranslation(MuscleId, Language)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_musclegroup_translation_lang ON MuscleGroupTranslation(MuscleGroupId, Language)");


            // ==========================
            // EXERCISES
            // ==========================

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_exercisemuscle_exercise ON ExerciseMuscle(ExerciseId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_exercisemuscle_muscle ON ExerciseMuscle(MuscleId)");


            // ==========================
            // WORKOUTS
            // ==========================

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_workout_user ON Workout(UserId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_workout_date ON Workout(Date)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_workoutexercise_workout ON WorkoutExercise(WorkoutId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_workoutexercise_exercise ON WorkoutExercise(ExerciseId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_exerciseset_workoutexercise ON ExerciseSet(WorkoutExerciseId)");


            // ==========================
            // WEEKLY PLAN
            // ==========================

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_weeklyplan_user ON WeeklyPlan(UserId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_weeklyplanday_plan ON WeeklyPlanDay(WeeklyPlanId)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_weeklyplanexercise_day ON WeeklyPlanExercise(WeeklyPlanDayId)");


            // ==========================
            // SYNC
            // ==========================

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_syncqueue_status ON SyncQueueItem(Status)");

            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_syncqueue_entity ON SyncQueueItem(EntityType)");
        }
    }
}
