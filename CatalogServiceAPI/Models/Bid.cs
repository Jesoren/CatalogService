namespace CatalogService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Bid
{
    [BsonId]
    [BsonRepresentationAttribute(BsonType.ObjectId)]
    public string ItemId { get; set; } // Unik ID for buddet
    public ObjectId BidderId { get; set; } // Referencer til brugeren, der afgiver buddet
    public int BidAmount { get; set; } // Beløbet for buddet
}