using Microsoft.EntityFrameworkCore;
using ts89070.Domain;

namespace ts89070.Data;

public sealed class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }
    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Person>(e =>
        {
            e.ToTable("Persons");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Lastname).IsRequired().HasMaxLength(100);
            e.Property(x => x.Zipcode).IsRequired().HasMaxLength(10);
            e.Property(x => x.City).IsRequired().HasMaxLength(100);
            e.Property(x => x.Color).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.Color);
            e.HasIndex(x => x.Zipcode);
        });
    }
}