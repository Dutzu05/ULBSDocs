using Microsoft.AspNetCore.Hosting;
using Moq;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services;
using Xunit;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UlbsDocAuth.Api.Tests.Services;

public class WordTemplateServiceTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    [Fact]
    public void GenerateDocx_TemplateNotFound_ThrowsFileNotFoundException()
    {
        _envMock.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        var service = new WordTemplateService(_envMock.Object);
        var studentData = new CertificateResponse("test@email.com", "Nume", "Fac", "Prog", 1, "Gr", "S", DateOnly.FromDateTime(DateTime.Now));

        Assert.Throws<FileNotFoundException>(() => service.GenerateDocx(studentData, "Motiv"));
    }

    [Fact]
    public void GenerateDocx_WithValidTemplate_ReplacesPlaceholdersAndReturnsPath()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var templateDir = Path.Combine(tempRoot, "Templates");
        Directory.CreateDirectory(templateDir);
        var templatePath = Path.Combine(templateDir, "Adeverinta_2_Completat.docx");

        using (var wordDoc = WordprocessingDocument.Create(templatePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document(
                new Body(
                    new Paragraph(
                        new Run(
                            new Text("Salut {{NumeComplet}}!")))));
            mainPart.Document.Save();
        }

        _envMock.Setup(e => e.ContentRootPath).Returns(tempRoot);
        var service = new WordTemplateService(_envMock.Object);
        var studentData = new CertificateResponse("test@ulbs.ro", "Popescu Ion", "Inginerie", "Calculatoare", 1, "311", "SN123", DateOnly.FromDateTime(DateTime.Now));

        var resultPath = service.GenerateDocx(studentData, "Motiv test");

        Assert.True(File.Exists(resultPath));
        Assert.Contains("Adeverinta_", resultPath);
        
        if (File.Exists(resultPath)) File.Delete(resultPath);
        Directory.Delete(tempRoot, true);
    }

    [Fact]
    public void GenerateDocx_EmptyBody_ThrowsInvalidOperationException()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var templateDir = Path.Combine(tempRoot, "Templates");
        Directory.CreateDirectory(templateDir);
        var templatePath = Path.Combine(templateDir, "Adeverinta_2_Completat.docx");

        using (var wordDoc = WordprocessingDocument.Create(templatePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            wordDoc.AddMainDocumentPart();
        }

        _envMock.Setup(e => e.ContentRootPath).Returns(tempRoot);
        var service = new WordTemplateService(_envMock.Object);
        var studentData = new CertificateResponse("test@ulbs.ro", "Ion", "Fac", "Prog", 1, "Gr", "S", DateOnly.FromDateTime(DateTime.Now));

        Assert.Throws<InvalidOperationException>(() => service.GenerateDocx(studentData, "test"));

        Directory.Delete(tempRoot, true);
    }

    [Fact]
    public void GenerateDocx_WithNullValuesAndEmptyText_HandlesGracefully()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var templateDir = Path.Combine(tempRoot, "Templates");
        Directory.CreateDirectory(templateDir);
        var templatePath = Path.Combine(templateDir, "Adeverinta_2_Completat.docx");

        using (var wordDoc = WordprocessingDocument.Create(templatePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body(
                new Paragraph(
                    new Run(new Text("")), 
                    new Run(new Text("Nume: {{NumeComplet}}"))
                )
            ));
            mainPart.Document.Save();
        }

        _envMock.Setup(e => e.ContentRootPath).Returns(tempRoot);
        var service = new WordTemplateService(_envMock.Object);
        
        var studentData = new CertificateResponse("test@ulbs.ro", null!, null!, null!, 1, null!, null!, DateOnly.FromDateTime(DateTime.Now));

        var resultPath = service.GenerateDocx(studentData, null!);

        Assert.True(File.Exists(resultPath));
        
        if (File.Exists(resultPath)) File.Delete(resultPath);
        Directory.Delete(tempRoot, true);
    }
}