using ColoringBook.Api.Storage;
using Microsoft.AspNetCore.StaticFiles;

namespace ColoringBook.Api.Endpoints
{
	public static class FilesEndpoints
	{
		public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
		{
			app.MapGet("/files/{file}", (string file, IFileStorage storage) =>
			{
				var full = storage.Resolve(file);
				if (!System.IO.File.Exists(full)) return Results.NotFound();


				var provider = new FileExtensionContentTypeProvider();
				if (!provider.TryGetContentType(full, out var contentType))
					contentType = "application/octet-stream";


				return Results.File(full, contentType);
			});
			return app;
		}
	}
}
