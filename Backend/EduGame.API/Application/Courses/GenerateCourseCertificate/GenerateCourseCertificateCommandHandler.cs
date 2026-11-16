using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GenerateCourseCertificate;

public sealed class GenerateCourseCertificateCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<GenerateCourseCertificateCommand, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GenerateCourseCertificateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var pdfBytes = await service.GenerateCertificateAsync(request.UserId, request.CourseId, cancellationToken);
        if (pdfBytes == null || pdfBytes.Length == 0) return Result.Error("Не удалось сформировать файл сертификата.");

        return Result.Success(pdfBytes);
    }
}