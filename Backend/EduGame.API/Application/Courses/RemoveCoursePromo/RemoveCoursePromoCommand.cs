using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.RemoveCoursePromo;

public sealed record RemoveCoursePromoCommand(string UserId, int CourseId, int PromoId) : IRequest<Result<bool>>; 