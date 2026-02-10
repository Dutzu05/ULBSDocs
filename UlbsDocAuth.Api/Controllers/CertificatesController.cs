using Microsoft.AspNetCore.Mvc;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController(ICertificateDataService service) : ControllerBase
{
    [HttpGet("mock")]
    public IActionResult GetMockByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest(new { error = "Invalid email." });

        var result = service.GetByEmail(email);
        if (result is null)
            return NotFound(new { error = "No mock data for this email." });

        return Ok(result);
    }
}
