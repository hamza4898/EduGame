using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyTeacherXpAwards;

public class GetMyTeacherXpAwardsQueryHandler(
    IUserRepository teacherRepository)
    : IRequestHandler<GetMyTeacherXpAwardsQuery, Result<List<TeacherXpAwardDto>>>
{
    public async Task<Result<List<TeacherXpAwardDto>>> Handle(GetMyTeacherXpAwardsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (await teacherRepository.GetTeacherById(request.TeacherId) is null) return Result.Forbidden("Достижения доступны только преподавателю.");

        var awards = await teacherRepository.GetTopTeacherXpAwardsAsync(request.TeacherId, 20, cancellationToken);

        return Result.Success(awards);
    }
}