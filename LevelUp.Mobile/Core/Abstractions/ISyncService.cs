namespace LevelUp.Mobile.Core.Abstractions
{
    public interface ISyncService
    {
        /// <summary>Descarga catálogos (exercises, muscles). Sin auth requerida en algunos casos.</summary>
        Task SyncCatalogAsync(CancellationToken ct = default);

        /// <summary>Descarga y sube datos del usuario (plans, workouts).</summary>
        Task SyncUserDataAsync(CancellationToken ct = default);

        /// <summary>Procesa la cola de operaciones pendientes (push al backend).</summary>
        Task ProcessSyncQueueAsync(CancellationToken ct = default);

        /// <summary>Sync completo: primero push, luego pull.</summary>
        Task FullSyncAsync(CancellationToken ct = default);
    }
}
