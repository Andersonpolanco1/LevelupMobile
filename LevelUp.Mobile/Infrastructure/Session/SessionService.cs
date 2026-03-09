using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Infrastructure.LocalDb;
using LevelUp.Mobile.Infrastructure.Token;

namespace LevelUp.Mobile.Infrastructure.Session
{
    public class SessionService : ISessionService
    {
        private readonly ITokenService _tokenService;
        private readonly LocalDatabase _localDatabase;

        public SessionService(ITokenService tokenService, LocalDatabase localDatabase)
        {
            _tokenService = tokenService;
            _localDatabase = localDatabase;
        }

        // ── Estado en memoria ──────────────────────────────────────────────
        public bool IsAuthenticated { get; private set; }
        public Guid UserId { get; private set; }
        public string? UserName { get; private set; }
        public string? Email { get; private set; }
        public string? PhotoUrl { get; private set; }

        // ── Ya existente ───────────────────────────────────────────────────
        public async Task<bool> HasValidSessionAsync()
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();
            var expiration = await _tokenService.GetExpirationAsync();

            if (string.IsNullOrWhiteSpace(accessToken) || expiration == null)
                return false;

            if (expiration <= DateTime.UtcNow.AddMinutes(1))
                return false;

            return true;
        }

        // ── Nuevo ──────────────────────────────────────────────────────────
        public async Task<bool> TryRestoreAsync()
        {
            if (!await HasValidSessionAsync())
            {
                IsAuthenticated = false;
                return false;
            }

            var claims = await _tokenService.GetUserClaimsAsync();
            SetFromClaims(claims);
            return IsAuthenticated;
        }

        public void SetFromClaims(IDictionary<string, string> claims)
        {
            if (claims.TryGetValue("sub", out var sub) && Guid.TryParse(sub, out var id))
                UserId = id;

            if (claims.TryGetValue("userName", out var name)) UserName = name;
            if (claims.TryGetValue("email", out var email)) Email = email;
            if (claims.TryGetValue("picture", out var photo)) PhotoUrl = photo;

            IsAuthenticated = UserId != Guid.Empty;
        }

        public void Clear()
        {
            IsAuthenticated = false;
            UserId = Guid.Empty;
            UserName = null;
            Email = null;
            PhotoUrl = null;
        }

        public async Task ClearUserDataAsync()
        {
            // Limpiar datos del usuario de SQLite

            await _localDatabase.EnsureInitializedAsync();
            var conn = _localDatabase.Connection;

            await conn.ExecuteAsync("DELETE FROM WeeklyPlan");
            await conn.ExecuteAsync("DELETE FROM WeeklyPlanDay");
            await conn.ExecuteAsync("DELETE FROM WeeklyPlanExercise");
            await conn.ExecuteAsync("DELETE FROM Workout");
            await conn.ExecuteAsync("DELETE FROM WorkoutExercise");
            await conn.ExecuteAsync("DELETE FROM ExerciseSet");
            await conn.ExecuteAsync("DELETE FROM SyncQueueItem");  // ← crítico
            await conn.ExecuteAsync("DELETE FROM SyncState");      // ← crítico, fuerza full sync al login
        }
    }

}
