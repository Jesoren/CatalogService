using System.Text;
using CatalogService.Repositories;
using CatalogService.Models;
using CatalogService.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace CatalogService.Services;

// Background worker fra Microsoft Extension sørger for at klassen initialiseres ved opstart af applikation.
public class RabbitMQReceiver : BackgroundService
{
    private readonly ILogger<RabbitMQReceiver> _logger; 
    private readonly MongoRepository<Item> _repository;

    public RabbitMQReceiver(ILogger<RabbitMQReceiver> logger, IMongoClient client, IOptions<MongoDbSettings> options)
    {
        _logger = logger;

        // Brug injiceret IMongoClient til at oprette en MongoRepository
        var database = client.GetDatabase(options.Value.DatabaseName);
        var collection = database.GetCollection<Item>(nameof(Item));
        _repository = new MongoRepository<Item>(client, options);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Opret ny stream gennem RabbitMQ
        var streamSystem = await StreamSystem.Create(new StreamSystemConfig
        {
            Endpoints = new List<EndPoint> { new DnsEndPoint("rabbitmq", 5552) } // Stream port hvor vores rabbitmq lytter til beskeder
        });

        await streamSystem.CreateStream(new StreamSpec("bidding-stream") // Navngivningen af vores stream
        {
            MaxLengthBytes = 5_000_000_000 // Max kapacitet af beskeder på 5gb
        });

        var consumer = await Consumer.Create(new ConsumerConfig(streamSystem, "bidding-stream") // Vores consumer oprettes
        {
            OffsetSpec = new OffsetTypeFirst(),
            MessageHandler = async (stream, _, _, message) => // Vores besked modtages gennem stream og er klar til at gå igennem betingelser
            {
                string bidMessage = Encoding.UTF8.GetString(message.Data.Contents);
                _logger.LogInformation($"Stream: {stream} - Modtaget besked: {bidMessage}");

                try
                {
                    var bid = JsonSerializer.Deserialize<Bid>(bidMessage);
                    if (bid == null || !ObjectId.TryParse(bid.ItemId, out ObjectId objectId))
                    {
                        _logger.LogWarning("Ugyldigt format.");
                        return;
                    }

                    var item = await _repository.GetByIdAsync(objectId);
                    if (item == null)
                    {
                        _logger.LogWarning($"Genstand med ID {bid.ItemId} blev ikke fundet.");
                        return;
                    }

                    if (bid.BidAmount > item.HighestBid)
                    {
                        item.HighestBid = bid.BidAmount;
                        item.HighestBidder = bid.BidderId;

                        await _repository.UpdateAsync(objectId, item);
                        _logger.LogInformation($"Bud accepteret. Nye højeste bud: {bid.BidAmount} fra {bid.BidderId}");
                    }
                    else
                    {
                        _logger.LogInformation($"Bud afvist. Højeste nuværende bud er: {item.HighestBid}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Kunne ikke modtage bud: {ex.Message}");
                }

                await Task.CompletedTask;
            }
        });

        while (!stoppingToken.IsCancellationRequested) // While loop der fortsætter indtil stream lukkes.
        {
            await Task.Delay(1000, stoppingToken);
        }

        await consumer.Close();
        await streamSystem.Close();
    }
}
