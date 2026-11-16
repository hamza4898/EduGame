using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.SaveCourseCertificate;

public sealed record SaveCourseCertificateCommand(string UserId, int CourseId, SaveCourseCertificateRequestDto Request) : IRequest<Result<bool>>;