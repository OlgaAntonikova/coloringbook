using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColoringBook.Api.Tests;

public class PdfExportTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PdfExportTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private sealed record GenerateResultDto(string Id, string Url, string Format);

    [Fact]
    public async Task Pdf_IsSingleA4PageOfVectorGraphics()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/generate",
            new { Animal = "lion", LineThickness = 3, AddLabels = false, Format = "pdf" });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GenerateResultDto>();

        var bytes = await client.GetByteArrayAsync(result!.Url);
        var text = Encoding.Latin1.GetString(bytes);

        Assert.StartsWith("%PDF-", text);
        Assert.Single(Regex.Matches(text, @"/Type\s*/Page\b"));
        // A4 in points: 595 x 842
        Assert.Matches(@"/MediaBox\s*\[\s*0\s+0\s+595(\.\d+)?\s+842(\.\d+)?\s*\]", text);
        // Vector output embeds no raster images (the previous version embedded a 300 DPI PNG)
        Assert.DoesNotMatch(@"/Subtype\s*/Image", text);
        Assert.True(bytes.Length < 50_000, $"PDF is {bytes.Length} bytes");
    }
}
