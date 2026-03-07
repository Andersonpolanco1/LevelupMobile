using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Infrastructure.Sync
{
    public class SyncService
    {
        private readonly SyncQueueRepository _queue;
        private readonly ApiService _api;

        public SyncService(SyncQueueRepository queue, ApiService api)
        {
            _queue = queue;
            _api = api;
        }

        public async Task PushAsync()
        {
            var items = await _queue.GetPendingAsync();

            foreach (var item in items)
            {
                await _api.SendAsync(item);

                await _queue.DeleteAsync(item);
            }
        }
    }
}
