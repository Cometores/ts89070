using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CSVService.Tests;

[TestFixture]
public class ContentNegotiationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

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
    public async Task GetPersons_WithUnsupportedAccept_Returns406()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/persons");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        var response = await _client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
    }

    [Test]
    public async Task GetPersons_WithWildcardAccept_AllowsJson()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/persons");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        var response = await _client.SendAsync(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetPersons_WithVendorJsonAccept_AllowsJson()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/persons");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

        var response = await _client.SendAsync(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }
}
