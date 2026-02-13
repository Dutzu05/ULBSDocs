using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UlbsDocAuth.Api.Controllers;
using UlbsDocAuth.Api.Services.Interfaces;
using Xunit;

namespace UlbsDocAuth.Api.Tests;

public class DocxToPdfControllerTests
{
    [Fact]
    public async Task Convert_MissingFile_ReturnsBadRequest()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Development");

        var converter = new Mock<IDocxToPdfConverter>(MockBehavior.Strict);
        var controller = new DocxToPdfController(env.Object, converter.Object);

        var file = new FormFile(Stream.Null, 0, 0, "file", "a.docx");
        var result = await controller.Convert(file, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Convert_WrongExtension_ReturnsBadRequest()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Development");

        var converter = new Mock<IDocxToPdfConverter>(MockBehavior.Strict);
        var controller = new DocxToPdfController(env.Object, converter.Object);

        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", "a.txt");
        var result = await controller.Convert(file, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Convert_ValidDocx_CallsConverter()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Development");

        var converter = new Mock<IDocxToPdfConverter>(MockBehavior.Strict);
        converter
            .Setup(c => c.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>((_, outPath, __) =>
            {
                File.WriteAllBytes(outPath, "%PDF-1.7\n"u8.ToArray());
                return Task.CompletedTask;
            });

        var controller = new DocxToPdfController(env.Object, converter.Object);

        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", "a.docx");
        var result = await controller.Convert(file, CancellationToken.None);

        Assert.IsType<FileContentResult>(result);
        converter.VerifyAll();
    }

    [Fact]
    public async Task Convert_FileTooLarge_ReturnsBadRequest()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        var converter = new Mock<IDocxToPdfConverter>();
        var controller = new DocxToPdfController(env.Object, converter.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(26 * 1024 * 1024);
        fileMock.Setup(f => f.FileName).Returns("large.docx");

        var result = await controller.Convert(fileMock.Object, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        // Aici era warning-ul, acum e rezolvat cu '?'
        Assert.Contains("File too large", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Convert_Win32Exception_InProduction_Returns501_WithSimpleMessage()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Production");

        var converter = new Mock<IDocxToPdfConverter>();
        converter.Setup(c => c.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.ComponentModel.Win32Exception("Access denied or missing file"));

        var controller = new DocxToPdfController(env.Object, converter.Object);

        var file = new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "file", "test.docx");

        var result = await controller.Convert(file, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(501, objectResult.StatusCode);

        var valStr = objectResult.Value?.ToString();
        Assert.Contains("DOCX->PDF converter dependency is missing", valStr);
    }

    [Fact]
    public async Task Convert_GeneralException_InProduction_Returns500_WithSimpleMessage()
    {
        var env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Production");

        var converter = new Mock<IDocxToPdfConverter>();
        converter.Setup(c => c.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Something exploded internally"));

        var controller = new DocxToPdfController(env.Object, converter.Object);

        var file = new FormFile(new MemoryStream(new byte[] { 1 }), 0, 1, "file", "test.docx");

        var result = await controller.Convert(file, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        
        Assert.NotNull(objectResult.Value); 

        var props = objectResult.Value.GetType().GetProperties();
        var detailsProp = props.FirstOrDefault(p => p.Name == "details");
        var detailsValue = detailsProp?.GetValue(objectResult.Value)?.ToString();

        Assert.Equal("Something exploded internally", detailsValue);
    }
}