using Microsoft.AspNetCore.Mvc;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("api/docx-to-pdf")]
public class DocxToPdfController(IHostEnvironment environment, IDocxToPdfConverter converter) : ControllerBase
{
    [HttpPost("convert")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Convert(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            return BadRequest(new { error = "Missing file." });

        if (file.Length > 25 * 1024 * 1024)
            return BadRequest(new { error = "File too large (max 25MB)." });

        var fileName = file.FileName ?? "input.docx";
        if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .docx files are supported." });

        var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.docx");
        var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

        try
        {
            await using (var inputStream = System.IO.File.Create(tempInput))
            {
                await file.CopyToAsync(inputStream, cancellationToken);
            }

            try
            {
                await converter.ConvertAsync(tempInput, tempOutput, cancellationToken);
            }
            catch (Exception startEx) when (startEx is System.ComponentModel.Win32Exception)
            {
                // e.g. 'soffice' missing on Linux
                return StatusCode(501, new
                {
                    error = "DOCX->PDF converter dependency is missing.",
                    details = environment.IsDevelopment() ? startEx.ToString() : startEx.Message,
                    hint = "On Linux: sudo apt update && sudo apt install -y libreoffice"
                });
            }

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(tempOutput, cancellationToken);
            return File(pdfBytes, "application/pdf", Path.ChangeExtension(fileName, ".pdf"));
        }
        catch (Exception ex)
        {
            var details = environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new { error = "DOCX to PDF conversion failed.", details });
        }
        finally
        {
            try { System.IO.File.Delete(tempInput); } catch { /* ignore */ }
            try { System.IO.File.Delete(tempOutput); } catch { /* ignore */ }
        }
    }
}
