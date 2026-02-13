using Xunit;
using UlbsDocAuth.Api.DTOs; 
using UlbsDocAuth.Api.Controllers;

public class DtoTests
{
    [Fact]
    public void Dto_EqualityChecks_CoverHiddenBranches()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        
        var dto1 = new CertificateResponse("email", "Nume", "Fac", "Prog", 1, "Gr", "S", date);
        var dto2 = new CertificateResponse("email", "Nume", "Fac", "Prog", 1, "Gr", "S", date);
        
        
        var dto3 = new CertificateResponse("ALT_EMAIL", "Nume", "Fac", "Prog", 1, "Gr", "S", date);

        
        Assert.Equal(dto1, dto2);
        
        Assert.NotEqual(dto1, dto3);
        
        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
        
        Assert.NotNull(dto1.ToString());

        var req1 = new GenerateRequest("a@b.com", "Motiv");
        var req2 = new GenerateRequest("a@b.com", "Motiv");
        var req3 = new GenerateRequest("x@y.com", "Altceva");

        Assert.Equal(req1, req2);
        Assert.NotEqual(req1, req3);
        Assert.Equal(req1.GetHashCode(), req2.GetHashCode());
    }
}