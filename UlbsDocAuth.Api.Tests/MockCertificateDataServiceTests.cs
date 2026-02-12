using UlbsDocAuth.Api.Services.Mock;
using Xunit;

namespace UlbsDocAuth.Api.Tests;

public class MockCertificateDataServiceTests
{
    [Fact]
    public void GetByEmail_IsCaseInsensitive()
    {
        var service = new MockCertificateDataService();
        var result = service.GetByEmail("ANA.POPESCU@STUDENT.ULBS.RO");
        Assert.NotNull(result);
    }
}
