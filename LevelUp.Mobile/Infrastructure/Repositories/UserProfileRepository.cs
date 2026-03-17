using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para leer y guardar el perfil del usuario en SQLite.
    /// Siempre existe un único registro por sesión (el del usuario logueado).
    /// </summary>
    public class UserProfileRepository(LocalDatabase db)
    {
        // ── Obtener ───────────────────────────────────────────────────

        public async Task<UserProfile?> GetAsync()
        {
            await db.EnsureInitializedAsync();
            return await db.Connection.Table<UserProfile>()
                .FirstOrDefaultAsync();
        }

        public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
        {
            await db.EnsureInitializedAsync();
            return await db.Connection.Table<UserProfile>()
                .Where(p => p.Id == userId)
                .FirstOrDefaultAsync();
        }

        // ── Guardar ───────────────────────────────────────────────────

        public async Task UpsertAsync(UserProfile profile)
        {
            await db.EnsureInitializedAsync();

            var existing = await db.Connection.Table<UserProfile>()
                .Where(p => p.Id == profile.Id)
                .FirstOrDefaultAsync();

            if (existing is null)
                await db.Connection.InsertAsync(profile);
            else
                await db.Connection.UpdateAsync(profile);
        }

        // ── Eliminar (logout) ─────────────────────────────────────────

        public async Task DeleteAllAsync()
        {
            await db.EnsureInitializedAsync();
            await db.Connection.ExecuteAsync("DELETE FROM UserProfile");
        }
    }
}