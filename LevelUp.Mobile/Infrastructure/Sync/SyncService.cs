// Infrastructure/Sync/SyncService.cs
using System.Text.Json;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.Api;
using LevelUp.Mobile.Infrastructure.Api.Dtos;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Infrastructure.Sync;

public class SyncService : ISyncService
{
    private readonly IApiClient _api;
    private readonly LocalDatabase _db;
    private readonly ExerciseRepository _exercises;
    private readonly WeeklyPlanRepository _plans;
    private readonly WorkoutRepository _workouts;

    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    public SyncService(
        IApiClient api,
        LocalDatabase db,
        ExerciseRepository exercises,
        WeeklyPlanRepository plans,
        WorkoutRepository workouts)
    {
        _api = api;
        _db = db;
        _exercises = exercises;
        _plans = plans;
        _workouts = workouts;
    }

    public async Task FullSyncAsync(CancellationToken ct = default)
    {
        await ProcessSyncQueueAsync(ct);   // 1. subir cambios locales
        await SyncCatalogAsync(ct);        // 2. bajar catálogos
        await SyncUserDataAsync(ct);       // 3. bajar datos del usuario
    }

    // ── PULL: Catálogos ────────────────────────────────────────────────────────

    public async Task SyncCatalogAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();
        var lastSync = await GetLastSyncAsync("Catalog");
        var url = lastSync.HasValue
            ? $"api/sync/catalog?since={lastSync.Value:O}"
            : "api/sync/catalog";

        CatalogSyncResponseDto dto;
        try { dto = await _api.GetAsync<CatalogSyncResponseDto>(url); }
        catch { return; } // sin red → seguimos con local

        await _db.Connection.RunInTransactionAsync(conn =>
        {
            foreach (var mg in dto.MuscleGroups)
            {
                conn.InsertOrReplace(new MuscleGroup
                {
                    Id = mg.Id,
                    CreatedAt = mg.CreatedAt,
                    IsSynced = true
                });
                foreach (var t in mg.Translations)
                    conn.InsertOrReplace(new MuscleGroupTranslation
                    {
                        Id = t.Id,
                        MuscleGroupId = t.MuscleGroupId,
                        Language = t.Language,
                        Name = t.Name,
                        IsSynced = true
                    });

                foreach (var m in mg.Muscles)
                {
                    conn.InsertOrReplace(new Muscle
                    {
                        Id = m.Id,
                        MuscleGroupId = m.MuscleGroupId,
                        CreatedAt = m.CreatedAt,
                        IsSynced = true
                    });
                    foreach (var mt in m.Translations)
                        conn.InsertOrReplace(new MuscleTranslation
                        {
                            Id = mt.Id,
                            MuscleId = mt.MuscleId,
                            Language = mt.Language,
                            Name = mt.Name,
                            IsSynced = true
                        });
                }
            }

            foreach (var ex in dto.Exercises)
            {
                conn.InsertOrReplace(new Exercise
                {
                    Id = ex.Id,
                    IncludesBodyWeight = ex.IncludesBodyWeight,
                    BodyWeightFactor = ex.BodyWeightFactor,
                    ImageUrl = ex.ImageUrl,
                    Type = ex.Type,
                    CreatorId = ex.CreatorId,
                    IsDeleted = ex.IsDeleted,
                    CreatedAt = ex.CreatedAt,
                    UpdatedAt = ex.UpdatedAt,
                    IsSynced = true
                });

                foreach (var t in ex.Translations)
                    conn.InsertOrReplace(new ExerciseTranslation
                    {
                        Id = t.Id,
                        ExerciseId = t.ExerciseId,
                        Language = t.Language,
                        Name = t.Name,
                        Instructions = t.Instructions,
                        Tips = t.Tips,
                        CommonMistakes = t.CommonMistakes
                    });

                conn.Execute(
                    "DELETE FROM ExerciseMuscle WHERE ExerciseId = ?", ex.Id);
                foreach (var m in ex.Muscles)
                    conn.Insert(new ExerciseMuscle
                    {
                        ExerciseId = ex.Id,
                        MuscleId = m.MuscleId,
                        Role = m.Role
                    });
            }
        });

        await SetLastSyncAsync("Catalog");
    }

    // ── PULL: Datos del usuario ────────────────────────────────────────────────

    public async Task SyncUserDataAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();

        var lastSync = await GetLastSyncAsync("UserData");
        var url = lastSync.HasValue
            ? $"api/sync/user-data?since={lastSync.Value:O}"
            : "api/sync/user-data";

        UserDataSyncResponseDto dto;
        try { dto = await _api.GetAsync<UserDataSyncResponseDto>(url); }
        catch { return; }

        await _db.Connection.RunInTransactionAsync(conn =>
        {
            foreach (var plan in dto.WeeklyPlans)
            {
                conn.InsertOrReplace(new WeeklyPlan
                {
                    Id = plan.Id,
                    UserId = plan.UserId,
                    Name = plan.Name,
                    Notes = plan.Notes,
                    IsActive = plan.IsActive,
                    IsDeleted = plan.IsDeleted,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt,
                    IsSynced = true
                });
                foreach (var day in plan.Days)
                {
                    conn.InsertOrReplace(new WeeklyPlanDay
                    {
                        Id = day.Id,
                        WeeklyPlanId = day.WeeklyPlanId,
                        DayOfWeek = day.DayOfWeek,
                        Notes = day.Notes,
                        IsDeleted = day.IsDeleted,
                        CreatedAt = day.CreatedAt,
                        UpdatedAt = day.UpdatedAt,
                        IsSynced = true
                    });
                    foreach (var ex in day.Exercises)
                        conn.InsertOrReplace(new WeeklyPlanExercise
                        {
                            Id = ex.Id,
                            WeeklyPlanDayId = ex.WeeklyPlanDayId,
                            ExerciseId = ex.ExerciseId,
                            Order = ex.Order,
                            SetsPlanned = ex.SetsPlanned,
                            RepsPlanned = ex.RepsPlanned,
                            DurationPlanned = ex.DurationPlanned,
                            RestTimePlanned = ex.RestTimePlanned,
                            Notes = ex.Notes,
                            IsDeleted = ex.IsDeleted,
                            CreatedAt = ex.CreatedAt,
                            UpdatedAt = ex.UpdatedAt,
                            IsSynced = true
                        });
                }
            }

            foreach (var wo in dto.Workouts)
            {
                conn.InsertOrReplace(new Workout
                {
                    Id = wo.Id,
                    UserId = wo.UserId,
                    Notes = wo.Notes,
                    WeeklyPlanDayId = wo.WeeklyPlanDayId,
                    BodyWeightInLbSnapshot = wo.BodyWeightInLbSnapshot,
                    WorkoutState = wo.WorkoutState,
                    FinishedAt = wo.FinishedAt,
                    IsDeleted = wo.IsDeleted,
                    CreatedAt = wo.CreatedAt,
                    UpdatedAt = wo.UpdatedAt,
                    IsSynced = true
                });
                foreach (var we in wo.WorkoutExercises)
                {
                    conn.InsertOrReplace(new WorkoutExercise
                    {
                        Id = we.Id,
                        WorkoutId = we.WorkoutId,
                        ExerciseId = we.ExerciseId,
                        WeeklyPlanExerciseId = we.WeeklyPlanExerciseId,
                        Order = we.Order,
                        Notes = we.Notes,
                        IsDeleted = we.IsDeleted,
                        CreatedAt = we.CreatedAt,
                        UpdatedAt = we.UpdatedAt,
                        IsSynced = true
                    });
                    foreach (var s in we.Sets)
                        conn.InsertOrReplace(new ExerciseSet
                        {
                            Id = s.Id,
                            WorkoutExerciseId = s.WorkoutExerciseId,
                            Reps = s.Reps,
                            WeightInLb = s.WeightInLb,
                            DurationSeconds = s.Duration,
                            Distance = s.Distance,
                            Rpe = s.Rpe,
                            Type = s.Type,
                            RestTimeSeconds = s.RestTimeSeconds,
                            IsDeleted = s.IsDeleted,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            IsSynced = true
                        });
                }
            }
        });

        await SetLastSyncAsync("UserData");
    }

    // ── PUSH: Cola de sincronización ──────────────────────────────────────────

    public async Task ProcessSyncQueueAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();

        var queue = await _db.Connection.Table<SyncQueueItem>()
            .Where(q => q.Status == SyncItemStatus.Pending)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        foreach (var item in queue)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                await DispatchQueueItemAsync(item);
                await _db.Connection.DeleteAsync(item);
            }
            catch (ApiException ex)// when ((int)ex.StatusCode is >= 400 and < 500)
            {
                // Error de cliente → no tiene sentido reintentar
                await _db.Connection.DeleteAsync(item);
            }
            catch
            {
                item.RetryCount++;
                item.Status = item.RetryCount >= 5
                    ? SyncItemStatus.Failed
                    : SyncItemStatus.Pending;
                await _db.Connection.UpdateAsync(item);
            }
        }
    }

    private async Task DispatchQueueItemAsync(SyncQueueItem item)
    {
        switch (item.EntityType)
        {
            case nameof(WeeklyPlan):
                await PushEntityAsync<WeeklyPlan>(
                    item, "api/sync/weekly-plans");
                break;

            case nameof(WeeklyPlanDay):
                await PushEntityAsync<WeeklyPlanDay>(
                    item, "api/sync/weekly-plan-days");
                break;

            case nameof(WeeklyPlanExercise):
                await PushEntityAsync<WeeklyPlanExercise>(
                    item, "api/sync/weekly-plan-exercises");
                break;

            case nameof(Workout):
                await PushEntityAsync<Workout>(
                    item, "api/sync/workouts");
                break;

            case nameof(WorkoutExercise):
                await PushEntityAsync<WorkoutExercise>(
                    item, "api/sync/workout-exercises");
                break;

            case nameof(ExerciseSet):
                await PushEntityAsync<ExerciseSet>(
                    item, "api/sync/exercise-sets");
                break;
        }
    }

    private async Task PushEntityAsync<T>(SyncQueueItem item, string baseUrl)
    {
        var entity = JsonSerializer.Deserialize<T>(item.PayloadJson, _json)!;
        switch (item.Operation)
        {
            case SyncOperation.Create:
            case SyncOperation.Update:
            case SyncOperation.Delete:  
                await _api.PostAsync<T, object>(baseUrl, entity);
                break;
        }
    }

    // ── Helpers SyncState ──────────────────────────────────────────────────────

    private async Task<DateTime?> GetLastSyncAsync(string entity)
    {
        var state = await _db.Connection.Table<Core.Entities.SyncState>()
            .Where(s => s.EntityName == entity)
            .FirstOrDefaultAsync();
        return state?.LastSync;
    }

    private async Task SetLastSyncAsync(string entity)
    {
        await _db.Connection.InsertOrReplaceAsync(new Core.Entities.SyncState
        {
            EntityName = entity,
            LastSync = DateTime.UtcNow
        });
    }
}