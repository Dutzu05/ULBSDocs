using Microsoft.AspNetCore.Mvc;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;
using UlbsDocAuth.Api.Services; 

namespace UlbsDocAuth.Api.Controllers;

public record GenerateRequest(string Email, string Reason);

[ApiController]
[Route("api/certificates")]
public class CertificatesController(
    ICertificateDataService dataService,    
    IWordTemplateService templateService,   
    IDocxToPdfConverter pdfConverter        
    ) : ControllerBase
{
    // --- ACEASTA ESTE METODA CARE LIPSEA SAU AVEA NUME GREȘIT ---
    // Ruta trebuie să fie "mock" pentru că așa o apelează app.js
    [HttpGet("mock")]
    public IActionResult GetMockByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email)) 
            return BadRequest("Emailul este obligatoriu.");

        var student = dataService.GetByEmail(email);
        
        if (student is null) 
            return NotFound("Nu am găsit niciun student cu acest email.");
            
        return Ok(student);
    }
    // -------------------------------------------------------------

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCertificate([FromBody] GenerateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) 
            return BadRequest("Emailul este obligatoriu.");

        var student = dataService.GetByEmail(request.Email);
        if (student is null) 
            return NotFound("Studentul nu a fost găsit.");

        string? tempDocxPath = null;
        string? tempPdfPath = null;

        try
        {
            // 1. Generăm DOCX
            tempDocxPath = templateService.GenerateDocx(student, request.Reason);

            // 2. Pregătim PDF
            tempPdfPath = Path.ChangeExtension(tempDocxPath, ".pdf");

            // 3. Convertim
            await pdfConverter.ConvertAsync(tempDocxPath, tempPdfPath, CancellationToken.None);

            if (!System.IO.File.Exists(tempPdfPath))
                throw new Exception("PDF-ul nu a fost creat.");

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(tempPdfPath);
            var downloadName = $"Adeverinta_{student.FullName.Replace(" ", "_")}.pdf";
            
            return File(pdfBytes, "application/pdf", downloadName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Eroare server", details = ex.Message });
        }
        finally
        {
            if (tempDocxPath != null && System.IO.File.Exists(tempDocxPath))
                System.IO.File.Delete(tempDocxPath);

            if (tempPdfPath != null && System.IO.File.Exists(tempPdfPath))
                System.IO.File.Delete(tempPdfPath);
        }
    }
}