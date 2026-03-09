// Core/Abstractions/IUserSession.cs
namespace LevelUp.Mobile.Core.Abstractions;

public interface IUserSession
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? PhotoUrl { get; }

    Task<bool> TryRestoreAsync();   // llamado en OnStart
    void SetFromClaims(IDictionary<string, string> claims);
    void Clear();
}