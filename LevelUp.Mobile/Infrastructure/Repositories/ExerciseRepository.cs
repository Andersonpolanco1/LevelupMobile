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

    /// <summary>
    /// Devuelve el nombre traducido del grupo muscular primario por ExerciseId.
    /// </summary>
    public async Task<Dictionary<Guid, string>> GetPrimaryMuscleGroupNameMapAsync(Language language)
    {
        var db = await GetDbAsync();

        // Solo músculos primarios
        var exerciseMuscles = await db.Table<ExerciseMuscle>()
            .Where(em => !em.IsDeleted && em.Role == MuscleRole.Primary)
            .ToListAsync();

        var muscles = await db.Table<Muscle>()
            .Where(m => !m.IsDeleted)
            .ToListAsync();

        var muscleGroupTranslations = await db.Table<MuscleGroupTranslation>()
            .Where(t => t.Language == language && !t.IsDeleted)
            .ToListAsync();

        var muscleMap = muscles.ToDictionary(m => m.Id, m => m.MuscleGroupId);
        var groupNameMap = muscleGroupTranslations.ToDictionary(t => t.MuscleGroupId, t => t.Name);

        return exerciseMuscles
            .GroupBy(em => em.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var muscleId = g.First().MuscleId;
                    if (!muscleMap.TryGetValue(muscleId, out var groupId)) return "—";
                    return groupNameMap.TryGetValue(groupId, out var name) ? name : "—";
                });
    }

    // 1. Mapa ExerciseId → nombres de músculos primarios (para las cards de la lista)
    public async Task<Dictionary<Guid, string>> GetPrimaryMuscleNamesAsync(Language language)
    {
        var db = await GetDbAsync();

        // Obtener todas las relaciones Exercise → Muscle (rol Primary)
        var primaryLinks = await db.Table<ExerciseMuscle>()
            .Where(em => em.Role == MuscleRole.Primary && !em.IsDeleted)
            .ToListAsync();

        var muscleIds = primaryLinks.Select(em => em.MuscleId).Distinct().ToList();

        // Traducciones de esos músculos
        var translations = await db.Table<MuscleTranslation>()
            .Where(t => muscleIds.Contains(t.MuscleId) && t.Language == language && !t.IsDeleted)
            .ToListAsync();

        var nameMap = translations.ToDictionary(t => t.MuscleId, t => t.Name);

        // Agrupar por ejercicio → concatenar nombres
        return primaryLinks
            .GroupBy(em => em.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g => string.Join(", ", g
                    .Select(em => nameMap.TryGetValue(em.MuscleId, out var n) ? n : "")
                    .Where(n => !string.IsNullOrEmpty(n)))
            );
    }

    // 2. Obtener ejercicio por ID (sin traducción)
    public async Task<Exercise?> GetByIdAsync(Guid id)
    {
        var db = await GetDbAsync();
        var exercise = await db.Table<Exercise>()
            .Where(e => e.Id == id && !e.IsDeleted)
            .FirstOrDefaultAsync();

        return exercise;
    }

    // 3. Obtener traducción de un ejercicio
    public async Task<ExerciseTranslation?> GetTranslationAsync(Guid exerciseId, Language language)
    {
        var db = await GetDbAsync();
        return await db.Table<ExerciseTranslation>()
            .Where(t => t.ExerciseId == exerciseId && t.Language == language && !t.IsDeleted)
            .FirstOrDefaultAsync();
    }
}