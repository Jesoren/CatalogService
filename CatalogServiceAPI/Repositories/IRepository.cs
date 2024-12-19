using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogService.Repositories
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(ObjectId id);
        Task CreateAsync(T entity);
        Task UpdateAsync(ObjectId id, T entity);
        Task DeleteAsync(ObjectId id);
    }
}
