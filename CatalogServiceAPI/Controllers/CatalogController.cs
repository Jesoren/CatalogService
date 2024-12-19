using Microsoft.AspNetCore.Mvc;
using CatalogService.Models;
using System.Text.Json;
using CatalogService.Repositories;
using MongoDB.Bson; // For at kunne bruge ObjectId

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly MongoRepository<Item> _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(MongoRepository<Item> repository, ILogger<CatalogController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            _logger.LogInformation("GetItems called to retrieve all items.");
            try
            {
                var items = await _repository.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {ItemCount} items.", items.Count());
                return Ok(items);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Failed to retrieve items: Repository returned null.");
                return StatusCode(StatusCodes.Status500InternalServerError, "The data source returned no items.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Failed to retrieve items: The operation timed out.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, "The request timed out while retrieving items.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving items: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(string id)
        {
            _logger.LogInformation("GetItem by ID {id} called to retrieve an item.", id);

            try
            {
                // Validér ID-format
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    _logger.LogWarning("Invalid ID format: {id}.", id);
                    return BadRequest("Invalid ID format.");
                }

                // Forsøg at hente objektet
                var item = await _repository.GetByIdAsync(objectId);

                if (item == null)
                {
                    _logger.LogWarning("Item not found for ID: {id}.", id);
                    return NotFound($"Item with ID {id} was not found.");
                }

                _logger.LogInformation("Successfully retrieved item with ID: {Id}.", id);
                return Ok(item);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout occurred while retrieving item with ID: {id}.", id);
                return StatusCode(StatusCodes.Status504GatewayTimeout, "The request timed out while retrieving the item.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving item with ID: {id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(Item newItem)
        {
            _logger.LogInformation("CreateItem called with item: {NewItem}", JsonSerializer.Serialize(newItem));

            try
            {
                // Valider input
                if (newItem == null)
                {
                    _logger.LogWarning("CreateItem failed: null value item created.");
                    return BadRequest("Item cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(newItem.Name)) // Antag at Item har en Name-egenskab
                {
                    _logger.LogWarning("CreateItem failed: Missing or not valid name for item.");
                    return BadRequest("Item must have a valid name.");
                }

                // Opret item
                await _repository.CreateAsync(newItem);
                _logger.LogInformation("Item created successfully with ID: {ItemId}.", newItem.id);

                return CreatedAtAction(nameof(GetItem), new { id = newItem.id }, newItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating item: {NewItem}.", JsonSerializer.Serialize(newItem));
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

[HttpPut("{id}")]
public async Task<IActionResult> UpdateItem(string id, Item updatedItem)
{
    _logger.LogInformation("UpdateItem method called with ID: {id} and data: {UpdatedItem}", id, JsonSerializer.Serialize(updatedItem));

    try
    {
        // Valider ID-format
        if (!ObjectId.TryParse(id, out ObjectId objectId))
        {
            _logger.LogWarning("UpdateItem failed: Invalid ID format for ID: {id}.", id);
            return BadRequest("Invalid ID format.");
        }

        // Tjek om elementet eksisterer
        var existingItem = await _repository.GetByIdAsync(objectId);
        if (existingItem == null)
        {
            _logger.LogWarning("UpdateItem failed: Item with ID {id} not found.", id);
            return NotFound($"Item with ID {id} was not found.");
        }

        // Valider opdateret data
        if (updatedItem == null || string.IsNullOrWhiteSpace(updatedItem.Name)) // Antag at Item har en Name-egenskab
        {
            _logger.LogWarning("UpdateItem failed: Invalid data provided for ID: {id}.", id);
            return BadRequest("Updated item data is invalid.");
        }

        // Opdater elementet
        await _repository.UpdateAsync(objectId, updatedItem);
        _logger.LogInformation("UpdateItem completed successfully for ID: {id}.", id);

        return NoContent();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An unexpected error occurred while updating item with ID: {id}.", id);
        return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
    }
}


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            _logger.LogInformation("DeleteItem method called with ID: {id}", id);

            try
            {
                // Valider ID-format
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    _logger.LogWarning("DeleteItem failed: Invalid ID format for ID: {id}.", id);
                    return BadRequest("Invalid ID format.");
                }

                // Tjek om elementet eksisterer
                var existingItem = await _repository.GetByIdAsync(objectId);
                if (existingItem == null)
                {
                    _logger.LogWarning("DeleteItem failed: Item with ID {id} not found.", id);
                    return NotFound($"Item with ID {id} was not found.");
                }

                // Slet elementet
                await _repository.DeleteAsync(objectId);
                _logger.LogInformation("DeleteItem completed successfully for ID: {id}.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting item with ID: {id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
