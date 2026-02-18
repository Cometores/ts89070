using Microsoft.EntityFrameworkCore;
namespace CSV;

/// <summary>
/// Provides an implementation of the <see cref="IPersonRepository"/> interface using Entity Framework.
/// </summary>
public sealed class EfPersonRepository(PersonDbContext db) : IPersonRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct) =>
        await db.Persons.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc/>
    public Task<Person?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken ct)
    {
        return await db.Persons.AsNoTracking()
            .Where(p => p.Color.ToLower() == color.ToLower())
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Person> CreateAsync(Person input, CancellationToken ct)
    {
        db.Persons.Add(input);
        await db.SaveChangesAsync(ct);
        return input;
    }
}