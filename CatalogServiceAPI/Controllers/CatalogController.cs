using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Options; // Til IOptions<>
using CatalogService.Configurations; // Til MongoDbSettings
using CatalogService.Repositories;

namespace CatalogService.Controllers;

    [ApiController]
    [Route("items")]
    public class CatalogController : ControllerBase
    {
        private readonly MongoRepository<Item> _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(MongoRepository<Item> repository, ILogger<CatalogController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

/* Testdata, et objekt af Item i en liste.
    private static List<Item> _items = new List<Item>() {
new () {
Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
Name = "Carlsberg bil",
Description = "Flot og velholdt samleobjekt, årgang 1874",
StartPrice = 12000,
MinimumPrice = 10000,
DateOfCreation = DateTime.UtcNow,
}
};*/

[HttpGet]
public async Task<ActionResult<IEnumerable<Item>>> GetItems()
{
    _logger.LogInformation("GetItems called at {DT}", DateTime.UtcNow);
    var items = await _repository.GetAllAsync();
    _logger.LogInformation("GetItems called successfully at {DT}", DateTime.UtcNow);
    return Ok(items);
}

[HttpGet("{id:length(24)}")]
public async Task<ActionResult<Item>> GetItem(string id)
{
    _logger.LogInformation("GetItem by ID {id} called at {DT}", DateTime.UtcNow);
    var item = await _repository.GetByIdAsync(id);
    if (item == null)

    _logger.LogWarning("GetItem by ID method failed at {DT}", id, DateTime.UtcNow);
        return NotFound();

    _logger.LogInformation("GetItem method completed at {DT}", DateTime.UtcNow);
    return Ok(item);
}

[HttpPost]
public async Task<IActionResult> CreateItem(Item newItem)
{
    _logger.LogInformation("CreateItem called at {DT}", newItem, DateTime.UtcNow);
    await _repository.CreateAsync(newItem);
    _logger.LogInformation("CreateItem method completed successfully at {DT}", DateTime.UtcNow);
    return CreatedAtAction(nameof(GetItem), new { id = newItem.id }, newItem);
}

[HttpPut("{id:length(24)}")]
public async Task<IActionResult> UpdateItem(string id, Item updatedItem)
{
    _logger.LogInformation("UpdateItem method called with ID {ID} at {DT}", id, DateTime.UtcNow);
    var existingItem = await _repository.GetByIdAsync(id);
    if (existingItem == null)
    _logger.LogWarning("UpdateItem could not find an item with ID {ID} at {DT}", id, DateTime.UtcNow);
        return NotFound();

    await _repository.UpdateAsync(id, updatedItem);
    _logger.LogInformation("UpdateItem method completed successfully at {DT}", DateTime.UtcNow);
    return NoContent();
}

[HttpDelete("{id:length(24)}")]
public async Task<IActionResult> DeleteItem(string id)
{
    _logger.LogInformation("DeleteItem method called with ID {ID} at {DT}", id, DateTime.UtcNow);
    var existingItem = await _repository.GetByIdAsync(id);
    if (existingItem == null)
    _logger.LogWarning("DeleteItem could not find an item with ID {ID} at {DT}", id, DateTime.UtcNow);
        return NotFound();

    await _repository.DeleteAsync(id);
    _logger.LogInformation("DeleteItem method completed successfully at {DT}", DateTime.UtcNow);
    return NoContent();
}

   /* [HttpPost("{itemId}/bids", Name = "PlaceBid")]
public async Task<IActionResult> PlaceBid(Guid itemId, [FromBody] Bid newBid)
{
    _logger.LogInformation("PlaceBid method called at {DT}", DateTime.UtcNow.ToLongTimeString());

    // Find item baseret på itemId
    var item = _items.FirstOrDefault(i => i.Id == itemId);

    if (item == null)
    {
        _logger.LogInformation("Item with ID {ItemId} not found at {DT}", itemId, DateTime.UtcNow.ToLongTimeString());
        return NotFound("Item not found");
    }

    if (newBid == null || newBid.Amount <= 0)
    {
        _logger.LogInformation("Invalid bid received at {DT}", DateTime.UtcNow.ToLongTimeString());
        return BadRequest("Invalid bid");
    }

    // Valider bruger via UserService
    using var httpClient = new HttpClient();
    var userServiceUrl = $"http://userservice:8080/users/{newBid.UserId}";
    HttpResponseMessage response;

    try
    {
        response = await httpClient.GetAsync(userServiceUrl);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error contacting UserService");
        return StatusCode(500, "Unable to validate user due to an internal error");
    }

    if (!response.IsSuccessStatusCode)
    {
        _logger.LogInformation("User with ID {UserId} not found at {DT}", newBid.UserId, DateTime.UtcNow.ToLongTimeString());
        return NotFound("User not found");
    }

    // Parse JSON-data direkte
    var userContent = await response.Content.ReadAsStringAsync();
    using var jsonDoc = JsonDocument.Parse(userContent);
    var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("name", out var nameElement))
    {
        _logger.LogInformation("Name property missing in user data for UserID {UserId} at {DT}", newBid.UserId, DateTime.UtcNow.ToLongTimeString());
        return StatusCode(500, "Unable to retrieve user name");
    }

    var userName = nameElement.GetString();

    // Tjek om buddet er mindst så højt som StartPrice
    if (newBid.Amount < item.StartPrice)
    {
        _logger.LogInformation("Bid amount {Amount} is lower than StartPrice {StartPrice} at {DT}", newBid.Amount, item.StartPrice, DateTime.UtcNow.ToLongTimeString());
        return BadRequest($"Bid must be at least as high as the starting price ({item.StartPrice})");
    }

    // Tjek om buddet er højere end det højeste eksisterende bud (hvis der er bud)
    if (item.Bids.Count > 0 && newBid.Amount <= item.Bids.Max(b => b.Amount))
    {
        _logger.LogInformation("Bid amount {Amount} is lower than the current highest bid at {DT}", newBid.Amount, DateTime.UtcNow.ToLongTimeString());
        return BadRequest("Bid must be higher than the current highest bid");
    }

    // Hvis validering er bestået, tilføj buddet
    newBid.Id = Guid.NewGuid();
    newBid.Timestamp = DateTime.UtcNow;
    newBid.Name = userName; // Brug brugerens navn direkte fra JSON
    item.Bids.Add(newBid);

    _logger.LogInformation("Successfully placed bid at {DT}", DateTime.UtcNow.ToLongTimeString());
    return Ok(newBid);
}*/
}