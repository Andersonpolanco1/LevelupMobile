// Infrastructure/Sync/ConnectivitySyncTrigger.cs
using LevelUp.Mobile.Core.Abstractions;

namespace LevelUp.Mobile.Infrastructure.Sync;

public class ConnectivitySyncTrigger : IDisposable
{
    private readonly ISyncService _sync;
    private CancellationTokenSource _cts = new();

    public ConnectivitySyncTrigger(ISyncService sync)
    {
        _sync = sync;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            // Cancela cualquier sync anterior que pudiera estar colgado
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            _ = Task.Run(() => _sync.FullSyncAsync(_cts.Token));
        }
    }

    public void Dispose()
    {
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        _cts.Dispose();
    }
}