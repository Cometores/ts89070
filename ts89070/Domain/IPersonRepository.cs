namespace ts89070.Domain;

/// <summary>
/// Represents a repository interface for managing <see cref="Person"/> entities.
/// Provides methods for querying, creating, and retrieving <see cref="Person"/> based on various criteria.
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Retrieves all <see cref="Person"/> entities asynchronously.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The task result contains a read-only list of <see cref="Person"/> entities.</returns>
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a <see cref="Person"/> entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="Person"/> to retrieve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>The task result contains the <see cref="Person"/> entity if found; otherwise, null.</returns>
    Task<Person?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Retrieves all <see cref="Person"/> entities with the specified color asynchronously.
    /// </summary>
    /// <param name="color">The color to filter the <see cref="Person"/> entities by.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The task result contains a read-only list of <see cref="Person"/> entities matching the specified color.</returns>
    Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct);

    /// <summary>
    /// Creates a new <see cref="Person"/> entity asynchronously.
    /// </summary>
    /// <param name="input">The <see cref="Person"/> entity to be created.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The task result contains the created <see cref="Person"/> entity.</returns>
    Task<Person> CreateAsync(Person input, CancellationToken ct);
}