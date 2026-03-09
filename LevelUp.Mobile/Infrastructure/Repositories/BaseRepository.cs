using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.LocalDb;
using SQLite;

namespace LevelUp.Mobile.Infrastructure.Repositories;

public class BaseRepository<T>(LocalDatabase databaseService) where T : class, ILocalEntity, new()

{
    protected async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        await databaseService.EnsureInitializedAsync();
        return databaseService.Connection;
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<T>()
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        var db = await GetDbAsync();
        return await db.Table<T>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public virtual async Task InsertAsync(T entity)
    {
        var db = await GetDbAsync();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsSynced = false;
        await db.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        var db = await GetDbAsync();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsSynced = false;
        await db.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var db = await GetDbAsync();
        var entity = await GetByIdAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsSynced = false;
        await db.UpdateAsync(entity);
    }

    public virtual async Task<T?> GetByIdRawAsync(Guid id)
    {
        var db = await GetDbAsync();
        return await db.Table<T>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Inserta si no existe, actualiza si ya existe. Usado por el sync.
    /// NO encola en SyncQueue (es datos que vienen del servidor).
    /// </summary>
    public virtual async Task UpsertFromSyncAsync(T entity)
    {
        var db = await GetDbAsync();
        var existing = await db.Table<T>()
            .Where(x => x.Id == entity.Id)
            .FirstOrDefaultAsync();

        if (existing is null)
            await db.InsertAsync(entity);
        else
            await db.UpdateAsync(entity);
    }

    /// <summary>
    /// Upsert en lote dentro de una transacción. Para sync masivo.
    /// </summary>
    public virtual async Task UpsertManyFromSyncAsync(IEnumerable<T> entities)
    {
        var list = entities.ToList();
        if (list.Count == 0) return;

        // Traer los IDs existentes de una vez (evita N queries)
        var ids = list.Select(e => e.Id).ToHashSet();

        var db = await GetDbAsync();
        var existing = await db.Table<T>()
            .ToListAsync(); // SQLite-net no soporta .Where(x => ids.Contains(x.Id))
                            // para lotes grandes, mejor traer todo y filtrar en memoria

        var existingIds = existing.Select(e => e.Id).ToHashSet();

        var toInsert = list.Where(e => !existingIds.Contains(e.Id)).ToList();
        var toUpdate = list.Where(e => existingIds.Contains(e.Id)).ToList();

        await db.RunInTransactionAsync(conn =>
        {
            foreach (var e in toInsert) conn.Insert(e);
            foreach (var e in toUpdate) conn.Update(e);
        });
    }
}