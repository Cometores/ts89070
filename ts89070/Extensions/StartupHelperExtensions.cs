using Microsoft.EntityFrameworkCore;
using ts89070.Data;
using ts89070.Data.CSV;
using ts89070.Data.DB;
using ts89070.Domain;

namespace ts89070.Extensions;

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
        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();
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