namespace LevelUp.Mobile.Core.Abstractions
{
    public interface ISessionService
    {
        Task<bool> HasValidSessionAsync();
    }
}
