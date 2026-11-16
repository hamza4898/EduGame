using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EduGame.Application.Courses.GetMyTeacherCourses;
using EduGame.Application.Courses.GetMyLikedTeacherCourses;
using EduGame.Application.Courses.GetMyArchivedTeacherCourses;
using EduGame.Application.Courses.GetMyTeacherCourseStudentCounts;
using EduGame.Application.Courses.GetMyTeacherCourseRatings;
using EduGame.Application.Courses.GetMyTeacherCourseProgress;
using EduGame.Application.Courses.GetMyTeacherCourseCompletion;
using EduGame.Application.Courses.GetMyTeacherCourseStudents;
using EduGame.Application.Courses.GetMyTeacherXpAwards;
using EduGame.Application.Courses.CreateCourseWithCommunity;
using EduGame.Application.Courses.ChangeCourseDescription;
using EduGame.Application.Courses.UpdateCourseContent;
using EduGame.Application.Courses.GetStudentCourses;
using EduGame.Application.Courses.ToggleLikedCourse;
using EduGame.Application.Courses.ToggleArchivedCourse;
using EduGame.Application.Courses.GetCourseDescription;
using EduGame.Application.Courses.GetCourse;
using EduGame.Application.Courses.PurchaseCourse;
using EduGame.Application.Courses.CompleteLesson;
using EduGame.Application.Courses.GetCourseReadiness;
using EduGame.Application.Courses.PublishCourse;
using EduGame.Application.Courses.UnpublishCourse;
using EduGame.Application.Courses.SaveCoursePrice;
using EduGame.Application.Courses.SaveCourseCertificate;
using EduGame.Application.Courses.GetCourseNews;
using EduGame.Application.Courses.GetCourseReviews;
using EduGame.Application.Courses.SaveCourseReview;
using EduGame.Application.Courses.GetCoursePromos;
using EduGame.Application.Courses.GenerateCourseCertificate;
using EduGame.Application.Courses.RemoveCourseTeacher;
using EduGame.Application.Courses.InviteCourseTeacher;
using EduGame.Application.Courses.GetCourseTeachers;
using EduGame.Application.Courses.RemoveCoursePromo;
using EduGame.Application.Courses.AddCoursePromo;
using EduGame.Application.Courses.PublishCourseNews;
using EduGame.Application.Courses.GetCatalogCourses;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Courses.GetCourseCreationOptions;

namespace EduGame.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/courses/[action]")]
public class CoursesController(
    IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ActionName("GetCourseCreationOptions")]
    public async Task<IActionResult> GetCourseCreationOptions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCourseCreationOptionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourses")]
    public async Task<IActionResult> GetMyTeacherCourses(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCoursesQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyLikedTeacherCourses")]
    public async Task<IActionResult> GetMyLikedTeacherCourses(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyLikedTeacherCoursesQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyArchivedTeacherCourses")]
    public async Task<IActionResult> GetMyArchivedTeacherCourses(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyArchivedTeacherCoursesQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourseStudentCounts")]
    public async Task<IActionResult> GetMyTeacherCourseStudentCounts(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCourseStudentCountsQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourseRatings")]
    public async Task<IActionResult> GetMyTeacherCourseRatings(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCourseRatingsQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourseProgress")]
    public async Task<IActionResult> GetMyTeacherCourseProgress(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCourseProgressQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourseCompletionCounts")]
    public async Task<IActionResult> GetMyTeacherCourseCompletionCounts(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCourseCompletionCountsQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherCourseStudents")]
    public async Task<IActionResult> GetMyTeacherCourseStudents(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherCourseStudentsQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyTeacherXpAwards")]
    public async Task<IActionResult> GetMyTeacherXpAwards(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMyTeacherXpAwardsQuery(CurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("CreateCourse")]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseWithCommunityDto dto,
        CancellationToken cancellationToken)
    {
        var teacherId = CurrentUserId();
        var teacherName = User.Identity?.Name ?? string.Empty;

        var result = await mediator.Send(new CreateCourseWithCommunityCommand(
            dto,
            teacherId,
            teacherName), cancellationToken);

        return Ok(result);
    }

    [HttpPut]
    [ActionName("ChangeCourseDescription")]
    public async Task<IActionResult> ChangeCourseDescription(
        [FromQuery] int courseId,
        [FromBody] ChangeCourseDescriptionDto dto,
        CancellationToken cancellationToken)
    {
        var teacherId = CurrentUserId();

        var result = await mediator.Send(new ChangeCourseDescriptionCommand(
            teacherId,
            courseId,
            dto.CourseName,
            dto.CourseDescription,
            dto.SkillSubSectionId), cancellationToken);

        return Ok(result);
    }

    [HttpPut]
    [ActionName("UpdateCourseContent")]
    public async Task<IActionResult> UpdateCourseContent(
        [FromQuery] int courseId,
        [FromBody] UpdateCourseContentDto dto,
        CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? string.Empty;

        var result = await mediator.Send(
            new UpdateCourseContentCommand(teacherId, courseId, dto),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetMyStudentCourses")]
    public async Task<IActionResult> GetMyStudentCourses(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new GetStudentCoursesQuery(userId), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ActionName("ToggleLiked")]
    public async Task<IActionResult> ToggleLikedCourse(
        [FromQuery] int courseId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new ToggleLikedCourseCommand(userId, courseId), cancellationToken);

        return Ok(result);      
    }

    [HttpPost]
    [ActionName("ToggleArchived")]
    public async Task<IActionResult> ToggleArchivedCourse(
        [FromQuery] int courseId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new ToggleArchivedCourseCommand(userId, courseId), cancellationToken);

        return Ok(result);      
    }

    [HttpGet]
    [ActionName("GetCatalogCourses")]
    public async Task<IActionResult> GetCatalogCourses(
        [FromQuery] GetCatalogCoursesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query with { UserId = CurrentUserId() }, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetCourseDescription")]
    public async Task<IActionResult> GetCourseDescription(int courseId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();

        var result = await mediator.Send(new GetCourseDescriptionQuery(userId, courseId), cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [ActionName("GetCourse")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCourseByIdQuery(id, CurrentUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("StartCourse")]
    public async Task<IActionResult> StartCourse(
        [FromBody] StartCourseRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PurchaseCourseCommand(CurrentUserId(), request), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("BuyCourse")]
    public async Task<IActionResult> BuyCourse(
        [FromBody] StartCourseRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PurchaseCourseCommand(CurrentUserId(), request), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ActionName("CompleteLesson")]
    public async Task<IActionResult> CompleteLesson(
        [FromBody] CompleteLessonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteLessonCommand(CurrentUserId(), request.LessonId, request.SelectedAnswer),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseReadiness(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCourseReadinessQuery(CurrentUserId(), courseId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PublishCourse(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PublishCourseCommand(CurrentUserId(), courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UnpublishCourse(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnpublishCourseCommand(CurrentUserId(), courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> SaveCoursePrice(
        int courseId,
        [FromBody] SaveCoursePriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SaveCoursePriceCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> SaveCourseCertificate(
        int courseId,
        [FromBody] SaveCourseCertificateRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SaveCourseCertificateCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseNews(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCourseNewsQuery(CurrentUserId(), courseId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PublishCourseNews(
        int courseId,
        [FromBody] PublishCourseNewsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new PublishCourseNewsCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseReviews(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCourseReviewsQuery(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCourseReview(
        int courseId,
        [FromBody] SaveCourseReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SaveCourseReviewCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCoursePromos(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCoursePromosQuery(CurrentUserId(), courseId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddCoursePromo(
        int courseId,
        [FromBody] AddCoursePromoRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AddCoursePromoCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveCoursePromo(
        int courseId,
        int promoId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RemoveCoursePromoCommand(CurrentUserId(), courseId, promoId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseTeachers(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCourseTeachersQuery(CurrentUserId(), courseId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> InviteCourseTeacher(
        int courseId,
        [FromBody] InviteCourseTeacherRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new InviteCourseTeacherCommand(CurrentUserId(), courseId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveCourseTeacher(
        int courseId,
        string userName,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RemoveCourseTeacherCommand(CurrentUserId(), courseId, userName),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCourseCertificate(int courseId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GenerateCourseCertificateCommand(CurrentUserId(), courseId),
            cancellationToken);
        if (!result.IsSuccess)
            return Ok(result);
        return File(result.Value, "application/pdf", $"EduGame-Certificate-{courseId}.pdf");
    }

    private string CurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? string.Empty;
}