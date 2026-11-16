using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.GenerateCourseCertificate;

public sealed record GenerateCourseCertificateCommand(string UserId, int CourseId) : IRequest<Result<byte[]>>;