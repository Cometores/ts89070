using CSVService.API.Domain;
using CSVService.API.EndpointHandlers;

namespace CSVService.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterPersonsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/persons", PersonsHandler.GetPersonsAsync)
            .WithName("GetAllPersons")
            .Produces<List<Person>>();

        endpointRouteBuilder.MapGet("/persons/{id}", PersonsHandler.GetPersonByIdAsync)
            .WithName("GetPersonById")
            .Produces<Person>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        endpointRouteBuilder.MapGet("/persons/color/{colorOrId}", PersonsHandler.GetPersonsByColorAsync)
            .WithName("GetPersonsByColor")
            .Produces<List<Person>>()
            .Produces(StatusCodes.Status404NotFound);

        endpointRouteBuilder.MapPost("/persons", PersonsHandler.CreatePersonAsync)
            .WithName("CreatePerson")
            .Produces<Person>()
            .Produces(StatusCodes.Status400BadRequest);
    }
}