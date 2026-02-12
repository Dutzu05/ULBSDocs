namespace UlbsDocAuth.Api.Services.Interfaces;

public record GoogleIdTokenPayload(
    string Email,
    string? Name,
    string Subject,
    bool EmailVerified
);

public interface IGoogleIdTokenValidator
{
    Task<GoogleIdTokenPayload> ValidateAsync(string idToken, string clientId, CancellationToken cancellationToken);
}
