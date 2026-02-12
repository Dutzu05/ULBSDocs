using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IGoogleIdTokenValidator, FakeGoogleIdTokenValidator>();
            services.AddSingleton<IDocxToPdfConverter, FakeDocxToPdfConverter>();
        });
    }

    private sealed class FakeGoogleIdTokenValidator : IGoogleIdTokenValidator
    {
        public Task<GoogleIdTokenPayload> ValidateAsync(string idToken, string clientId, CancellationToken cancellationToken)
        {
            if (idToken == "invalid")
                throw new InvalidJwtException();

            if (idToken == "unverified")
                return Task.FromResult(new GoogleIdTokenPayload("user@example.com", "User", "sub", EmailVerified: false));

            return Task.FromResult(new GoogleIdTokenPayload("user@example.com", "User", "sub", EmailVerified: true));
        }

        private sealed class InvalidJwtException : Exception { }
    }

    private sealed class FakeDocxToPdfConverter : IDocxToPdfConverter
    {
        public Task ConvertAsync(string inputDocxPath, string outputPdfPath, CancellationToken cancellationToken)
        {
            // Minimal PDF header bytes; enough for endpoint test.
            File.WriteAllBytes(outputPdfPath, "%PDF-1.7\n"u8.ToArray());
            return Task.CompletedTask;
        }
    }
}
