using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using UlbsDocAuth.Api.DTOs;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.IdToken))
        {
            return BadRequest(new { error = "Missing Google ID token" });
        }


        var clientId = _config["GoogleAuth:ClientId"];

        if (string.IsNullOrEmpty(clientId))
            return StatusCode(500, "Google ClientId not configured");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google token");
        }
        catch (Exception)
        {
            return StatusCode(500, "Google token validation failed");
        }

        // At this point, the token is VERIFIED
        // payload contains trusted user info
        if (payload.EmailVerified != true)
            return Forbid("Google account email is not verified");

        return Ok(new
        {
            email = payload.Email,
            name = payload.Name,
            subject = payload.Subject // Google's unique user ID
        });
    }
}
