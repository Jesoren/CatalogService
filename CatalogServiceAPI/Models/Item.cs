namespace CatalogService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Item 
{
    [BsonId]
    [BsonRepresentationAttribute(BsonType.ObjectId)]
    public ObjectId id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? StartPrice { get; set; }
    public DateTime DateOfCreation { get; set; }
    public DateTime EndDate { get; set; }
    public double HighestBid { get; set; } = 0;
    public ObjectId HighestBidder { get; set; }
}