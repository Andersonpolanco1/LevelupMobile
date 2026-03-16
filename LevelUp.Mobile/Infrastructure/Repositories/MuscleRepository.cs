using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Infrastructure.Repositories;

public class MuscleRepository(LocalDatabase db)
{
    private async Task<SQLite.SQLiteAsyncConnection> GetDbAsync()
    {
        await db.EnsureInitializedAsync();
        return db.Connection;
    }

    // ── Muscle Groups ─────────────────────────────────────────────────

    public async Task<List<(MuscleGroup Group, MuscleGroupTranslation? Translation)>>
        GetGroupsWithTranslationAsync(Language language)
    {
        var conn = await GetDbAsync();

        var groups = await conn.Table<MuscleGroup>()
            .Where(g => !g.IsDeleted)
            .ToListAsync();

        var translations = await conn.Table<MuscleGroupTranslation>()
            .Where(t => t.Language == language)
            .ToListAsync();

        var map = translations.ToDictionary(t => t.MuscleGroupId);

        return groups
            .Select(g => (g, map.TryGetValue(g.Id, out var t) ? t : null))
            .ToList();
    }

    // ── Muscles ───────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve los MuscleId que pertenecen a un MuscleGroup.
    /// Útil para filtrar ejercicios por grupo muscular.
    /// </summary>
    public async Task<List<Guid>> GetMuscleIdsByGroupAsync(Guid muscleGroupId)
    {
        var conn = await GetDbAsync();
        var muscles = await conn.Table<Muscle>()
            .Where(m => m.MuscleGroupId == muscleGroupId && !m.IsDeleted)
            .ToListAsync();
        return muscles.Select(m => m.Id).ToList();
    }

    /// <summary>
    /// Devuelve los ExerciseId cuyo músculo principal (Primary) pertenece al grupo.
    /// </summary>
    public async Task<HashSet<Guid>> GetExerciseIdsByMuscleGroupAsync(Guid muscleGroupId)
    {
        var conn = await GetDbAsync();

        var muscleIds = await GetMuscleIdsByGroupAsync(muscleGroupId);
        if (muscleIds.Count == 0) return [];

        // ExerciseMuscle no filtra por IsDeleted porque es tabla de relación
        var links = await conn.Table<ExerciseMuscle>().ToListAsync();

        return links
            .Where(l => muscleIds.Contains(l.MuscleId))
            .Select(l => l.ExerciseId)
            .ToHashSet();
    }
}