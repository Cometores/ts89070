using ts89070.Domain;
using ts89070.Services;

namespace ts89070.Data;

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

    private void Reload()
    {
        lock (_gate)
        {
            _cache = CsvLoader.Load(_store.CsvPath).ToList();
        }
    }

    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct)
    {
        lock (_gate) return Task.FromResult((IReadOnlyList<Person>)_cache.ToList());
    }

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct)
    {
        lock (_gate) return Task.FromResult(_cache.FirstOrDefault(p => p.Id == id));
    }

    public Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct)
    {
        lock (_gate)
        {
            var res = _cache.Where(p => string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult((IReadOnlyList<Person>)res);
        }
    }

    public Task<Person> CreateAsync(Person input, CancellationToken ct)
    {
        lock (_gate)
        {
            input.Id = _cache.Count == 0 ? 1 : _cache.Max(p => p.Id) + 1;

            // Сначала пишем в файл (источник истины для CSV-варианта)
            _store.Append(input);

            // Потом обновляем кеш
            _cache.Add(input);

            return Task.FromResult(input);
        }
    }
}