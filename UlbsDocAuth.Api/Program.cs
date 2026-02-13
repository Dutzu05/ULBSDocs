using UlbsDocAuth.Api.Services.Interfaces;
using UlbsDocAuth.Api.Services.Mock;
using UlbsDocAuth.Api.Services.Google;
using UlbsDocAuth.Api.Services;
using UlbsDocAuth.Api.Services.DocxToPdf;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;

// Prefer serving the repo-level /frontend as the web root when present.
// IMPORTANT: WebRoot must be configured via WebApplicationOptions during CreateBuilder.
var frontendWebRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "frontend"));
var builderOptions = Directory.Exists(frontendWebRoot)
    ? new WebApplicationOptions { Args = args, WebRootPath = frontendWebRoot }
    : new WebApplicationOptions { Args = args };

var builder = WebApplication.CreateBuilder(builderOptions);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register mock service
builder.Services.AddSingleton<ICertificateDataService, MockCertificateDataService>();

builder.Services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
builder.Services.AddScoped<IWordTemplateService, WordTemplateService>();

builder.Services.AddSingleton<IDocxToPdfConverter>(_ =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? new SpireDocxToPdfConverter()
        : new LibreOfficeDocxToPdfConverter());

var app = builder.Build();

app.UseCors("DevCors");

app.UseDefaultFiles();
app.UseStaticFiles();

// Also serve UlbsDocAuth.Api/wwwroot as a secondary static source (e.g. legacy google-test.html)
var legacyWwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var currentWebRoot = app.Environment.WebRootPath;
if (Directory.Exists(legacyWwwroot)
    && !string.Equals(
        Path.GetFullPath(legacyWwwroot).TrimEnd(Path.DirectorySeparatorChar),
        Path.GetFullPath(currentWebRoot ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar),
        StringComparison.OrdinalIgnoreCase))
{
    var legacyProvider = new PhysicalFileProvider(legacyWwwroot);
    app.UseStaticFiles(new StaticFileOptions { FileProvider = legacyProvider });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
