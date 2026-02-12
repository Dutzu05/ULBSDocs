using Microsoft.AspNetCore.Mvc;
using Spire.Doc;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("api/doc-docx")]
public class DocDocxConv(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("convert")]
    public IActionResult Convert()
    {
        // calea catre fisierul template
        string templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            "Adeverinta.doc"
        );

        // incarcam documentul .doc
        Document document = new Document();
        document.LoadFromFile(templatePath);

        // il salvam in memorie ca .docx
        MemoryStream ms = new MemoryStream();
        document.SaveToStream(ms, FileFormat.Docx);

        // returnam fisierul rezultat
        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "Adeverinta.docx"
        );
    }

    [HttpPost("convert")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ConvertUpload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
            return BadRequest(new { error = "Missing file." });

        if (file.Length > 25 * 1024 * 1024)
            return BadRequest(new { error = "File too large (max 25MB)." });

        var fileName = file.FileName ?? "input.doc";
        if (!fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .doc files are supported." });

        var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.doc");
        var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.docx");

        try
        {
            await using (var inputStream = System.IO.File.Create(tempInput))
            {
                await file.CopyToAsync(inputStream, cancellationToken);
            }

            var document = new Document();
            document.LoadFromFile(tempInput);
            document.SaveToFile(tempOutput, FileFormat.Docx);

            var bytes = await System.IO.File.ReadAllBytesAsync(tempOutput, cancellationToken);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                Path.ChangeExtension(fileName, ".docx"));
        }
        catch (Exception ex)
        {
            var details = environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new { error = "DOC to DOCX conversion failed.", details });
        }
        finally
        {
            try { System.IO.File.Delete(tempInput); } catch { /* ignore */ }
            try { System.IO.File.Delete(tempOutput); } catch { /* ignore */ }
        }
    }
}
