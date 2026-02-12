using Microsoft.AspNetCore.Mvc;
using Moq;
using UlbsDocAuth.Api.Controllers;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;
using Xunit;

namespace UlbsDocAuth.Api.Tests;

public class CertificatesControllerTests
{
    [Fact]
    public void GetMockByEmail_InvalidEmail_ReturnsBadRequest()
    {
        var service = new Mock<ICertificateDataService>(MockBehavior.Strict);
        var controller = new CertificatesController(service.Object);

        var result = controller.GetMockByEmail("not-an-email");
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetMockByEmail_NotFound_Returns404()
    {
        var service = new Mock<ICertificateDataService>(MockBehavior.Strict);
        service.Setup(s => s.GetByEmail("x@y.com")).Returns((CertificateResponse?)null);

        var controller = new CertificatesController(service.Object);
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

        var service = new Mock<ICertificateDataService>(MockBehavior.Strict);
        service.Setup(s => s.GetByEmail("x@y.com")).Returns(response);

        var controller = new CertificatesController(service.Object);
        var result = controller.GetMockByEmail("x@y.com");

        Assert.IsType<OkObjectResult>(result);
    }
}
