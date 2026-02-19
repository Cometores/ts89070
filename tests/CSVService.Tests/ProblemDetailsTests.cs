using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CSVService.Tests;

[TestFixture]
public class ProblemDetailsTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task NotFoundRoute_ReturnsProblemDetails()
    {
        // Act
        var response = await _client.GetAsync("/non-existent-route");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problemDetails, Is.Not.Null);
        Assert.That(problemDetails!.Status, Is.EqualTo((int)HttpStatusCode.NotFound));
        Assert.That(problemDetails.Title, Is.Not.Null.Or.Empty);
    }

    [Test]
    public async Task UnsupportedAccept_ReturnsProblemDetails()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/persons");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        var response = await _client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problemDetails, Is.Not.Null);
        Assert.That(problemDetails!.Status, Is.EqualTo((int)HttpStatusCode.NotAcceptable));
    }

    [Test]
    public async Task GetPersonById_WithInvalidId_ReturnsProblemDetails()
    {
        // Act
        var response = await _client.GetAsync("/persons/eric");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problemDetails, Is.Not.Null);
        Assert.That(problemDetails!.Status, Is.EqualTo((int)HttpStatusCode.BadRequest));
        Assert.That(problemDetails.Detail, Is.EqualTo("id must be a number"));
    }
}