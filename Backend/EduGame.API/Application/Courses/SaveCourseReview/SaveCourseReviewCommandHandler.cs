using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.SaveCourseReview;

internal sealed class SaveCourseReviewCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<SaveCourseReviewCommand, Result<CourseReviewResponseDto>>
{
    public async Task<Result<CourseReviewResponseDto>> Handle(SaveCourseReviewCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) 
            return Result.Unauthorized("Не удалось определить пользователя.");
            
        if (request.CourseId <= 0) 
            return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса невалиден."));
            
        if (request.Request is null)
            return Result.Invalid(new ValidationError("Request", "Данные запроса пусты."));

        if (request.Request.Stars is < 1 or > 5)
            return Result.Invalid(new ValidationError("Stars", "Оценка должна быть в диапазоне от 1 до 5 звезд."));

        if (request.Request.Text?.Trim().Length > 1000)
            return Result.Invalid(new ValidationError("Text", "Текст отзыва не должен превышать 1000 символов."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists) 
            return Result.NotFound($"Курс с ID {request.CourseId} не найден.");

        var result = await service.SaveReviewAsync(request.UserId, request.CourseId, request.Request, cancellationToken);
        
        return Result.Success(result);
    }
}