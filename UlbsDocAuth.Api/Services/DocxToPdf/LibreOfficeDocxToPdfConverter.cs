using System.Diagnostics;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Services.DocxToPdf;

public class LibreOfficeDocxToPdfConverter : IDocxToPdfConverter
{
    public async Task ConvertAsync(string inputDocxPath, string outputPdfPath, CancellationToken cancellationToken)
    {
        var outputDir = Path.GetDirectoryName(outputPdfPath) ?? throw new InvalidOperationException("Invalid output path.");

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
                    inputDocxPath
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

        await process.WaitForExitAsync(timeoutCts.Token);

        var stdout = await process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
        var stderr = await process.StandardError.ReadToEndAsync(timeoutCts.Token);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"LibreOffice conversion failed (exit {process.ExitCode}).\nstdout: {stdout}\nstderr: {stderr}");
        }

        var expectedOut = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputDocxPath) + ".pdf");
        if (!File.Exists(expectedOut))
        {
            throw new FileNotFoundException($"LibreOffice reported success but PDF was not found at {expectedOut}.\nstdout: {stdout}\nstderr: {stderr}");
        }

        File.Move(expectedOut, outputPdfPath, overwrite: true);
    }
}
