using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Core.Abstractions
{
    public interface IRepository<T> where T : ILocalEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<int> UpsertAsync(T entity);
        Task<int> UpsertManyAsync(IEnumerable<T> entities);
        Task SoftDeleteAsync(Guid id);
    }
}
