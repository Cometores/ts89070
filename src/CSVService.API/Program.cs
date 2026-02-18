using CSVService.API.Data;
using CSVService.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

string activeDataSource = builder.Configuration["DataSources:ActiveDataSource"] ?? "CSV";
if (activeDataSource.Equals("EF", StringComparison.OrdinalIgnoreCase))
    await Seeder.PopulateDatabaseAsync(app);

app.Run();
