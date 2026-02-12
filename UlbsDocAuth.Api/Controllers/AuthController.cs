using Microsoft.AspNetCore.Mvc;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IGoogleIdTokenValidator _tokenValidator;

    public AuthController(IConfiguration config, IGoogleIdTokenValidator tokenValidator)
    {
        _config = config;
        _tokenValidator = tokenValidator;
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

        GoogleIdTokenPayload payload;
        try
        {
            payload = await _tokenValidator.ValidateAsync(request.IdToken, clientId, HttpContext.RequestAborted);
        }
        catch (Exception ex) when (ex.GetType().Name is "InvalidJwtException")
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
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Google account email is not verified" });

        return Ok(new
        {
            email = payload.Email,
            name = payload.Name,
            subject = payload.Subject // Google's unique user ID
        });
    }
}
