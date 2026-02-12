using Google.Apis.Auth;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Services.Google;

public class GoogleIdTokenValidator : IGoogleIdTokenValidator
{
    public async Task<GoogleIdTokenPayload> ValidateAsync(string idToken, string clientId, CancellationToken cancellationToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(
            idToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

        return new GoogleIdTokenPayload(
            Email: payload.Email,
            Name: payload.Name,
            Subject: payload.Subject,
            EmailVerified: payload.EmailVerified == true
        );
    }
}
