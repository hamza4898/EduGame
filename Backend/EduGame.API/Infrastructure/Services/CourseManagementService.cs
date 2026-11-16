using System.Globalization;
using EduGame.Application.Common.Interfaces;
using System.Text;
using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using EduGame.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Courses.SaveCoursePrice;
using EduGame.Application.Courses.SaveCourseCertificate;
using EduGame.Application.Courses.PublishCourseNews;
using EduGame.Application.Courses.SaveCourseReview;
using EduGame.Application.Courses.AddCoursePromo;
using EduGame.Application.Courses.InviteCourseTeacher;

namespace EduGame.Infrastructure.Services;

public sealed class CourseManagementService(
    EduGameDbContext context,
    UserManager<ApplicationUser> userManager,
    IHubContext<NotificationHub> notificationHub,
    ITeacherProgressionService teacherProgressionService) : ICourseManagementService
{
    public async Task<CourseReadinessResponseDto> GetReadinessAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);
        return EvaluateReadiness(course);
    }

    public async Task<CourseReadinessResponseDto> PublishAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);
        var readiness = EvaluateReadiness(course);
        if (!readiness.IsReady)
            throw new InvalidOperationException($"Курс нельзя опубликовать: {string.Join("; ", readiness.MissingRequirements)}");

        course.IsPublished = true;
        await context.SaveChangesAsync(cancellationToken);
        await teacherProgressionService.AwardCoursePublishedAsync(courseId, cancellationToken);
        return readiness;
    }

    public async Task<CourseReadinessResponseDto> UnpublishAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);
        course.IsPublished = false;
        await context.SaveChangesAsync(cancellationToken);
        return EvaluateReadiness(course);
    }

    public async Task SavePriceAsync(string userId, int courseId, SaveCoursePriceRequestDto request, CancellationToken cancellationToken)
    {
        if (request.PriceCoins < 0 || (request.IsPaid && request.PriceCoins == 0) || (!request.IsPaid && request.PriceCoins != 0))
            throw new ArgumentException("Для платного курса цена должна быть больше нуля, для бесплатного — равна нулю.");

        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);
        course.IsPaid = request.IsPaid;
        course.PriceCoins = request.PriceCoins;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveCertificateAsync(string userId, int courseId, SaveCourseCertificateRequestDto request, CancellationToken cancellationToken)
    {
        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);
        course.CertificateEnabled = request.Enabled;
        course.CertificateSigner = request.Signer.Trim();
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseNewsResponseDto>> GetNewsAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await context.Courses.AsNoTracking().FirstOrDefaultAsync(item => item.Id == courseId && item.IsPublished, cancellationToken);

        if (course is null) await GetEditableCourseAsync(userId, courseId, cancellationToken);

        var news = await context.CourseNews.AsNoTracking()
            .Where(item => item.CourseId == courseId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        var authorIds = news.Select(item => item.AuthorId).Distinct().ToArray();

        var authors = await userManager.Users
            .Where(user => authorIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.UserName ?? user.Id, cancellationToken);

        return news.Select(item => new CourseNewsResponseDto(
            item.Id, authors.GetValueOrDefault(item.AuthorId, "Преподаватель"), item.Text, item.CreatedAt)).ToList();
    }

    public async Task<CourseNewsResponseDto> PublishNewsAsync(string userId, int courseId, PublishCourseNewsRequestDto request, CancellationToken cancellationToken)
    {
        var text = request.Text.Trim();

        if (text.Length is 0 or > 2000)
            throw new ArgumentException("Новость должна содержать от 1 до 2000 символов.");

        var course = await GetEditableCourseAsync(userId, courseId, cancellationToken);

        var news = new CourseNews { CourseId = course.Id, AuthorId = userId, Text = text };

        context.CourseNews.Add(news);

        await context.SaveChangesAsync(cancellationToken);

        var recipients = await context.StudentCourses
            .Where(enrollment => enrollment.CourseId == courseId)
            .Select(enrollment => enrollment.StudentId)
            .ToListAsync(cancellationToken);

        var notifiedStudents = 0;

        if (course.IsPublished)
        {
            var pendingNotifications = recipients.Select(receiverId => new Notification
            {
                ReceiverId = receiverId,
                SenderId = userId,
                Message = $"Новая новость курса «{course.Title}»: {text}",
                Type = "CourseNews",
                CourseId = courseId
            }).ToList();

            context.Notifications.AddRange(pendingNotifications);

            await context.SaveChangesAsync(cancellationToken);

            foreach (var notification in pendingNotifications)
            {
                await notificationHub.Clients.User(notification.ReceiverId).SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Message,
                    notification.Type,
                    notification.CourseId,
                    notification.IsRead,
                    notification.CreatedAt
                }, cancellationToken);
            }
            notifiedStudents = pendingNotifications.Count;
        }

        var author = await userManager.FindByIdAsync(userId);

        return new CourseNewsResponseDto(news.Id, author?.UserName ?? userId, news.Text, news.CreatedAt, notifiedStudents);
    }

    public async Task<IReadOnlyList<CourseReviewResponseDto>> GetReviewsAsync(int courseId, CancellationToken cancellationToken)
    {
        if (!await context.Courses.AnyAsync(course => course.Id == courseId && course.IsPublished, cancellationToken))
            return [];

        var reviews = await context.CourseReviews.AsNoTracking()
            .Where(review => review.CourseId == courseId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);

        var studentIds = reviews.Select(review => review.StudentId).Distinct().ToArray();

        var names = await userManager.Users
            .Where(user => studentIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.UserName ?? user.Id, cancellationToken);

        var avatarUrls = await context.BaseUsers.AsNoTracking()
            .Where(user => studentIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.AvatarUrl, cancellationToken);

        return reviews.Select(review => new CourseReviewResponseDto(
            review.Id,
            review.StudentId,
            names.GetValueOrDefault(review.StudentId, "Ученик"),
            avatarUrls.GetValueOrDefault(review.StudentId),
            review.Stars,
            review.Text,
            new DateTimeOffset(DateTime.SpecifyKind(review.CreatedAt, DateTimeKind.Utc)))).ToList();
    }

    public async Task<CourseReviewResponseDto> SaveReviewAsync(string userId, int courseId, SaveCourseReviewRequestDto request, CancellationToken cancellationToken)
    {
        if (request.Stars is < 1 or > 5 || request.Text.Trim().Length > 1000)
            throw new ArgumentException("Оценка должна быть от 1 до 5, текст — не длиннее 1000 символов.");

        if (!await context.StudentCourses.AnyAsync(
                enrollment => enrollment.CourseId == courseId && enrollment.StudentId == userId,
                cancellationToken))
            throw new UnauthorizedAccessException("Оставить отзыв могут только учащиеся курса.");

        if (!await context.Courses.AnyAsync(course => course.Id == courseId && course.IsPublished, cancellationToken))
            throw new KeyNotFoundException("Опубликованный курс не найден.");

        var review = await context.CourseReviews
            .FirstOrDefaultAsync(item => item.CourseId == courseId && item.StudentId == userId, cancellationToken);

        if (review is null)
        {
            review = new CourseReview { CourseId = courseId, StudentId = userId };
            context.CourseReviews.Add(review);
        }

        review.Stars = request.Stars;
        review.Text = request.Text.Trim();
        review.CreatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        await teacherProgressionService.AwardCourseReviewAsync(courseId, userId, cancellationToken);

        var user = await userManager.FindByIdAsync(userId);

        var avatarUrl = await context.BaseUsers.AsNoTracking()
            .Where(baseUser => baseUser.Id == userId)
            .Select(baseUser => baseUser.AvatarUrl)
            .FirstOrDefaultAsync(cancellationToken);

        return new CourseReviewResponseDto(
            review.Id,
            userId,
            user?.UserName ?? userId,
            avatarUrl,
            review.Stars,
            review.Text,
            new DateTimeOffset(DateTime.SpecifyKind(review.CreatedAt, DateTimeKind.Utc)));
    }

    public async Task<IReadOnlyList<CoursePromoResponseDto>> GetPromosAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        await GetEditableCourseAsync(userId, courseId, cancellationToken);

        return await context.CoursePromoCodes.AsNoTracking()
            .Where(promo => promo.CourseId == courseId)
            .OrderBy(promo => promo.Code)
            .Select(promo => new CoursePromoResponseDto(promo.Id, promo.Code, promo.DiscountPercent))
            .ToListAsync(cancellationToken);
    }

    public async Task<CoursePromoResponseDto> AddPromoAsync(string userId, int courseId, AddCoursePromoRequestDto request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (code.Length is < 3 or > 32 || request.DiscountPercent is < 1 or > 100)
            throw new ArgumentException("Промокод должен содержать 3–32 символа, скидка — от 1 до 100%.");

        await GetEditableCourseAsync(userId, courseId, cancellationToken);

        if (await context.CoursePromoCodes.AnyAsync(
                promo => promo.CourseId == courseId && promo.Code == code, cancellationToken))
            throw new InvalidOperationException("Такой промокод уже существует.");

        var promoCode = new CoursePromoCode { CourseId = courseId, Code = code, DiscountPercent = request.DiscountPercent };

        context.CoursePromoCodes.Add(promoCode);
        await context.SaveChangesAsync(cancellationToken);

        return new CoursePromoResponseDto(promoCode.Id, promoCode.Code, promoCode.DiscountPercent);
    }

    public async Task RemovePromoAsync(string userId, int courseId, int promoId, CancellationToken cancellationToken)
    {
        await GetEditableCourseAsync(userId, courseId, cancellationToken);
        var promo = await context.CoursePromoCodes.FirstOrDefaultAsync(
            item => item.Id == promoId && item.CourseId == courseId, cancellationToken);
        if (promo is null)
            throw new KeyNotFoundException("Промокод не найден.");
        context.CoursePromoCodes.Remove(promo);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseTeacherResponseDto>> GetTeachersAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        await GetEditableCourseAsync(userId, courseId, cancellationToken);
        return await context.CourseTeacherAccesses.AsNoTracking()
            .Where(access => access.CourseId == courseId)
            .Join(context.Users, access => access.TeacherId, user => user.Id,
                (access, user) => new CourseTeacherResponseDto(user.UserName ?? user.Id, access.Role))
            .ToListAsync(cancellationToken);
    }

    public async Task InviteTeacherAsync(string userId, int courseId, InviteCourseTeacherRequestDto request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);
        var nickname = request.UserName.Trim();
        var teacher = await userManager.Users.FirstOrDefaultAsync(
            user => user.NormalizedUserName == nickname.ToUpperInvariant(), cancellationToken);
        if (teacher is null || !await context.Teachers.AnyAsync(item => item.Id == teacher.Id, cancellationToken))
            throw new KeyNotFoundException("Преподаватель с таким никнеймом не найден.");
        if (teacher.Id == userId)
            throw new ArgumentException("Нельзя добавить себя.");

        var alreadyAdded = await context.CourseTeacherAccesses.AnyAsync(
            access => access.CourseId == courseId && access.TeacherId == teacher.Id, cancellationToken);
        if (alreadyAdded)
            throw new InvalidOperationException("Этот преподаватель уже имеет доступ к курсу.");

        context.CourseTeacherAccesses.Add(new CourseTeacherAccess
        {
            CourseId = courseId,
            TeacherId = teacher.Id,
            Role = "Editor"
        });
        var notification = new Notification
        {
            ReceiverId = teacher.Id,
            SenderId = userId,
            Message = $"Вам предоставлен доступ к редактированию курса «{course.Title}».",
            Type = "CourseAccess",
            CourseId = courseId
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);
        await notificationHub.Clients.User(teacher.Id).SendAsync("ReceiveNotification", new
        {
            notification.Id,
            notification.Message,
            notification.Type,
            notification.CourseId,
            notification.IsRead,
            notification.CreatedAt
        }, cancellationToken);
    }

    public async Task RemoveTeacherAsync(string userId, int courseId, string teacherUserName, CancellationToken cancellationToken)
    {
        await GetOwnedCourseAsync(userId, courseId, cancellationToken);
        var teacherId = await userManager.Users
            .Where(user => user.NormalizedUserName == teacherUserName.Trim().ToUpperInvariant())
            .Select(user => user.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (teacherId is null)
            throw new KeyNotFoundException("Преподаватель не найден.");
        var access = await context.CourseTeacherAccesses.FirstOrDefaultAsync(
            item => item.CourseId == courseId && item.TeacherId == teacherId, cancellationToken);
        if (access is null)
            throw new KeyNotFoundException("Доступ преподавателя не найден.");
        context.CourseTeacherAccesses.Remove(access);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationResponseDto>> GetNotificationsAsync(string userId, CancellationToken cancellationToken)
    {
        var notifications = await context.Notifications.AsNoTracking()
            .Where(notification => notification.ReceiverId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
        var senderIds = notifications.Select(notification => notification.SenderId).Distinct().ToArray();
        var senders = await userManager.Users.Where(user => senderIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.UserName ?? user.Id, cancellationToken);
        return notifications.Select(notification => new NotificationResponseDto(
            notification.Id,
            senders.GetValueOrDefault(notification.SenderId, "EduGame"),
            notification.Message,
            notification.Type,
            notification.CourseId,
            notification.TargetId,
            notification.IsRead,
            notification.CreatedAt)).ToList();
    }

    public async Task MarkNotificationReadAsync(string userId, int notificationId, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications.FirstOrDefaultAsync(
            item => item.Id == notificationId && item.ReceiverId == userId, cancellationToken);
        if (notification is null)
            throw new KeyNotFoundException("Уведомление не найдено.");
        notification.IsRead = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<StartCourseResponseDto> StartCourseAsync(string studentId, StartCourseRequestDto request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var lockedStudentId = await context.Database.SqlQuery<string>(
                $"""SELECT "Id" AS "Value" FROM "Students" WHERE "Id" = {studentId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);

        if (lockedStudentId is null)
            throw new KeyNotFoundException("Профиль учащегося не найден.");

        var student = await context.Students.FirstOrDefaultAsync(item => item.Id == studentId, cancellationToken)
            ?? throw new KeyNotFoundException("Профиль учащегося не найден.");

        var course = await context.Courses
            .Include(item => item.Community)
            .FirstOrDefaultAsync(item => item.Id == request.CourseId && item.IsPublished, cancellationToken)
            ?? throw new KeyNotFoundException("Опубликованный курс не найден.");

        var alreadyCommunityMember = await context.CommunityMembers.AnyAsync(
            member => member.CommunityId == course.Community.CommunityId && member.UserId == studentId,
            cancellationToken);

        if (!alreadyCommunityMember && course.Community.IsFull)
            throw new InvalidOperationException("В сообществе курса больше нет свободных мест.");

        if (await context.StudentCourses.AnyAsync(
                item => item.StudentId == studentId && item.CourseId == course.Id, cancellationToken))
            throw new InvalidOperationException("Вы уже записаны на этот курс.");

        var charge = course.IsPaid ? course.PriceCoins : 0;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoCode = request.PromoCode.Trim().ToUpperInvariant();
            var promo = await context.CoursePromoCodes.FirstOrDefaultAsync(
                item => item.CourseId == course.Id && item.Code == promoCode, cancellationToken);
            if (promo is null)
                throw new ArgumentException("Промокод не найден или не действует для этого курса.");
            charge = (int)Math.Ceiling(charge * (100 - promo.DiscountPercent) / 100m);
        }

        if (student.CoinsBalance < charge)
            throw new InvalidOperationException($"Недостаточно монет. Необходимо: {charge}, на балансе: {student.CoinsBalance}.");

        Teacher? teacher = null;

        if (charge > 0)
        {
            var teacherId = course.Community.TeacherId;
            if (string.IsNullOrWhiteSpace(teacherId))
                throw new InvalidOperationException("У курса не указан преподаватель для зачисления оплаты.");

            var lockedTeacherId = await context.Database.SqlQuery<string>(
                    $"""SELECT "Id" AS "Value" FROM "Teachers" WHERE "Id" = {teacherId} FOR UPDATE""")
                .SingleOrDefaultAsync(cancellationToken);
            if (lockedTeacherId is null)
                throw new InvalidOperationException("Преподаватель курса не найден, покупка отменена.");

            teacher = await context.Teachers.FirstOrDefaultAsync(item => item.Id == teacherId, cancellationToken)
                ?? throw new InvalidOperationException("Преподаватель курса не найден, покупка отменена.");
        }

        student.CoinsBalance -= charge;

        if (teacher is not null) teacher.CoinsBalance = checked(teacher.CoinsBalance + charge);

        context.StudentCourses.Add(new StudentCourse
        {
            StudentId = studentId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            ProgressPercent = 0,
            IsCompleted = false
        });

        if (charge > 0 && teacher is not null)
        {
            var createdAt = DateTime.UtcNow;
            var transactionGroupId = Guid.NewGuid();
            context.CoinTransactions.AddRange(
                new CoinTransaction
                {
                    UserId = studentId,
                    Amount = -charge,
                    Type = "CoursePurchase",
                    CourseId = course.Id,
                    CounterpartyId = teacher.Id,
                    TransactionGroupId = transactionGroupId,
                    CreatedAt = createdAt
                },
                new CoinTransaction
                {
                    UserId = teacher.Id,
                    Amount = charge,
                    Type = "CourseSale",
                    CourseId = course.Id,
                    CounterpartyId = studentId,
                    TransactionGroupId = transactionGroupId,
                    CreatedAt = createdAt
                });
        }

        if (!alreadyCommunityMember)
        {
            context.CommunityMembers.Add(new CommunityMember
            {
                CommunityId = course.Community.CommunityId,
                UserId = studentId,
                AvatarUrl = student.AvatarUrl ?? string.Empty,
                Role = ClanRole.Member,
                JoinedAt = DateTime.UtcNow
            });
            course.Community.MembersCount += 1;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new StartCourseResponseDto(charge, student.CoinsBalance);
    }

    public async Task<byte[]> GenerateCertificateAsync(string studentId, int courseId, CancellationToken cancellationToken)
    {
        var enrollment = await context.StudentCourses.AsNoTracking()
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.CourseId == courseId && item.StudentId == studentId, cancellationToken);
        if (enrollment is null || !enrollment.IsCompleted)
            throw new UnauthorizedAccessException("Сертификат доступен после завершения курса.");
        if (!enrollment.Course.CertificateEnabled)
            throw new InvalidOperationException("Сертификат для этого курса не включён.");

        var student = await userManager.FindByIdAsync(studentId);
        if (student is null)
            throw new KeyNotFoundException("Пользователь не найден.");
        var content = CertificatePdf.Create(
            EscapePdfText(Transliterate(student.UserName ?? "Student")),
            EscapePdfText(Transliterate(enrollment.Course.Title)),
            EscapePdfText(Transliterate(enrollment.Course.CertificateSigner)),
            DateTime.UtcNow);
        return content;
    }

    private async Task<Course> GetEditableCourseAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await context.Courses
            .Include(item => item.Sections)
                .ThenInclude(section => section.Lessons)
                    .ThenInclude(lesson => lesson.Blocks)
            .Include(item => item.News)
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);

        if (course is null)
            throw new KeyNotFoundException("Курс не найден.");

        var isOwner = await context.Communities.AnyAsync(
            community => community.Id == course.CommunityId && community.TeacherId == userId,
            cancellationToken);

        var hasAccess = await context.CourseTeacherAccesses.AnyAsync(
            access => access.CourseId == courseId && access.TeacherId == userId, cancellationToken);
            
        if (!isOwner && !hasAccess)
            throw new UnauthorizedAccessException("Нет прав на редактирование курса.");

        return course;
    }

    private async Task<Course> GetOwnedCourseAsync(string userId, int courseId, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course is null)
            throw new KeyNotFoundException("Курс не найден.");
        if (!await context.Communities.AnyAsync(
                community => community.Id == course.CommunityId && community.TeacherId == userId,
                cancellationToken))
            throw new UnauthorizedAccessException("Управлять доступом может только владелец курса.");
        return course;
    }

    private static CourseReadinessResponseDto EvaluateReadiness(Course course)
    {
        var lessons = course.Sections.SelectMany(section => section.Lessons).ToList();
        var hasTwoContentLessons = lessons.Count(lesson => lesson.Blocks.Any(block => !string.IsNullOrWhiteSpace(block.Content))) >= 2;
        var hasTask = lessons.SelectMany(lesson => lesson.Blocks).Any(block => block.Type == BlockType.Quiz);
        var missing = new List<string>();
        if (course.Title.Trim().Length < 5) missing.Add("Название курса должно содержать не менее 5 символов.");
        if (course.Description.Trim().Length < 20) missing.Add("Добавьте описание не короче 20 символов.");
        if (lessons.Count < 3) missing.Add("Добавьте не менее 3 уроков.");
        if (!hasTwoContentLessons) missing.Add("Заполните содержанием не менее 2 уроков.");
        if (!hasTask) missing.Add("Добавьте хотя бы один квиз с проверкой ответа.");
        if (course.News.Count == 0) missing.Add("Опубликуйте хотя бы одну новость.");
        if (course.IsPaid ? course.PriceCoins <= 0 : course.PriceCoins != 0)
            missing.Add("Укажите корректную цену или выберите бесплатный курс.");
        return new CourseReadinessResponseDto(missing.Count == 0, missing);
    }

    private static string Transliterate(string value)
    {
        var replacements = new Dictionary<char, string>
        {
            ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ё']="yo", ['ж']="zh",
            ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n", ['о']="o",
            ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f", ['х']="kh", ['ц']="ts",
            ['ч']="ch", ['ш']="sh", ['щ']="shch", ['ъ']="", ['ы']="y", ['ь']="", ['э']="e", ['ю']="yu", ['я']="ya"
        };
        var result = new StringBuilder();
        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            var lower = char.ToLowerInvariant(character);
            if (replacements.TryGetValue(lower, out var replacement))
                result.Append(char.IsUpper(character) ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(replacement) : replacement);
            else if (char.IsAscii(character) && !char.IsControl(character))
                result.Append(character);
        }
        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string EscapePdfText(string value) =>
        string.Concat(value.Where(character => !char.IsControl(character)))
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
}