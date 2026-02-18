using CSV;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

string activeDataSource = builder.Configuration["DataSources:ActiveDataSource"] ?? "CSV";
if (activeDataSource.Equals("EF", StringComparison.OrdinalIgnoreCase))
{
    await InitializeDatabaseWithCsvData(app);
}

app.Run();


async Task InitializeDatabaseWithCsvData(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PersonDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Persons.AnyAsync())
    {
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var location = cfg["DataSources:PersonCsv:Location"];
        if (string.IsNullOrWhiteSpace(location)) location = AppContext.BaseDirectory;
        var filename = cfg["DataSources:PersonCsv:Filename"] ?? "sample-input.csv";
        var path = Path.Combine(location, filename);

        var persons = CsvLoader.Load(path);
        await db.Persons.AddRangeAsync(persons);
        await db.SaveChangesAsync();
    }
}
