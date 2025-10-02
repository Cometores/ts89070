using ts89070.Domain;

namespace ts89070.EndpointHandlers;

public static class PersonsHandler
{
    public static async Task<IResult> GetPersonsAsync(IPersonRepository repo, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(ct);
        return Results.Ok(list);
    }
    
    public static async Task<IResult> GetPersonByIdAsync(string id, IPersonRepository repo, CancellationToken ct)
    {
        if (!int.TryParse(id, out var parsed))
            return Results.BadRequest(new { error = "id must be a number" });

        var person = await repo.GetByIdAsync(parsed, ct);
        return person is null ? Results.NotFound() : Results.Ok(person);
    }
    
    public static async Task<IResult> GetPersonByColorAsync(string color, IPersonRepository repo, CancellationToken ct)
    {
        var list = await repo.GetByColorAsync(color, ct);
        return list.Count == 0 ? Results.NotFound() : Results.Ok(list);
    }
    
    public static async Task<IResult> CreatePersonAsync(Person input, IPersonRepository repo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) 
            || string.IsNullOrWhiteSpace(input.Lastname) 
            || string.IsNullOrWhiteSpace(input.Zipcode) 
            || string.IsNullOrWhiteSpace(input.City) 
            || string.IsNullOrWhiteSpace(input.Color))
        {
            return Results.BadRequest(new { error = "All properties except Id are required." });
        }
        
        // id created by the repository
        input.Id = 0;
        var created = await repo.CreateAsync(input, ct);
        return Results.Ok(created);
    }
}