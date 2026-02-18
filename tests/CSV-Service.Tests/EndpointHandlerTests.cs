using CSV;
using Moq;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CSV;

[TestFixture]
public class PersonHandlerTests
{
    private Mock<IPersonRepository> _mockRepo;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IPersonRepository>();
    }

    [Test]
    public async Task GetPersonsAsync_ReturnsAllPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Id = 1, Name = "Test1", Color = "Red" },
            new() { Id = 2, Name = "Test2", Color = "Blue" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(persons);

        // Act
        var result = await PersonsHandler.GetPersonsAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = result as Ok<IReadOnlyList<Person>>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(persons));
    }

    [Test]
    public async Task GetPersonByIdAsync_WithValidId_ReturnsPerson()
    {
        // Arrange
        var person = new Person { Id = 1, Name = "Test", Color = "Red" };
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(person);

        // Act
        var result = await PersonsHandler.GetPersonByIdAsync("1", _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = result as Ok<Person>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(person));
    }

    [Test]
    public async Task GetPersonByIdAsync_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await PersonsHandler.GetPersonByIdAsync("invalid", _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.That(result.GetType().Name, Does.StartWith("BadRequest"));
    }

    [Test]
    public async Task GetPersonByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

        // Act
        var result = await PersonsHandler.GetPersonByIdAsync("1", _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<NotFound>());
    }

    [Test]
    public async Task GetPersonByColorAsync_WithValidColor_ReturnsPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Id = 1, Name = "Test1", Color = "Red" },
            new() { Id = 2, Name = "Test2", Color = "Red" }
        };
        _mockRepo.Setup(r => r.GetByColorAsync("Red", It.IsAny<CancellationToken>())).ReturnsAsync(persons);

        // Act
        var result = await PersonsHandler.GetPersonsByColorAsync("Red", _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = result as Ok<IReadOnlyList<Person>>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(persons));
    }

    [Test]
    public async Task GetPersonByColorAsync_WithNoMatches_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByColorAsync("Red", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Person>());

        // Act
        var result = await PersonsHandler.GetPersonsByColorAsync("Red", _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<NotFound>());
    }

    [Test]
    public async Task CreatePersonAsync_WithValidPerson_ReturnsPerson()
    {
        // Arrange
        var person = new Person { Name = "Test", Lastname = "Last", Zipcode = "12345", City = "City", Color = "Red" };
        var createdPerson = new Person
            { Id = 1, Name = "Test", Lastname = "Last", Zipcode = "12345", City = "City", Color = "Red" };
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await PersonsHandler.CreatePersonAsync(person, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = result as Created<Person>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(createdPerson));
    }

    [Test]
    public async Task CreatePersonAsync_WithInvalidPerson_ReturnsBadRequest()
    {
        // Arrange
        var person = new Person { Name = "Test" }; // Not all required fields have been filled in

        // Act
        var result = await PersonsHandler.CreatePersonAsync(person, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.That(result.GetType().Name, Does.StartWith("BadRequest"));
    }
}