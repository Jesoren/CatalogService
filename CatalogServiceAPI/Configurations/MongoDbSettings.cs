namespace CatalogService.Configurations;

public class MongoDbSettings
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string ItemsCollection { get; set; } // Tilføj denne linje
}
