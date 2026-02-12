using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using UlbsDocAuth.Api.Controllers;
using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;
using Xunit;

namespace UlbsDocAuth.Api.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task GoogleLogin_MissingToken_ReturnsBadRequest()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var validator = new Mock<IGoogleIdTokenValidator>(MockBehavior.Strict);

        var controller = new AuthController(config, validator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GoogleLogin(new GoogleLoginRequest(""));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GoogleLogin_MissingClientId_Returns500()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var validator = new Mock<IGoogleIdTokenValidator>(MockBehavior.Strict);

        var controller = new AuthController(config, validator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GoogleLogin(new GoogleLoginRequest("token"));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_UnverifiedEmail_Returns403()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["GoogleAuth:ClientId"] = "client" })
            .Build();

        var validator = new Mock<IGoogleIdTokenValidator>(MockBehavior.Strict);
        validator
            .Setup(v => v.ValidateAsync("token", "client", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleIdTokenPayload("a@b.com", "Name", "sub", EmailVerified: false));

        var controller = new AuthController(config, validator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GoogleLogin(new GoogleLoginRequest("token"));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, obj.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_ValidToken_ReturnsOkWithEmail()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["GoogleAuth:ClientId"] = "client" })
            .Build();

        var validator = new Mock<IGoogleIdTokenValidator>(MockBehavior.Strict);
        validator
            .Setup(v => v.ValidateAsync("token", "client", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleIdTokenPayload("a@b.com", "Name", "sub", EmailVerified: true));

        var controller = new AuthController(config, validator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GoogleLogin(new GoogleLoginRequest("token"));
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
