using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.Database;
using SQLite;

namespace LevelUp.Mobile.Infrastructure.Sync
{
    public class SyncQueueRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public SyncQueueRepository(DatabaseService database)
        {
            _db = database.Connection;
        }

        public Task AddAsync(SyncQueueItem item)
        {
            return _db.InsertAsync(item);
        }

        public Task<List<SyncQueueItem>> GetPendingAsync()
        {
            return _db.Table<SyncQueueItem>()
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public Task DeleteAsync(SyncQueueItem item)
        {
            return _db.DeleteAsync(item);
        }
    }
}
