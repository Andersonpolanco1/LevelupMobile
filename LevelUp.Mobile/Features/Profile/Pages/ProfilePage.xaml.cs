using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Features.Profile.Pages;


public partial class ProfilePage : ContentPage
{
    private readonly ISyncService _sync;
    private readonly LocalDatabase _db;

    public ProfilePage(ISyncService sync, LocalDatabase db)
    {
        InitializeComponent();
        _sync = sync;
        _db = db;
    }

    private async void OnSyncClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Sincronizando...";
        try
        {
            await _sync.FullSyncAsync();
            await ShowDbStats();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async Task ShowDbStats()
    {
        var db = _db.Connection;
        var exercises = await db.Table<Exercise>().CountAsync();
        var muscleGroups = await db.Table<MuscleGroup>().CountAsync();
        var muscles = await db.Table<Muscle>().CountAsync();
        var plans = await db.Table<WeeklyPlan>().CountAsync();
        var workouts = await db.Table<Workout>().CountAsync();
        var syncQueue = await db.Table<SyncQueueItem>().CountAsync();
        var syncState = await db.Table<SyncState>().ToListAsync();

        var lastCatalog = syncState.FirstOrDefault(s => s.EntityName == "Catalog")?.LastSync;
        var lastUserData = syncState.FirstOrDefault(s => s.EntityName == "UserData")?.LastSync;

        StatusLabel.Text = $"""
            ✅ Sync completado

            📦 Catálogo:
              Ejercicios: {exercises}
              Grupos musculares: {muscleGroups}
              Músculos: {muscles}

            👤 Usuario:
              Planes: {plans}
              Workouts: {workouts}

            🔄 Cola pendiente: {syncQueue}

            🕐 Último sync:
              Catálogo: {lastCatalog:g}
              UserData: {lastUserData:g}
            """;
    }
}