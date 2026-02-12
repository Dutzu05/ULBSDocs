using UlbsDocAuth.E2E;

namespace UlbsDocAuth.E2E;

public class ApiE2ETests
{
    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("E2E_BASE_URL")?.TrimEnd('/')
        ?? "http://localhost:3000";

    private static HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task SwaggerJson_Loads()
    {
        using var client = CreateClient();
        var res = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task CertificatesMock_KnownEmail_ReturnsOk()
    {
        using var client = CreateClient();
        var res = await client.GetAsync("/api/certificates/mock?email=ana.popescu@student.ulbs.ro");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task AuthGoogle_MissingToken_ReturnsBadRequest()
    {
        using var client = CreateClient();
        var res = await client.PostAsJsonAsync("/auth/google", new { idToken = "" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task DocDocx_Convert_ReturnsDocx()
    {
        using var client = CreateClient();
        var res = await client.GetAsync("/api/doc-docx/convert");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task DocxToPdf_Convert_ReturnsPdf()
    {
        using var client = CreateClient();

        var docxBytes = MinimalDocx.Create("E2E test");
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(docxBytes), "file", "test.docx");

        var res = await client.PostAsync("/api/docx-to-pdf/convert", form);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/pdf", res.Content.Headers.ContentType?.MediaType);

        var bytes = await res.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 100);
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }
}
