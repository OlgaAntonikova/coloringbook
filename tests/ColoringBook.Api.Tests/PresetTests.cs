using System.Net.Http.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColoringBook.Api.Tests;

public class PresetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PresetTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private sealed record PresetDto(string Key, string Title);
    private sealed record GenerateResultDto(string Id, string Url, string Format);

    [Fact]
    public async Task Presets_ListsEveryAnimalWithEnglishTitle()
    {
        var presets = await _factory.CreateClient().GetFromJsonAsync<List<PresetDto>>("/presets");

        Assert.Equal(new[] { "bear", "cat", "dog", "fox", "lion", "rabbit", "turtle" }, presets!.Select(p => p.Key));
        Assert.Contains(presets!, p => p.Key == "turtle" && p.Title == "Turtle");
    }

    [Theory]
    [InlineData("bear")]
    [InlineData("cat")]
    [InlineData("dog")]
    [InlineData("fox")]
    [InlineData("lion")]
    [InlineData("rabbit")]
    [InlineData("turtle")]
    public async Task Generate_ProducesValidSvgForEveryAnimal(string animal)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/generate",
            new { Animal = animal, LineThickness = 4, AddLabels = true, Format = "svg" });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GenerateResultDto>();
        var svg = XDocument.Parse(await client.GetStringAsync(result!.Url));

        var shapes = svg.Descendants().Where(e => e.Name.LocalName is "path" or "circle" or "ellipse").ToList();
        Assert.NotEmpty(shapes);
        Assert.All(shapes, s => Assert.Equal("4", s.Attribute("stroke-width")?.Value));
        Assert.Contains(svg.Descendants(), e => e.Name.LocalName == "text" && e.Value == animal.ToUpperInvariant());
    }
}
