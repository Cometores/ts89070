using ts89070.Data.CSV;
using ts89070.Domain;

namespace ts89070.Data;

/// <summary>
/// A repository implementation for managing <see cref="Person"/> objects, backed by a CSV data store.
/// Provides methods to perform CRUD operations and query person data by specific criteria.
/// </summary>
public sealed class CsvPersonRepository : IPersonRepository
{
    private readonly PersonCsvStore _store;
    private readonly object _gate = new();
    private List<Person> _cache = new();

    public CsvPersonRepository(PersonCsvStore store)
    {
        _store = store;
        Reload();
    }

    /// <summary>
    /// Reloads the internal cache of <see cref="Person"/> objects from the CSV data source.
    /// This method acquires a lock to ensure thread-safety during the reload operation.
    /// </summary>
    private void Reload()
    {
        lock (_gate)
            _cache = CsvLoader.Load(_store.CsvPath).ToList();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct)
    {
        lock (_gate) 
            return Task.FromResult((IReadOnlyList<Person>)_cache.ToList());
    }

    /// <inheritdoc/>
    public Task<Person?> GetByIdAsync(int id, CancellationToken ct)
    {
        lock (_gate) 
            return Task.FromResult(_cache.FirstOrDefault(p => p.Id == id));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct)
    {
        lock (_gate)
        {
            var res = _cache.Where(p => string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult((IReadOnlyList<Person>)res);
        }
    }

    /// <inheritdoc/>
    public Task<Person> CreateAsync(Person input, CancellationToken ct)
    {
        lock (_gate)
        {
            input.Id = _cache.Count == 0 ? 1 : _cache.Max(p => p.Id) + 1;

            _store.Append(input);
            _cache.Add(input);

            return Task.FromResult(input);
        }
    }
}