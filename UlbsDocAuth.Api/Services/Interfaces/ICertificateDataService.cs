using UlbsDocAuth.Api.DTOs;

namespace UlbsDocAuth.Api.Services.Interfaces;

public interface ICertificateDataService
{
    CertificateResponse? GetByEmail(string email);
}
