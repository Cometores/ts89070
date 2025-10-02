namespace ts89070.Domain;

/// <summary>
/// Represents a repository interface for managing <see cref="Person"/> entities.
/// Provides methods for querying, creating, and retrieving <see cref="Person"/> based on various criteria.
/// </summary>
public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct);
    Task<Person?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct);
    Task<Person> CreateAsync(Person input, CancellationToken ct);
}