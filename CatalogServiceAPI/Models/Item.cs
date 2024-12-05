namespace Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class Item 
{
    [BsonId]
    [BsonRepresentationAttribute(BsonType.ObjectId)]
    public ObjectId id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public int? startPrice { get; set; }
    public int? minimumPrice  { get; set; }
    public DateTime dateOfCreation { get; set; }
    public List<Bid> bids { get; set; } = new List<Bid>(); // Liste over bud
}