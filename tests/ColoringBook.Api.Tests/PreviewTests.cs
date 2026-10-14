using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColoringBook.Api.Tests;

public class PreviewTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PreviewTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Preview_ReturnsSvgAndSeed_WithoutStoringAFile()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/preview", new { Kind = "mandala", Symmetry = 10, Rings = 4, Seed = 123 });

        response.EnsureSuccessStatusCode();
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("123", Assert.Single(response.Headers.GetValues("X-Seed")));
        Assert.Equal("svg", XDocument.Parse(await response.Content.ReadAsStringAsync()).Root!.Name.LocalName);
    }

    [Fact]
    public async Task Preview_MatchesTheGeneratedFile()
    {
        var client = _factory.CreateClient();
        var request = new { Kind = "mandala", Symmetry = 8, Rings = 3, Seed = 9, Format = "svg" };

        var preview = await (await client.PostAsJsonAsync("/preview", request)).Content.ReadAsStringAsync();
        var generated = await (await client.PostAsJsonAsync("/generate", request)).Content.ReadFromJsonAsync<GenerateResultDto>();

        Assert.Equal(preview, await client.GetStringAsync(generated!.Url));
    }

    [Fact]
    public async Task Preview_ReturnsBadRequestForInvalidInput()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/preview", new { Kind = "mandala", Rings = 20 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Rings", await response.Content.ReadAsStringAsync());
    }

    private sealed record GenerateResultDto(string Id, string Url, string Format, int? Seed);
}
