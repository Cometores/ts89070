using CSVService.API.Data;
using CSVService.API.Data.CSV;
using CSVService.API.Data.DB;
using CSVService.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace CSVService.API.Extensions;

internal static class StartupHelperExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        string activeDataSource = builder.Configuration["DataSources:ActiveDataSource"] ?? "CSV";
        builder.RegisterDataContexts(activeDataSource);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddProblemDetails();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.RegisterPersonsEndpoints();

        return app;
    }

    private static void RegisterDataContexts(this WebApplicationBuilder builder, string activeDataSource)
    {
        if (activeDataSource.Equals("EF", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddDbContext<PersonDbContext>(opt =>
                opt.UseSqlite(builder.Configuration.GetConnectionString("PersonDb")));
            builder.Services.AddScoped<IPersonRepository, EfPersonRepository>();
        }
        else
        {
            builder.Services.AddScoped<PersonCsvStore>();
            builder.Services.AddScoped<IPersonRepository, CsvPersonRepository>();
        }
    }
}