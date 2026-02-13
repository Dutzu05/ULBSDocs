using Microsoft.AspNetCore.Mvc;
using Moq;
using UlbsDocAuth.Api.Controllers;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;
using Xunit;
using UlbsDocAuth.Api.Services;           

namespace UlbsDocAuth.Api.Tests;
public class CertificatesControllerTests
{
    private readonly Mock<ICertificateDataService> _dataServiceMock = new();
    private readonly Mock<IWordTemplateService> _templateServiceMock = new();
    private readonly Mock<IDocxToPdfConverter> _pdfConverterMock = new();

    
    private CertificatesController CreateController() => 
        new (_dataServiceMock.Object, _templateServiceMock.Object, _pdfConverterMock.Object);

    [Fact]
    public void GetMockByEmail_InvalidEmail_ReturnsBadRequest()
    {
        var controller = CreateController();
        var result = controller.GetMockByEmail("");
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetMockByEmail_NotFound_Returns404()
    {
        
        _dataServiceMock.Setup(s => s.GetByEmail("x@y.com")).Returns((CertificateResponse?)null);

        
        var controller = CreateController();
        var result = controller.GetMockByEmail("x@y.com");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public void GetMockByEmail_Found_ReturnsOk()
    {
        var response = new CertificateResponse(
            Email: "x@y.com",
            FullName: "X",
            Faculty: "F",
            Program: "P",
            StudyYear: 1,
            Group: "G",
            Serial: "S",
            IssuedAt: new DateOnly(2026, 2, 12));

        _dataServiceMock.Setup(s => s.GetByEmail("x@y.com")).Returns(response);

        var controller = CreateController();
        var result = controller.GetMockByEmail("x@y.com");

        Assert.IsType<OkObjectResult>(result);
    }
    [Fact]
    public async Task GenerateCertificate_InvalidEmail_ReturnsBadRequest()
    {
        var request = new GenerateRequest("", "Motiv");
        var controller = CreateController();

        var result = await controller.GenerateCertificate(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
    [Fact]
    public async Task GenerateCertificate_StudentNotFound_ReturnsNotFound()
    {
        var request = new GenerateRequest("inexistent@ulbs.ro", "Motiv");
        _dataServiceMock.Setup(s => s.GetByEmail(request.Email)).Returns((CertificateResponse?)null);
        var controller = CreateController();

        var result = await controller.GenerateCertificate(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }
   [Fact]
    public async Task GenerateCertificate_Success_ReturnsFile()
    {
        var request = new GenerateRequest("valid@ulbs.ro", "Bursa");
        var student = new CertificateResponse(request.Email, "Ion", "F", "P", 1, "G", "S", DateOnly.FromDateTime(DateTime.Now));
        
        var tempDocx = Path.GetTempFileName();
        var tempPdf = Path.ChangeExtension(tempDocx, ".pdf");
        await System.IO.File.WriteAllBytesAsync(tempPdf, new byte[] { 1, 2, 3 });

        try 
        {
            _dataServiceMock.Setup(s => s.GetByEmail(request.Email)).Returns(student);
            _templateServiceMock.Setup(s => s.GenerateDocx(student, request.Reason)).Returns(tempDocx);
            _pdfConverterMock.Setup(c => c.ConvertAsync(tempDocx, tempPdf, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);

            var controller = CreateController();

            var result = await controller.GenerateCertificate(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }
        finally
        {
            if (System.IO.File.Exists(tempDocx)) System.IO.File.Delete(tempDocx);
            if (System.IO.File.Exists(tempPdf)) System.IO.File.Delete(tempPdf);
        }
    }
}