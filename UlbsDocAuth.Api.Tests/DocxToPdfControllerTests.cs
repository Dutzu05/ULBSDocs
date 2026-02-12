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

        // null IFormFile isn't possible at runtime, but we can simulate an empty file
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
}
