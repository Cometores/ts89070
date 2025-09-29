using Microsoft.EntityFrameworkCore;
using ts89070.Domain;

namespace ts89070.Data;

public sealed class EfPersonRepository : IPersonRepository
{
    private readonly PersonDbContext _db;
    public EfPersonRepository(PersonDbContext db) => _db = db;

    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct) =>
        await _db.Persons.AsNoTracking().ToListAsync(ct);

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)!;

    public async Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct) =>
        await _db.Persons.AsNoTracking()
            .Where(p => p.Color.ToLower() == color.ToLower())
            .ToListAsync(ct);

    public async Task<Person> CreateAsync(Person input, CancellationToken ct)
    {
        _db.Persons.Add(input);
        await _db.SaveChangesAsync(ct);
        return input;
    }
}