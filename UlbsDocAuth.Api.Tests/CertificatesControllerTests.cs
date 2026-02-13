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
}