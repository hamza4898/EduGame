using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Application.Courses.CompleteLesson;

public class CompleteLessonCommandHandler(
    IUserRepository userRepository,
    IStudentCourseRepository studentCourseRepository,
    ISkillTreeRepository skillTreeRepository,
    ICourseRepository courseRepository,
    IGamificationService gamificationService,
    IQuizValidationService quizValidationService,
    ITeacherProgressionService teacherProgressionService) : IRequestHandler<CompleteLessonCommand, Result<LessonRewardDto>>
{
    public async Task<Result<LessonRewardDto>> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var student = await userRepository.GetStudentById(request.UserId);
        if (student == null) return Result.NotFound("Игровой профиль студента не найден.");

        var lesson = await studentCourseRepository.GetLessonWithCourseAsync(request.LessonId);
        if (lesson == null) return Result.NotFound("Урок не найден в текущем рейде.");

        var studentCourse = await studentCourseRepository.GetStudentCourseAsync(request.UserId, lesson.Section.CourseId);
        if (studentCourse is null) return Result.Forbidden("Запишитесь на курс, прежде чем завершать уроки.");

        var courseLessonIds = await studentCourseRepository.GetCourseLessonIdsAsync(lesson.Section.CourseId);
        var completedLessonIds = await studentCourseRepository.GetCompletedLessonIdsAsync(request.UserId, lesson.Section.CourseId);
        
        var lessonIndex = courseLessonIds.IndexOf(lesson.Id);
        if (lessonIndex < 0 || courseLessonIds.Take(lessonIndex).Any(id => !completedLessonIds.Contains(id)))
            return Result.Forbidden("Сначала завершите предыдущие уроки курса.");

        var quiz = lesson.Blocks.FirstOrDefault(block => block.Type == BlockType.Quiz);
        if (quiz is not null)
        {
            var quizValidationResult = quizValidationService.ValidateQuizAnswer(quiz.Content, request.SelectedAnswer);
            if (!quizValidationResult.IsSuccess) return Result.Invalid(quizValidationResult.ValidationErrors);
        }

        var isAlreadyDone = await studentCourseRepository.IsLessonAlreadyCompletedAsync(request.UserId, request.LessonId);
        if (isAlreadyDone) 
        {
            if (studentCourse.IsCompleted)
                await teacherProgressionService.AwardCourseCompletionAsync(
                    lesson.Section.CourseId, request.UserId, cancellationToken);
            return Result.Invalid(new ValidationError("Этот урок уже пройден. Опыт и коины начислены ранее."));
        }

        var (xpGained, coinsGained) = gamificationService.CalculateLessonRewards(lesson);
        student.GlobalXp += xpGained;
        student.CoinsBalance += coinsGained;
        
        await skillTreeRepository.AwardSubSectionXpAsync(
            student.Id,
            lesson.Section.Course.SkillSubSectionId,
            xpGained);

        studentCourseRepository.CompleteLesson(new StudentLesson
        {
            StudentId = student.Id,
            LessonId = lesson.Id,
            CompletedAt = DateTime.UtcNow,
            IsCompleted = true
        });

        var totalLessons = await studentCourseRepository.GetTotalLessonsInCourseAsync(lesson.Section.CourseId);
        var completedLessons = completedLessonIds.Count + 1; 
        
        var newProgress = gamificationService.CalculateProgressPercent(totalLessons, completedLessons);
        studentCourse.ProgressPercent = newProgress;
        
        if (newProgress >= 100) 
            studentCourse.IsCompleted = true;

        await courseRepository.Save(); 

        if (newProgress >= 100)
            await teacherProgressionService.AwardCourseCompletionAsync(
                lesson.Section.CourseId, request.UserId, cancellationToken);

        return Result.Success(new LessonRewardDto(xpGained, coinsGained, newProgress));
    }
}