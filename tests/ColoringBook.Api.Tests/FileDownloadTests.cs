using System.Net;
using System.Net.Http.Json;
using ColoringBook.Api.Storage;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColoringBook.Api.Tests;

public class FileDownloadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FileDownloadTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData("..\\appsettings.json")]
    [InlineData("../appsettings.json")]
    [InlineData("appsettings.json")]
    [InlineData("0123456789abcdef0123456789abcdef.exe")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF.svg")]
    public void Resolve_RejectsAnythingButGeneratedFileNames(string fileName)
    {
        var storage = new LocalFileStorage(Path.Combine(Path.GetTempPath(), "coloringbook-tests"));

        Assert.Null(storage.Resolve(fileName));
    }

    [Fact]
    public void Resolve_MapsGeneratedFileNameIntoStorageFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "coloringbook-tests");
        var storage = new LocalFileStorage(root);

        var path = storage.Resolve("0123456789abcdef0123456789abcdef.pdf");

        Assert.Equal(Path.Combine(Path.GetFullPath(root), "0123456789abcdef0123456789abcdef.pdf"), path);
    }

    [Theory]
    [InlineData("/files/..%5Cappsettings.json")]
    [InlineData("/files/..%2Fappsettings.json")]
    [InlineData("/files/ColoringBook.Api.dll")]
    public async Task Download_DoesNotServeFilesOutsideStorage(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_ServesGeneratedFile()
    {
        var client = _factory.CreateClient();
        var generated = await client.PostAsJsonAsync("/generate",
            new { Animal = "cat", LineThickness = 3, AddLabels = false, Format = "svg" });
        generated.EnsureSuccessStatusCode();
        var result = await generated.Content.ReadFromJsonAsync<GenerateResultDto>();

        var response = await client.GetAsync(result!.Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
    }

    private sealed record GenerateResultDto(string Id, string Url, string Format);
}
