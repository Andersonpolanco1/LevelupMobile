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

    // ── Full Sync ─────────────────────────────────────────────────────────────

    public async Task FullSyncAsync(CancellationToken ct = default)
    {
        Log("FullSync START");
        await ProcessSyncQueueAsync(ct);
        await SyncCatalogAsync(ct);
        await SyncUserDataAsync(ct);
        Log("FullSync END");
    }

    // ── PULL: Catálogos ───────────────────────────────────────────────────────

    public async Task SyncCatalogAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();

        var lastSync = await GetLastSyncAsync("Catalog");
        var url = lastSync.HasValue
            ? $"api/sync/catalog?since={lastSync.Value:O}"
            : "api/sync/catalog";

        Log($"SyncCatalog START | since={FormatDate(lastSync)} | url={url}");

        CatalogSyncResponseDto dto;
        try
        {
            dto = await _api.GetAsync<CatalogSyncResponseDto>(url);
            Log($"SyncCatalog recibido | muscleGroups={dto.MuscleGroups.Count} exercises={dto.Exercises.Count}");
        }
        catch (Exception ex)
        {
            Log($"SyncCatalog ERROR (sin red, usando local): {ex.Message}");
            return;
        }

        await _db.Connection.RunInTransactionAsync(conn =>
        {
            foreach (var mg in dto.MuscleGroups)
            {
                Upsert(conn, new MuscleGroup
                {
                    Id = mg.Id,
                    CreatedAt = mg.CreatedAt,
                    IsSynced = true
                });

                foreach (var t in mg.Translations)
                    Upsert(conn, new MuscleGroupTranslation
                    {
                        Id = t.Id,
                        MuscleGroupId = t.MuscleGroupId,
                        Language = t.Language,
                        Name = t.Name,
                        IsSynced = true
                    });

                foreach (var m in mg.Muscles)
                {
                    Upsert(conn, new Muscle
                    {
                        Id = m.Id,
                        MuscleGroupId = m.MuscleGroupId,
                        CreatedAt = m.CreatedAt,
                        IsSynced = true
                    });

                    foreach (var mt in m.Translations)
                        Upsert(conn, new MuscleTranslation
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
                Upsert(conn, new Exercise
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
                    Upsert(conn, new ExerciseTranslation
                    {
                        Id = t.Id,
                        ExerciseId = t.ExerciseId,
                        Language = t.Language,
                        Name = t.Name,
                        Instructions = t.Instructions,
                        Tips = t.Tips,
                        CommonMistakes = t.CommonMistakes
                    });

                conn.Execute("DELETE FROM ExerciseMuscle WHERE ExerciseId = ?", ex.Id);
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
        Log("SyncCatalog END ✓");
    }

    // ── PULL: Datos del usuario ───────────────────────────────────────────────

    public async Task SyncUserDataAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();

        var lastSync = await GetLastSyncAsync("UserData");
        var url = lastSync.HasValue
            ? $"api/sync/user-data?since={lastSync.Value:O}"
            : "api/sync/user-data";

        Log($"SyncUserData START | since={FormatDate(lastSync)} | url={url}");

        UserDataSyncResponseDto dto;
        try
        {
            dto = await _api.GetAsync<UserDataSyncResponseDto>(url);
            Log($"SyncUserData recibido | planes={dto.WeeklyPlans.Count} workouts={dto.Workouts.Count}");
        }
        catch (Exception ex)
        {
            Log($"SyncUserData ERROR (sin red, usando local): {ex.Message}");
            return;
        }

        await _db.Connection.RunInTransactionAsync(conn =>
        {
            // ── WeeklyPlans ──────────────────────────────────────────────
            foreach (var plan in dto.WeeklyPlans)
            {
                var isNew = !Exists<WeeklyPlan>(conn, plan.Id);
                Log($"  WeeklyPlan id={plan.Id} name={plan.Name} isActive={plan.IsActive} isDeleted={plan.IsDeleted} → {(isNew ? "INSERT" : "UPDATE")}");

                Upsert(conn, new WeeklyPlan
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
                    var isDayNew = !Exists<WeeklyPlanDay>(conn, day.Id);
                    Log($"    WeeklyPlanDay id={day.Id} dow={day.DayOfWeek} → {(isDayNew ? "INSERT" : "UPDATE")}");

                    Upsert(conn, new WeeklyPlanDay
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
                    {
                        var isExNew = !Exists<WeeklyPlanExercise>(conn, ex.Id);
                        Log($"      WeeklyPlanExercise id={ex.Id} order={ex.Order} → {(isExNew ? "INSERT" : "UPDATE")}");

                        Upsert(conn, new WeeklyPlanExercise
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
            }

            // ── Workouts ─────────────────────────────────────────────────
            foreach (var wo in dto.Workouts)
            {
                var isNew = !Exists<Workout>(conn, wo.Id);
                Log($"  Workout id={wo.Id} state={wo.WorkoutState} → {(isNew ? "INSERT" : "UPDATE")}");

                Upsert(conn, new Workout
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
                    var isWeNew = !Exists<WorkoutExercise>(conn, we.Id);
                    Log($"    WorkoutExercise id={we.Id} order={we.Order} → {(isWeNew ? "INSERT" : "UPDATE")}");

                    Upsert(conn, new WorkoutExercise
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
                    {
                        Upsert(conn, new ExerciseSet
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
            }
        });

        await SetLastSyncAsync("UserData");
        Log("SyncUserData END ✓");
    }

    // ── PUSH: Cola de sincronización ──────────────────────────────────────────

    public async Task ProcessSyncQueueAsync(CancellationToken ct = default)
    {
        await _db.EnsureInitializedAsync();

        var queue = await _db.Connection.Table<SyncQueueItem>()
            .Where(q => q.Status == SyncItemStatus.Pending)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        Log($"ProcessSyncQueue START | pendientes={queue.Count}");

        foreach (var item in queue)
        {
            if (ct.IsCancellationRequested)
            {
                Log("ProcessSyncQueue CANCELADO");
                break;
            }

            Log($"  Procesando entity={item.EntityType} op={item.Operation} id={item.Id}");

            try
            {
                await DispatchQueueItemAsync(item);
                await _db.Connection.DeleteAsync(item);
                Log($"  ✓ OK entity={item.EntityType} op={item.Operation}");
            }
            catch (ApiException ex)
            {
                Log($"  ✗ Error cliente entity={item.EntityType} → descartado: {ex.Message}");
                await _db.Connection.DeleteAsync(item);
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.Status = item.RetryCount >= 5
                    ? SyncItemStatus.Failed
                    : SyncItemStatus.Pending;
                await _db.Connection.UpdateAsync(item);
                Log($"  ✗ Error red entity={item.EntityType} retry={item.RetryCount} status={item.Status}: {ex.Message}");
            }
        }

        Log("ProcessSyncQueue END ✓");
    }

    private async Task DispatchQueueItemAsync(SyncQueueItem item)
    {
        switch (item.EntityType)
        {
            case nameof(WeeklyPlan):
                await PushEntityAsync<WeeklyPlan>(item, "api/sync/weekly-plans");
                break;
            case nameof(WeeklyPlanDay):
                await PushEntityAsync<WeeklyPlanDay>(item, "api/sync/weekly-plan-days");
                break;
            case nameof(WeeklyPlanExercise):
                await PushEntityAsync<WeeklyPlanExercise>(item, "api/sync/weekly-plan-exercises");
                break;
            case nameof(Workout):
                await PushEntityAsync<Workout>(item, "api/sync/workouts");
                break;
            case nameof(WorkoutExercise):
                await PushEntityAsync<WorkoutExercise>(item, "api/sync/workout-exercises");
                break;
            case nameof(ExerciseSet):
                await PushEntityAsync<ExerciseSet>(item, "api/sync/exercise-sets");
                break;
            default:
                Log($"  ⚠ EntityType desconocido: {item.EntityType}");
                break;
        }
    }

    private async Task PushEntityAsync<T>(SyncQueueItem item, string baseUrl)
    {
        var entity = JsonSerializer.Deserialize<T>(item.PayloadJson, _json)!;
        await _api.PostAsync<T, object>(baseUrl, entity);
    }

    // ── Helpers Upsert ────────────────────────────────────────────────────────

    /// <summary>
    /// Upsert seguro para PKs Guid. InsertOrReplace de SQLite-net
    /// hace DELETE+INSERT, lo que duplica filas cuando la PK es Guid (TEXT).
    /// </summary>
    private static void Upsert<T>(SQLite.SQLiteConnection conn, T entity) where T : new()
    {
        var tableName = typeof(T).Name;
        var pkProp = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{tableName} no tiene propiedad Id");
        var pk = pkProp.GetValue(entity)!;

        var exists = conn.ExecuteScalar<int>(
            $"SELECT COUNT(1) FROM \"{tableName}\" WHERE Id = ?", pk) > 0;

        if (exists) conn.Update(entity);
        else conn.Insert(entity);
    }

    private static bool Exists<T>(SQLite.SQLiteConnection conn, Guid id) where T : new()
    {
        var tableName = typeof(T).Name;
        return conn.ExecuteScalar<int>(
            $"SELECT COUNT(1) FROM \"{tableName}\" WHERE Id = ?", id) > 0;
    }

    // ── Helpers SyncState ─────────────────────────────────────────────────────

    private async Task<DateTime?> GetLastSyncAsync(string entity)
    {
        var state = await _db.Connection.Table<Core.Entities.SyncState>()
            .Where(s => s.EntityName == entity)
            .FirstOrDefaultAsync();
        Log($"GetLastSync entity={entity} → {FormatDate(state?.LastSync)}");
        return state?.LastSync;
    }

    private async Task SetLastSyncAsync(string entity)
    {
        var now = DateTime.UtcNow;
        await _db.Connection.InsertOrReplaceAsync(new Core.Entities.SyncState
        {
            EntityName = entity,
            LastSync = now
        });
        Log($"SetLastSync entity={entity} → {now:O}");
    }

    // ── Logging ───────────────────────────────────────────────────────────────

    private static string FormatDate(DateTime? date) =>
        date.HasValue ? date.Value.ToString("O") : "null";

    private static void Log(string msg) =>
        System.Diagnostics.Debug.WriteLine($"[SyncService] {msg}");
}