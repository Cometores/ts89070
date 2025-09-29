namespace ts89070.Domain;

public sealed class Person
{
    public int Id { get; set; }                // ID или номер строки в CSV
    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Zipcode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Color { get; set; } = null!;
}