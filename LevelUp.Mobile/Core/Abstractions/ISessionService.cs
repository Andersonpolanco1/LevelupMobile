namespace LevelUp.Mobile.Core.Abstractions
{
    public interface ISessionService
    {
        // Ya existente
        Task<bool> HasValidSessionAsync();

        // Nuevo — datos del usuario en memoria
        bool IsAuthenticated { get; }
        Guid UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        string? PhotoUrl { get; }

        // Nuevo — poblar desde token guardado
        Task<bool> TryRestoreAsync();

        // Nuevo — poblar después del login
        void SetFromClaims(IDictionary<string, string> claims);

        // Nuevo — limpiar al logout
        void Clear();

        Task ClearUserDataAsync();  // ← nuevo

    }
}
