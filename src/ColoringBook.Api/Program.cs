using ColoringBook.Api.Endpoints;
using ColoringBook.Api.Services;
using ColoringBook.Api.Storage;
using QuestPDF.Infrastructure;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
	.ReadFrom.Configuration(ctx.Configuration)
	.WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IOutlineGenerator, SvgOutlineGenerator>();
builder.Services.AddSingleton<IPdfExporter, QuestPdfExporter>();
builder.Services.AddSingleton<MandalaGenerator>();
builder.Services.AddSingleton<ColoringPageBuilder>();
builder.Services.AddSingleton<IPresetProvider>(_ => new FilePresetProvider(builder.Environment.ContentRootPath));

// Generated files; Storage:Path lets a container keep them on a volume
var storagePath = builder.Configuration["Storage:Path"] ?? Path.Combine(AppContext.BaseDirectory, "files");
builder.Services.AddSingleton<IFileStorage>(_ => new LocalFileStorage(storagePath));

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// The Blazor WebAssembly client is served by this app, so UI and API share one origin
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapGenerateEndpoints();
app.MapFilesEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

// Lets integration tests start the app with WebApplicationFactory<Program>
public partial class Program;
