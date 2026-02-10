using UlbsDocAuth.Api.DTOs;
using UlbsDocAuth.Api.Services.Interfaces;

namespace UlbsDocAuth.Api.Services.Mock;

public class MockCertificateDataService : ICertificateDataService
{
    private static readonly List<CertificateResponse> Data =
    [
        new(
            Email: "ana.popescu@student.ulbs.ro",
            FullName: "Ana Popescu",
            Faculty: "Inginerie",
            Program: "CTI",
            StudyYear: 3,
            Group: "334B",
            Serial: "MOCK-001",
            IssuedAt: DateOnly.FromDateTime(DateTime.UtcNow)
        ),
        new(
            Email: "mirona.botanel@student.ulbs.ro",
            FullName: "Mirona Botanel",
            Faculty: "Stiinte",
            Program: "Informatica",
            StudyYear: 1,
            Group: "211",
            Serial: "MOCK-002",
            IssuedAt: DateOnly.FromDateTime(DateTime.UtcNow)
        ),
        new(
            Email: "botanelmyrona@gmail.com",
            FullName: "Myrona Botanel",
            Faculty: "Calculatoare",
            Program: "Tehnologia Informatiei",
            StudyYear: 4,
            Group: "3",
            Serial: "MOCK-003",
            IssuedAt: DateOnly.FromDateTime(DateTime.UtcNow)
            )

    ];

    public CertificateResponse? GetByEmail(string email)
    {
        return Data.FirstOrDefault(x =>
            x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
