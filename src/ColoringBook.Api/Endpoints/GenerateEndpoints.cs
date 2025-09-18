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
            // app.MapGet("/presets", (IPresetProvider presets) => Results.Ok(presets.PresetsList()));

            app.MapGet("/presets/localized", () =>
            {
                var list = new[]
                {
                    new { key="cat",      title="Кошка" },
                    new { key="dog",      title="Собака" },
                    new { key="rabbit",   title="Заяц" },
                    new { key="lion",     title="Лев" },
                    new { key="fox",      title="Лиса" },
                    new { key="bear",     title="Медведь" },
                    new { key="turtle",   title="Черепаха" },
                };
                return Results.Ok(list);
            });

            app.MapPost("/generate", async (DTOs.GenerateRequest request, IOutlineGenerator outlineGen, IPdfExporter pdf, IFileStorage storage) =>
				{
                    try
                    {
                        // 1) генерим SVG контур по животному
                        var svg = outlineGen.BuildSvg(request.Animal, request.LineThickness, request.AddLabels);


                        // 2) экспортируем
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
