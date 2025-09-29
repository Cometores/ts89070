using ts89070.Domain;
using ts89070.Services;

namespace ts89070.Data;

public sealed class CsvPersonRepository : IPersonRepository
{
    private readonly string _csvPath;
    private readonly object _gate = new();
    private List<Person> _cache = new();

    public CsvPersonRepository(IConfiguration cfg, IWebHostEnvironment env)
    {
        // appsettings: DataSources:PersonCsv:Location + Filename
        // Если Location не задан — используем текущую директорию запуска (требование из README). :contentReference[oaicite:2]{index=2}
        var location = cfg["DataSources:PersonCsv:Location"];
        if (string.IsNullOrWhiteSpace(location))
            location = AppContext.BaseDirectory;

        var filename = cfg["DataSources:PersonCsv:Filename"] ?? "sample-input.csv";
        _csvPath = Path.Combine(location, filename);

        Reload();
    }

    private void Reload()
    {
        lock (_gate)
        {
            _cache = CsvLoader.Load(_csvPath).ToList();
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
        // Для CSV — просто добавляем в память. (Бонус: можно дописать в файл, но задание это не требует.)
        lock (_gate)
        {
            input.Id = _cache.Count == 0 ? 1 : _cache.Max(p => p.Id) + 1;
            _cache.Add(input);
            return Task.FromResult(input);
        }
    }
}
