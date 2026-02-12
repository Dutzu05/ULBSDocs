namespace UlbsDocAuth.Api.Services.Interfaces;

public interface IDocxToPdfConverter
{
    Task ConvertAsync(string inputDocxPath, string outputPdfPath, CancellationToken cancellationToken);
}
