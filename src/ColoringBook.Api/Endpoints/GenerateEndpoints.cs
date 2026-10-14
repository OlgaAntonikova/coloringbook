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

            app.MapPost("/generate", async (DTOs.GenerateRequest request, IOutlineGenerator outlineGen, IPdfExporter pdf, IFileStorage storage) =>
				{
                    try
                    {
                        // 1) Build the SVG outline for the animal
                        var svg = outlineGen.BuildSvg(request.Animal, request.LineThickness, request.AddLabels);


                        // 2) Export it
                        string id = Guid.NewGuid().ToString("N");
                        string ext = request.Format.ToLowerInvariant() == "svg" ? "svg" : "pdf";
                        string filePath = storage.GetPath(id, ext);


                        if (ext == "svg")
                        {
                            await File.WriteAllTextAsync(filePath, svg);
                        }
                        else
                        {
                            await pdf.ExportSvgToPdfAsync(svg, filePath);
                        }


                        string url = $"/files/{id}.{ext}";
                        return Results.Ok(new DTOs.GenerateResult(id, url, ext));
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message, statusCode: 500);
                    }
                })
				.WithOpenApi()
				.Produces<DTOs.GenerateResult>(StatusCodes.Status200OK);


			return app;
		}
	}
}
