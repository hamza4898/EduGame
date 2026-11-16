using MediatR;
using AutoMapper;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetStudentsRating;

public class GetStudentRatingsQueryHandler(
    IRatingRepository ratingRepository,
    IMapper mapper,
    ILevelService levelService
) : IRequestHandler<GetStudentRatingsQuery, Result<List<StudentRatingResponseDto>>>
{
    public async Task<Result<List<StudentRatingResponseDto>>> Handle(GetStudentRatingsQuery request, CancellationToken cancellationToken)
    {
        var topStudents = await ratingRepository.GetTopStudentsRawAsync(mapper.ConfigurationProvider, cancellationToken);

        if (topStudents == null) return Result.NotFound("Не удалось сформировать рейтинг студентов.");

        foreach (var student in topStudents)
        {
            var progress = levelService.CalculateProgress(student.GlobalXp, "Student");
            student.Level = progress.Level; 
        }

        return Result.Success(topStudents);
    }
}