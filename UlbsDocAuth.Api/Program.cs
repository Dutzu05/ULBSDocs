using UlbsDocAuth.Api.Services.Interfaces;
using UlbsDocAuth.Api.Services.Mock;
using UlbsDocAuth.Api.Services.Google;
using UlbsDocAuth.Api.Services.DocxToPdf;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton<IDocxToPdfConverter>(_ =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? new SpireDocxToPdfConverter()
        : new LibreOfficeDocxToPdfConverter());

var app = builder.Build();

app.UseCors("DevCors");

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
