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

        var links = await conn.Table<ExerciseMuscle>().ToListAsync();

        return links
            .Where(l => muscleIds.Contains(l.MuscleId) && l.Role == MuscleRole.Primary) 
            .Select(l => l.ExerciseId)
            .ToHashSet();
    }

    // ── Agregar este método a MuscleRepository ───────────────────────────────────
    // El join se hace en memoria porque SQLite-net-pcl no soporta joins nativos.

    public async Task<Dictionary<Guid, string>> GetPrimaryMuscleGroupByExerciseIdsAsync(
        List<Guid> exerciseIds, Language language)
    {
        if (exerciseIds.Count == 0) return [];

        var conn = await GetDbAsync();

        // 1. ExerciseMuscle donde Role == Primary y ExerciseId esté en la lista
        var exerciseMuscles = await conn.Table<ExerciseMuscle>()
            .Where(em => !em.IsDeleted && em.Role == MuscleRole.Primary)
            .ToListAsync();

        exerciseMuscles = exerciseMuscles
            .Where(em => exerciseIds.Contains(em.ExerciseId))
            .ToList();

        var muscleIds = exerciseMuscles.Select(em => em.MuscleId).Distinct().ToList();

        // 2. Muscles → MuscleGroupId
        var muscles = await conn.Table<Muscle>()
            .Where(m => !m.IsDeleted)
            .ToListAsync();
        muscles = muscles.Where(m => muscleIds.Contains(m.Id)).ToList();

        var groupIds = muscles.Select(m => m.MuscleGroupId).Distinct().ToList();

        // 3. MuscleGroupTranslation para el idioma
        var translations = await conn.Table<MuscleGroupTranslation>()
            .Where(t => !t.IsDeleted && t.Language == language)
            .ToListAsync();
        translations = translations.Where(t => groupIds.Contains(t.MuscleGroupId)).ToList();

        // 4. Construir diccionario ExerciseId → MuscleGroupName
        var muscleToGroup = muscles.ToDictionary(m => m.Id, m => m.MuscleGroupId);
        var groupToName = translations.ToDictionary(t => t.MuscleGroupId, t => t.Name);

        var result = new Dictionary<Guid, string>();
        foreach (var em in exerciseMuscles)
        {
            if (result.ContainsKey(em.ExerciseId)) continue; // solo el primero
            if (!muscleToGroup.TryGetValue(em.MuscleId, out var groupId)) continue;
            if (!groupToName.TryGetValue(groupId, out var name)) continue;
            result[em.ExerciseId] = name;
        }

        return result;
    }

    /// <summary>
    /// Devuelve el MuscleGroupId del músculo primario de un ejercicio.
    /// Devuelve null si el ejercicio no tiene músculos asociados.
    /// </summary>
    public async Task<Guid?> GetPrimaryMuscleGroupIdAsync(Guid exerciseId)
    {
        var conn = await GetDbAsync();

        // 1. Buscar el ExerciseMuscle primario
        var exerciseMuscles = await conn.Table<ExerciseMuscle>()
            .Where(em => !em.IsDeleted && em.ExerciseId == exerciseId && em.Role == MuscleRole.Primary)
            .ToListAsync();

        var primaryMuscleId = exerciseMuscles.FirstOrDefault()?.MuscleId;
        if (primaryMuscleId is null) return null;

        // 2. Buscar el Muscle para obtener MuscleGroupId
        var muscle = await conn.Table<Muscle>()
            .Where(m => !m.IsDeleted && m.Id == primaryMuscleId)
            .FirstOrDefaultAsync();

        return muscle?.MuscleGroupId;
    }
}