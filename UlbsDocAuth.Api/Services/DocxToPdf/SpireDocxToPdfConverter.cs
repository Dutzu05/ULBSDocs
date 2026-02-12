using Spire.Doc;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Services.DocxToPdf;

public class SpireDocxToPdfConverter : IDocxToPdfConverter
{
    public Task ConvertAsync(string inputDocxPath, string outputPdfPath, CancellationToken cancellationToken)
    {
        var document = new Document();
        document.LoadFromFile(inputDocxPath);
        document.SaveToFile(outputPdfPath, FileFormat.PDF);
        return Task.CompletedTask;
    }
}
