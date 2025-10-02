using Microsoft.EntityFrameworkCore;
using ts89070.Domain;

namespace ts89070.Data;

/// <summary>
/// Provides an implementation of the <see cref="IPersonRepository"/> interface using Entity Framework.
/// </summary>
public sealed class EfPersonRepository(PersonDbContext db) : IPersonRepository
{
    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct) =>
        await db.Persons.AsNoTracking().ToListAsync(ct);

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)!;

    public async Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct) =>
        await db.Persons.AsNoTracking()
            .Where(p => p.Color.ToLower() == color.ToLower())
            .ToListAsync(ct);

    public async Task<Person> CreateAsync(Person input, CancellationToken ct)
    {
        db.Persons.Add(input);
        await db.SaveChangesAsync(ct);
        return input;
    }
}