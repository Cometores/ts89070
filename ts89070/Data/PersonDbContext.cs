using Microsoft.EntityFrameworkCore;
using ts89070.Domain;

namespace ts89070.Data;

public sealed class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }
    public DbSet<Person> Persons => Set<Person>();
}