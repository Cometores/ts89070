using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ts89070.Data;
using ts89070.Domain;
using ts89070.Services;

var builder = WebApplication.CreateBuilder(args);

// ЧТО использовать: CSV или EF
// Ключ в README написан как DataSoruces:ActiveDataSource (опечатка).
// Поддержим оба: DataSources:ActiveDataSource и DataSoruces:ActiveDataSource. :contentReference[oaicite:3]{index=3}
string activeDataSource =
    builder.Configuration["DataSources:ActiveDataSource"]
    ?? builder.Configuration["DataSoruces:ActiveDataSource"] // на случай опечатки
    ?? "CSV";

// DI: репозиторий
if (activeDataSource.Equals("EF", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<PersonDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("PersonDb")));
    builder.Services.AddScoped<IPersonRepository, EfPersonRepository>();
}
else
{
    builder.Services.AddSingleton<PersonCsvStore>();
    builder.Services.AddSingleton<IPersonRepository, CsvPersonRepository>();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (activeDataSource.Equals("EF", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PersonDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Persons.AnyAsync())
    {
        // Притащим данные из CSV через твой CsvLoader
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var location = cfg["DataSources:PersonCsv:Location"];
        if (string.IsNullOrWhiteSpace(location)) location = AppContext.BaseDirectory;
        var filename = cfg["DataSources:PersonCsv:Filename"] ?? "sample-input.csv";
        var path = Path.Combine(location, filename);

        var persons = ts89070.Services.CsvLoader.Load(path);
        await db.Persons.AddRangeAsync(persons);
        await db.SaveChangesAsync();
    }
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

// --- Endpoints ---
// GET /persons
app.MapGet("/persons", async ([FromServices] IPersonRepository repo, CancellationToken ct) =>
{
    var list = await repo.GetAllAsync(ct);
    return Results.Ok(list); // 200
})
.WithName("GetAllPersons")
.Produces<List<Person>>(StatusCodes.Status200OK);

// GET /persons/{id}
app.MapGet("/persons/{id}", async (string id, [FromServices] IPersonRepository repo, CancellationToken ct) =>
{
    if (!int.TryParse(id, out var parsed)) // 400 если id не число (требование README) :contentReference[oaicite:4]{index=4}
        return Results.BadRequest(new { error = "id must be a number" });

    var person = await repo.GetByIdAsync(parsed, ct);
    return person is null ? Results.NotFound() : Results.Ok(person);
})
.WithName("GetPersonById")
.Produces<Person>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

// GET /persons/color/{color}
app.MapGet("/persons/color/{color}", async (string color, [FromServices] IPersonRepository repo, CancellationToken ct) =>
{
    var list = await repo.GetByColorAsync(color, ct);
    return list.Count == 0 ? Results.NotFound() : Results.Ok(list);
})
.WithName("GetPersonsByColor")
.Produces<List<Person>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// POST /persons
// 200 + созданная Person; 400 если не все поля заданы (кроме Id) — по README. :contentReference[oaicite:5]{index=5}
app.MapPost("/persons", async ([FromBody] Person input, [FromServices] IPersonRepository repo, CancellationToken ct) =>
{
    // Простая валидация
    if (string.IsNullOrWhiteSpace(input.Name) ||
        string.IsNullOrWhiteSpace(input.Lastname) ||
        string.IsNullOrWhiteSpace(input.Zipcode) ||
        string.IsNullOrWhiteSpace(input.City) ||
        string.IsNullOrWhiteSpace(input.Color))
    {
        return Results.BadRequest(new { error = "All properties except Id are required." });
    }

    // Id игнорируем — задаётся репозиторием
    input.Id = 0;
    var created = await repo.CreateAsync(input, ct);
    return Results.Ok(created);
})
.WithName("CreatePerson")
.Produces<Person>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.Run();
