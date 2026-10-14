using ColoringBook.Contracts;
using ColoringBook.Api.Services;
using ColoringBook.Api.Storage;
using Microsoft.AspNetCore.OpenApi;


namespace ColoringBook.Api.Endpoints
{
	public static class GenerateEndpoints
	{
		public static IEndpointRouteBuilder MapGenerateEndpoints(this IEndpointRouteBuilder app)
		{
            // Available animals: one SVG file per animal in Presets/Animals
            app.MapGet("/presets", (IPresetProvider presets) =>
                Results.Ok(presets.PresetsList()
                    .Select(key => new { key, title = char.ToUpperInvariant(key[0]) + key[1..] })));

            app.MapPost("/generate", async (DTOs.GenerateRequest request, ColoringPageBuilder builder,
                IPdfExporter pdf, IFileStorage storage, ILogger<ColoringPageBuilder> logger) =>
            {
                ColoringPage page;
                try
                {
                    page = builder.Build(request);
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
                }

                try
                {
                    var id = Guid.NewGuid().ToString("N");
                    var ext = request.Format.Equals("svg", StringComparison.OrdinalIgnoreCase) ? "svg" : "pdf";
                    var filePath = storage.GetPath(id, ext);

                    if (ext == "svg")
                        await File.WriteAllTextAsync(filePath, page.Svg);
                    else
                        await pdf.ExportSvgToPdfAsync(page.Svg, filePath);

                    return Results.Ok(new DTOs.GenerateResult(id, $"/files/{id}.{ext}", ext, page.Seed));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Export failed for {Kind}", request.Kind);
                    return Results.Problem("Could not create the file.", statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithOpenApi()
            .Produces<DTOs.GenerateResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

			return app;
		}
	}
}
