using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CatalogService.Configurations;
using MongoDB.Bson;

namespace CatalogService.Repositories
{
    public class MongoRepository<T> : IRepository<T> // Dependency Injection med vores Interface Repository så vi kan lave Moq
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoClient client, IOptions<MongoDbSettings> options) // Vores Singleton IMongoClient tager fat i vores database
        {
            var database = client.GetDatabase(options.Value.DatabaseName); // Database properties
            _collection = database.GetCollection<T>(options.Value.ItemsCollection);

            Console.WriteLine($"Repo is ready. Using collection: {options.Value.ItemsCollection}");
        }

        public async Task<List<T>> GetAllAsync() // Liste med alle vores items.
        {
            try
            {
                Console.WriteLine("Getting all items from database...");
                var result = await _collection.Find(_ => true).ToListAsync();
                Console.WriteLine($"Got {result.Count} items.");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when getting all items: {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetByIdAsync(ObjectId id) // Slå en genstand op for hvert id
        {
            try
            {
                Console.WriteLine($"Looking for item with ID: {id}");
                var result = await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync(); // Filtrer _id property til id fra modelklassen
                if (result == null)
                {
                    Console.WriteLine($"No item found with ID: {id}");
                }
                else
                {
                    Console.WriteLine($"Found item with ID: {id}");
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when getting item by ID: {id}. Error: {ex.Message}");
                throw;
            }
        }

        public async Task CreateAsync(T entity) // Opret en genstand
        {
            try
            {
                Console.WriteLine("Inserting new item into the collection...");
                await _collection.InsertOneAsync(entity);
                Console.WriteLine("New item inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when inserting new item: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(ObjectId id, T entity)
        {
            try
            {
                Console.WriteLine($"Updating item with ID: {id}");
                var result = await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);
                if (result.ModifiedCount > 0)
                {
                    Console.WriteLine($"Updated item with ID: {id}");
                }
                else
                {
                    Console.WriteLine($"No item was updated for ID: {id}. Maybe it wasn't found?");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when updating item with ID: {id}. Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(ObjectId id)
        {
            try
            {
                Console.WriteLine($"Deleting item with ID: {id}");
                var result = await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
                if (result.DeletedCount > 0)
                {
                    Console.WriteLine($"Deleted item with ID: {id}");
                }
                else
                {
                    Console.WriteLine($"No item was found to delete with ID: {id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when deleting item with ID: {id}. Error: {ex.Message}");
                throw;
            }
        }
    }
}
