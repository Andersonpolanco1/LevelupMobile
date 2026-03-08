using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Abstractions
{
    public interface IExerciseRepository : IRepository<Exercise>
    {
        Task<List<Exercise>> GetWithTranslationsAsync(Language lang);
        Task UpsertTranslationsAsync(IEnumerable<ExerciseTranslation> translations);
        Task UpsertMusclesAsync(IEnumerable<ExerciseMuscle> links);
    }
}
