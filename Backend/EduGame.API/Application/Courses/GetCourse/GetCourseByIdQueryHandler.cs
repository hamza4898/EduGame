using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using AutoMapper;
using System.Text.Json;
using System.Text.Json.Nodes;
using EduGame.Domain.Enums;

namespace EduGame.Application.Courses.GetCourse;

public class GetCourseByIdQueryHandler(
    ICourseRepository courseRepository,
    IStudentCourseRepository studentCourseRepository,
    IMapper mapper
) : IRequestHandler<GetCourseByIdQuery, Result<CourseDetailDto>>
{
    public async Task<Result<CourseDetailDto>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        CourseDetailDto? courseDto = await courseRepository.GetCourseDetailAsync(request.CourseId, request.StudentId, mapper.ConfigurationProvider);

        if (courseDto is null) return Result.NotFound("Запрашиваемый курс не найден.");

        courseDto.Sections = [.. courseDto.Sections.OrderBy(s => s.Order)];
        
        var isEnrolled = await studentCourseRepository.IsAlreadyEnrolledAsync(request.StudentId, request.CourseId);
        
        var completedLessonIds = isEnrolled
            ? await studentCourseRepository.GetCompletedLessonIdsAsync(request.StudentId, request.CourseId)
            : [];
            
        var hasUnlockedLesson = false;
        
        foreach (var section in courseDto.Sections)
        {
            section.Lessons = [.. section.Lessons.OrderBy(l => l.Order)];
            foreach (var lesson in section.Lessons)
            {
                lesson.Blocks = [.. lesson.Blocks.OrderBy(b => b.Order)];
                foreach (var block in lesson.Blocks.Where(block => block.Type == (int)BlockType.Quiz))
                {
                    try
                    {
                        var quiz = JsonNode.Parse(block.Content)?.AsObject();
                        quiz?.Remove("answer");
                        if (quiz is not null) block.Content = quiz.ToJsonString();
                    }
                    catch (JsonException) {}
                }
                
                var isCompleted = completedLessonIds.Contains(lesson.Id);
                lesson.Status = isCompleted ? "completed" : isEnrolled && !hasUnlockedLesson ? "active" : "locked";
                if (!isCompleted) hasUnlockedLesson = true;
            }
        }   

        return Result.Success(courseDto);
    }
}