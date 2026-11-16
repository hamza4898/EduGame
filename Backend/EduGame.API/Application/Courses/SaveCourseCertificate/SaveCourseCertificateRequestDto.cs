namespace EduGame.Application.Courses.SaveCourseCertificate;

public sealed record SaveCourseCertificateRequestDto(
    bool Enabled, 
    string Signer);