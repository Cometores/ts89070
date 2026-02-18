using CSVService.API.Data.CSV;
using CSVService.API.Data.DB;
using CSVService.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace CSVService.API.Data;

/// <summary>
/// Provides functionalities for seeding the database with initial data
/// from external sources such as CSV files.
/// </summary>
public static class Seeder
{
    /// <summary>
    /// Seeds the database with initial data by loading records from a CSV file
    /// and saving them into the database if the database was deleted.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance representing the application.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task PopulateDatabaseAsync(WebApplication app, CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PersonDbContext>();
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Persons.AnyAsync(ct))
            return;

        var persons = LoadPersonsFromCsv(scope.ServiceProvider);
        db.Persons.AddRange(persons);
        await db.SaveChangesAsync(ct);
    }

    private static IReadOnlyList<Person> LoadPersonsFromCsv(IServiceProvider services)
    {
        var cfg = services.GetRequiredService<IConfiguration>();

        string location = cfg.GetValue<string>("DataSources:PersonCsv:Location") ?? string.Empty;
        location = string.IsNullOrWhiteSpace(location)
            ? AppContext.BaseDirectory
            : location.Trim();

        string filename = cfg.GetValue<string>("DataSources:PersonCsv:Filename") ?? "sample-input.csv";
        string path = Path.GetFullPath(Path.Combine(location, filename));

        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV file for seeding was not found: '{path}'", path);

        var persons = CsvLoader.Load(path);
        return persons;
    }
}