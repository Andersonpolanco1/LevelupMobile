using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Services
{
    public class ApiService(IApiClient apiClient)
    {
        public async Task SendAsync(SyncQueueItem item)
        {
            throw new NotImplementedException();
        }
    }
}
