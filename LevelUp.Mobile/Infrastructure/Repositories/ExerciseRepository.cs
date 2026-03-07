using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.Database;
using SQLite;

namespace LevelUp.Mobile.Infrastructure.Repositories
{
    public class ExerciseRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public ExerciseRepository(DatabaseService database)
        {
            _db = database.Connection;
        }

        public Task<List<Exercise>> GetAllAsync()
        {
            return _db.Table<Exercise>().ToListAsync();
        }

        public Task InsertAsync(Exercise exercise)
        {
            return _db.InsertAsync(exercise);
        }

        public Task UpdateAsync(Exercise exercise)
        {
            return _db.UpdateAsync(exercise);
        }
    }
}
