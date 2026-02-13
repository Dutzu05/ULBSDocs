using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace UlbsDocAuth.Api.Tests;

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_Loads()
    {
        var res = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task CertificatesMock_KnownEmail_ReturnsOk()
    {
        var res = await _client.GetAsync("/api/certificates/mock?email=ana.popescu@student.ulbs.ro");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task AuthGoogle_Valid_ReturnsOk()
    {
        var res = await _client.PostAsJsonAsync("/auth/google", new { idToken = "ok" });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task AuthGoogle_Invalid_ReturnsUnauthorized()
    {
        var res = await _client.PostAsJsonAsync("/auth/google", new { idToken = "invalid" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AuthGoogle_Unverified_ReturnsForbidden()
    {
        var res = await _client.PostAsJsonAsync("/auth/google", new { idToken = "unverified" });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    //[Fact]
    // public async Task DocDocx_Convert_ReturnsDocx()
    // {
    //     var res = await _client.GetAsync("/api/doc-docx/convert");
    //     Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    //     Assert.Equal(
    //         "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    //         res.Content.Headers.ContentType?.MediaType);
    // }

    [Fact]
    public async Task DocxToPdf_Convert_ReturnsPdf()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "a.docx");

        var res = await _client.PostAsync("/api/docx-to-pdf/convert", content);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/pdf", res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DocDocx_ConvertUpload_ValidFile_ReturnsDocx()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "test.doc");

        var res = await _client.PostAsync("/api/doc-docx/convert", content);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DocDocx_ConvertUpload_InvalidExtension_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "test.txt");

        var res = await _client.PostAsync("/api/doc-docx/convert", content);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task DocDocx_ConvertUpload_NoFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();

        var res = await _client.PostAsync("/api/doc-docx/convert", content);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
    [Fact]
    public async Task DocDocx_Convert_Get_ReturnsDocx()
    {
        var res = await _client.GetAsync("/api/doc-docx/convert");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DocDocx_ConvertUpload_EmptyFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "empty.doc");

        var res = await _client.PostAsync("/api/doc-docx/convert", content);
        
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task DocDocx_ConvertUpload_TooLargeFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        var largeData = new byte[(25 * 1024 * 1024) + 1];
        content.Add(new ByteArrayContent(largeData), "file", "large.doc");

        var res = await _client.PostAsync("/api/doc-docx/convert", content);
        
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
