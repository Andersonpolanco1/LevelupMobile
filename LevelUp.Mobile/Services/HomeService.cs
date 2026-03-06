using LevelUp.Mobile.Features.Home.Models;
using LevelUp.Mobile.Infrastructure.Api;

namespace LevelUp.Mobile.Services
{
    public class HomeService(IApiClient apiClient)
    {
        public async Task<TodayPlanDto?> GetTodayPlanAsync()
        {
            return await apiClient.GetOptionalAsync<TodayPlanDto>("api/weekly-plans/today");
        }
    }

}
