namespace ts89070.Domain;

public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct);
    Task<Person?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct);
    Task<Person> CreateAsync(Person input, CancellationToken ct); // для POST /persons
}