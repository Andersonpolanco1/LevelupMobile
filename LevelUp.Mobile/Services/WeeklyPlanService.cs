using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Infrastructure.Api;

namespace LevelUp.Mobile.Services
{
    public class WeeklyPlanService
    {
        private readonly IApiClient _api;

        public WeeklyPlanService(IApiClient api)
        {
            _api = api;
        }

        public async Task<List<WeeklyPlanListItemDto>> GetPlansAsync()
        {
            return await _api.GetAsync<List<WeeklyPlanListItemDto>>("api/weekly-plans");
        }

        public async Task<WeeklyPlanListItemDto?> CreateWeeklyPlanAsync(string name, string? notes)
        {
            var dto = new WeeklyPlanAddDto
            {
                Name = name,
                Notes = notes
            };

            return await _api.PostAsync<WeeklyPlanAddDto, WeeklyPlanListItemDto>("api/weekly-plans", dto);

        }

        public async Task ActivatePlanAsync(Guid planId)
        {
            await _api.PostAsync<object, object>(
                $"/api/weekly-plans/{planId}/activate",
                new { });
        }
    }
}
