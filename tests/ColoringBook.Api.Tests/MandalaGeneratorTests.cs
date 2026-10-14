using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ColoringBook.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColoringBook.Api.Tests;

public class MandalaGeneratorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly MandalaGenerator _generator = new();
    private readonly WebApplicationFactory<Program> _factory;

    public MandalaGeneratorTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public void SameOptions_GiveTheSamePage()
    {
        var options = new MandalaOptions(Symmetry: 12, Rings: 5, Seed: 42, LineThickness: 3);

        Assert.Equal(_generator.BuildSvg(options), _generator.BuildSvg(options));
    }

    [Fact]
    public void DifferentSeeds_GiveDifferentPages()
    {
        var a = _generator.BuildSvg(new MandalaOptions(12, 5, 1, 3));
        var b = _generator.BuildSvg(new MandalaOptions(12, 5, 2, 3));

        Assert.NotEqual(a, b);
    }

    [Theory]
    [InlineData(4, 2)]
    [InlineData(12, 5)]
    [InlineData(24, 8)]
    public void Page_HasRequestedRingsAndSymmetry(int symmetry, int rings)
    {
        var svg = XDocument.Parse(_generator.BuildSvg(new MandalaOptions(symmetry, rings, 7, 3)));
        var root = svg.Root!;

        // Two center circles plus one boundary circle per ring
        Assert.Equal(2 + rings, root.Elements().Count(e => e.Name.LocalName == "circle"));

        // Every rotation angle is a multiple of half a symmetry step, and each angle is used once per layer
        var step = 360.0 / symmetry;
        var angles = root.Elements()
            .Where(e => e.Name.LocalName == "g")
            .Select(g => double.Parse(Regex.Match(g.Attribute("transform")!.Value, @"rotate\(([\d.]+)").Groups[1].Value,
                System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        Assert.All(angles, a => Assert.True(Math.Abs(a / (step / 2) - Math.Round(a / (step / 2))) < 1e-6, $"angle {a}"));
        Assert.Equal(symmetry * rings, angles.Count(a => Math.Abs(a / step - Math.Round(a / step)) < 1e-6));
    }

    [Fact]
    public void EveryShapeUsesRequestedLineThickness()
    {
        var svg = XDocument.Parse(_generator.BuildSvg(new MandalaOptions(10, 4, 3, 5)));

        var shapes = svg.Descendants().Where(e => e.Name.LocalName is "path" or "circle").ToList();
        Assert.NotEmpty(shapes);
        Assert.All(shapes, s => Assert.Equal("5", s.Attribute("stroke-width")?.Value));
        Assert.All(shapes, s => Assert.Equal("none", s.Attribute("fill")?.Value));
    }

    [Theory]
    [InlineData(3, 5, 3)]
    [InlineData(25, 5, 3)]
    [InlineData(12, 1, 3)]
    [InlineData(12, 9, 3)]
    [InlineData(12, 5, 0)]
    [InlineData(12, 5, 11)]
    public void InvalidOptions_AreRejected(int symmetry, int rings, int thickness)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _generator.BuildSvg(new MandalaOptions(symmetry, rings, 1, thickness)));
    }

    private sealed record GenerateResultDto(string Id, string Url, string Format, int? Seed);

    [Fact]
    public async Task Generate_ReturnsSeedThatRecreatesTheSamePage()
    {
        var client = _factory.CreateClient();
        var request = new { Kind = "mandala", Symmetry = 8, Rings = 4, Format = "svg" };

        var first = await (await client.PostAsJsonAsync("/generate", request)).Content.ReadFromJsonAsync<GenerateResultDto>();
        var again = await (await client.PostAsJsonAsync("/generate", new { request.Kind, request.Symmetry, request.Rings, request.Format, first!.Seed }))
            .Content.ReadFromJsonAsync<GenerateResultDto>();

        Assert.NotNull(first.Seed);
        Assert.Equal(await client.GetStringAsync(first.Url), await client.GetStringAsync(again!.Url));
    }

    [Theory]
    [InlineData("{\"Kind\":\"mandala\",\"Symmetry\":99}", "Symmetry")]
    [InlineData("{\"Kind\":\"animal\"}", "Choose an animal")]
    [InlineData("{\"Kind\":\"animal\",\"Animal\":\"unicorn\"}", "Unknown animal")]
    [InlineData("{\"Kind\":\"spiral\"}", "Kind must be")]
    [InlineData("{\"Kind\":\"animal\",\"Animal\":\"cat\",\"LineThickness\":0}", "Line thickness")]
    public async Task Generate_ReturnsBadRequestWithReason(string json, string expectedMessage)
    {
        var response = await _factory.CreateClient().PostAsync("/generate",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(expectedMessage, await response.Content.ReadAsStringAsync());
    }
}
