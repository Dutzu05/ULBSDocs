using Microsoft.AspNetCore.Mvc;
using Spire.Doc;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("api/docx-to-pdf")]
public class DocxToPdfController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("convert")]
    public IActionResult ConvertInfo()
    {
        return Ok(new
        {
            message = "Use POST /api/docx-to-pdf/convert with multipart/form-data.",
            field = "file",
            exampleCurl = "curl -X POST http://localhost:3000/api/docx-to-pdf/convert -F \"file=@/path/to/input.docx\" --output output.pdf"
        });
    }

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var document = new Document();
                document.LoadFromFile(tempInput);
                document.SaveToFile(tempOutput, FileFormat.PDF);
            }
            else
            {
                // .NET 8: System.Drawing.Common is not supported on Linux; Spire's PDF path triggers it.
                // Use LibreOffice headless for cross-platform conversion.
                var outputDir = Path.GetDirectoryName(tempOutput)!;
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "soffice",
                        ArgumentList =
                        {
                            "--headless",
                            "--nologo",
                            "--nofirststartwizard",
                            "--convert-to",
                            "pdf",
                            "--outdir",
                            outputDir,
                            tempInput
                        },
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                try
                {
                    process.Start();
                }
                catch (Exception startEx)
                {
                    return StatusCode(501, new
                    {
                        error = "LibreOffice (soffice) is required for DOCX->PDF on Linux.",
                        details = environment.IsDevelopment() ? startEx.ToString() : startEx.Message,
                        hint = "Install with: sudo apt update && sudo apt install -y libreoffice"
                    });
                }

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
                await process.WaitForExitAsync(timeoutCts.Token);

                var stdout = await process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
                var stderr = await process.StandardError.ReadToEndAsync(timeoutCts.Token);

                if (process.ExitCode != 0)
                {
                    return StatusCode(500, new
                    {
                        error = "LibreOffice conversion failed.",
                        exitCode = process.ExitCode,
                        stdout,
                        stderr
                    });
                }

                // LibreOffice names output based on input file name in the output directory.
                var expectedOut = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(tempInput) + ".pdf");
                if (!System.IO.File.Exists(expectedOut))
                {
                    return StatusCode(500, new
                    {
                        error = "LibreOffice reported success but PDF was not found.",
                        stdout,
                        stderr
                    });
                }

                System.IO.File.Move(expectedOut, tempOutput, overwrite: true);
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
