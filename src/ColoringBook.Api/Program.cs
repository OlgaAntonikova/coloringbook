using ColoringBook.Api.Endpoints;
using ColoringBook.Api.Services;
using ColoringBook.Api.Storage;
using Serilog;
using QuestPDF.Infrastructure;
QuestPDF.Settings.License = LicenseType.Community;


var builder = WebApplication.CreateBuilder(args);


// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
	.ReadFrom.Configuration(ctx.Configuration)
	.WriteTo.Console());


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHealthChecks();


// DI
builder.Services.AddSingleton<IOutlineGenerator, SvgOutlineGenerator>();
builder.Services.AddSingleton<IPdfExporter, QuestPdfExporter>();
builder.Services.AddSingleton<IFileStorage>(sp =>
	new LocalFileStorage(Path.Combine(AppContext.BaseDirectory, "files")));
builder.Services.AddSingleton<IPresetProvider>(sp =>
    new FilePresetProvider(builder.Environment.ContentRootPath));

// CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("web",
        p => p.WithOrigins("https://localhost:7280", "http://localhost:7280")
            .AllowAnyHeader()
            .AllowAnyMethod());
});


var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

app.UseCors("web");

// Endpoints
app.MapGenerateEndpoints();
app.MapFilesEndpoints();

app.Run();
