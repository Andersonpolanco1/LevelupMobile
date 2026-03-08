// Infrastructure/Repositories/ExerciseRepository.cs
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Infrastructure.Repositories;

public class ExerciseRepository : BaseRepository<Exercise>
{
    public ExerciseRepository(LocalDatabase db) : base(db) { }

    public async Task<List<(Exercise Exercise, ExerciseTranslation? Translation)>>
        GetWithTranslationAsync(Language language)
    {
        var db = await GetDbAsync();

        var exercises = await GetAllAsync();

        var translations = await db.Table<ExerciseTranslation>()
            .Where(t => t.Language == language)
            .ToListAsync();

        var translationMap = translations.ToDictionary(t => t.ExerciseId);

        return exercises
            .Select(e => (e, translationMap.TryGetValue(e.Id, out var t) ? t : null))
            .ToList();
    }

    public async Task UpsertTranslationsFromSyncAsync(
        IEnumerable<ExerciseTranslation> translations)
    {
        var db = await GetDbAsync();

        await db.RunInTransactionAsync(conn =>
        {
            foreach (var t in translations)
            {
                var existing = conn.Find<ExerciseTranslation>(t.Id);
                if (existing is null) conn.Insert(t);
                else conn.Update(t);
            }
        });
    }

    public async Task UpsertMuscleLinksFromSyncAsync(
        Guid exerciseId, IEnumerable<ExerciseMuscle> links)
    {
        var db = await GetDbAsync();

        await db.RunInTransactionAsync(conn =>
        {
            conn.Execute(
                "DELETE FROM ExerciseMuscle WHERE ExerciseId = ?", exerciseId);
            foreach (var link in links)
                conn.Insert(link);
        });
    }
}