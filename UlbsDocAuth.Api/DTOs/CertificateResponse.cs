namespace UlbsDocAuth.Api.DTOs;

public record CertificateResponse(
    string Email,
    string FullName,
    string Faculty,
    string Program,
    int StudyYear,
    string Group,
    string Serial,
    DateOnly IssuedAt
);
