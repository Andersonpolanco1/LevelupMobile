using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.Api;

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
