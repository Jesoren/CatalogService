using CatalogService.Repositories;
using MongoDB.Bson;
using Moq;
using CatalogService.Models;

public class RepositoryTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;

    public RepositoryTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenItemExists()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId(); // Generer et ObjectId gemt som testId
        var testItem = new Item { id = testId, Name = "Test Item" }; // Opret en ny instans af Item med et {id} = testId og et {Name}

        _mockRepository.Setup(repo => repo.GetByIdAsync(testId)) // Mock mongol db repository GetByIdAsync metoden 
                       .ReturnsAsync(testItem);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(testId); // Kald den mockede metode og afvent resultat

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testId, result.id);
        Assert.Equal("Test Item", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        // Arrange
        var invalidId = ObjectId.GenerateNewId(); // Generer et ObjectId gemt som invalidId

        _mockRepository.Setup(repo => repo.GetByIdAsync(invalidId)) // Mock mongodb repository GetByIdAsync metode med invalidId parameter
                       .ReturnsAsync((Item)null);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(invalidId); // Kald den mockede metode og vent på resultatet

        // Assert
        Assert.Null(result); // Returnerer nul fordi item ikke findes
    }
}
