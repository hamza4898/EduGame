using EduGame.Application.Common.Contracts;
using EduGame.Application.Courses.AddCoursePromo;
using EduGame.Application.Courses.InviteCourseTeacher;
using EduGame.Application.Courses.PublishCourseNews;
using EduGame.Application.Courses.SaveCourseCertificate;
using EduGame.Application.Courses.SaveCoursePrice;
using EduGame.Application.Courses.SaveCourseReview;

namespace EduGame.Application.Common.Interfaces;

public interface ICourseManagementService
{
    Task<CourseReadinessResponseDto> GetReadinessAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task<CourseReadinessResponseDto> PublishAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task<CourseReadinessResponseDto> UnpublishAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task SavePriceAsync(string userId, int courseId, SaveCoursePriceRequestDto request, CancellationToken cancellationToken);

    Task SaveCertificateAsync(string userId, int courseId, SaveCourseCertificateRequestDto request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseNewsResponseDto>> GetNewsAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task<CourseNewsResponseDto> PublishNewsAsync(string userId, int courseId, PublishCourseNewsRequestDto request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseReviewResponseDto>> GetReviewsAsync(int courseId, CancellationToken cancellationToken);

    Task<CourseReviewResponseDto> SaveReviewAsync(string userId, int courseId, SaveCourseReviewRequestDto request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CoursePromoResponseDto>> GetPromosAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task<CoursePromoResponseDto> AddPromoAsync(string userId, int courseId, AddCoursePromoRequestDto request, CancellationToken cancellationToken);

    Task RemovePromoAsync(string userId, int courseId, int promoId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseTeacherResponseDto>> GetTeachersAsync(string userId, int courseId, CancellationToken cancellationToken);

    Task InviteTeacherAsync(string userId, int courseId, InviteCourseTeacherRequestDto request, CancellationToken cancellationToken);
    
    Task RemoveTeacherAsync(string userId, int courseId, string teacherUserName, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationResponseDto>> GetNotificationsAsync(string userId, CancellationToken cancellationToken);
    
    Task MarkNotificationReadAsync(string userId, int notificationId, CancellationToken cancellationToken);

    Task<byte[]> GenerateCertificateAsync(string studentId, int courseId, CancellationToken cancellationToken);

    Task<StartCourseResponseDto> StartCourseAsync(string studentId, StartCourseRequestDto request, CancellationToken cancellationToken);
}