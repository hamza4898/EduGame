using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Users.UpdateMyProfile;
using EduGame.Application.Users.GetMyBusinessPassport;

namespace EduGame.Infrastructure.Services;

public sealed class UserProfileService(
    EduGameDbContext context,
    ILevelService levelService,
    IWebHostEnvironment environment,
    ILogger<UserProfileService> logger) : IUserProfileService
{
    private const long MaxAvatarBytes = 5 * 1024 * 1024;
    private const string AvatarUrlPrefix = "/avatars/";

    public async Task<MyBusinessPassportResponseDto?> GetBusinessPassportAsync(
        string? userId,
        string? userName,
        CancellationToken cancellationToken)
    {
        var users = context.Users.AsNoTracking().Include(user => user.BaseUser);
        var account = !string.IsNullOrWhiteSpace(userId)
            ? await users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken)
            : await users.FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);

        if (account?.BaseUser is not { } profile
            || account.UserRole is not (UserRole.Student or UserRole.Teacher))
            return null;

        var userNameValue = account.UserName;
        if (string.IsNullOrWhiteSpace(userNameValue))
            return null;

        var progress = levelService.CalculateProgress(profile.GlobalXp, account.UserRole.ToString());
        return new MyBusinessPassportResponseDto(
            ToProfileDto(profile),
            userNameValue,
            progress,
            account.UserRole.ToString());
    }

    public async Task UpdateMyProfileAsync(
        string userId,
        UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            throw new InvalidDataException("Укажите имя и фамилию.");
        if (request.DateOfBirth is null)
            throw new InvalidDataException("Укажите дату рождения.");

        var account = await context.Users
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (account is null)
            throw new KeyNotFoundException("Пользователь не найден.");

        BaseUser? profile = account.UserRole switch
        {
            UserRole.Student => await context.Students.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken),
            UserRole.Teacher => await context.Teachers.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken),
            _ => null
        };
        if (profile is null)
            throw new KeyNotFoundException("Профиль пользователя не найден.");

        var oldAvatarUrl = profile.AvatarUrl;
        var newAvatarUrl = request.Avatar is null
            ? null
            : await SaveAvatarAsync(request.Avatar, cancellationToken);

        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.DateOfBirth = request.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        profile.AvatarUrl = newAvatarUrl ?? oldAvatarUrl;

        if (profile is Student student)
        {
            student.Gender = request.Gender?.Trim() ?? string.Empty;
            student.Education = request.Education?.Trim() ?? string.Empty;
        }
        else if (profile is Teacher teacher)
        {
            teacher.Gender = request.Gender?.Trim() ?? string.Empty;
            teacher.Subject = request.Subject?.Trim() ?? string.Empty;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (newAvatarUrl is not null)
            DeletePreviousAvatar(oldAvatarUrl);
    }

    private static BusinessPassportUserDto ToProfileDto(BaseUser profile) =>
        new(
            profile.Id,
            profile.FirstName,
            profile.LastName,
            DateOnly.FromDateTime(profile.DateOfBirth),
            profile switch
            {
                Student student => student.Gender,
                Teacher teacher => teacher.Gender,
                _ => null
            },
            profile is Student studentProfile ? studentProfile.Education : null,
            profile is Teacher teacherProfile ? teacherProfile.Subject : null,
            profile.AvatarUrl,
            profile.GlobalXp,
            profile.CoinsBalance);

    private async Task<string> SaveAvatarAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxAvatarBytes)
            throw new InvalidDataException("Аватар должен быть размером не более 5 МБ.");

        await using var input = file.OpenReadStream();
        var signature = new byte[12];
        var bytesRead = await input.ReadAsync(signature.AsMemory(), cancellationToken);
        var extension = GetImageExtension(signature.AsSpan(0, bytesRead));
        if (extension is null)
            throw new InvalidDataException("Поддерживаются только изображения PNG, JPEG и WebP.");

        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");

        var avatarDirectory = Path.Combine(webRoot, "avatars");
        Directory.CreateDirectory(avatarDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(avatarDirectory, fileName);
        input.Position = 0;
        await using (var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous))
        {
            await input.CopyToAsync(output, cancellationToken);
        }

        return AvatarUrlPrefix + fileName;
    }

    private static string? GetImageExtension(ReadOnlySpan<byte> signature)
    {
        if (signature.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";
        if (signature.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }))
            return ".jpg";
        if (signature.Length >= 12
            && signature[..4].SequenceEqual("RIFF"u8)
            && signature[8..12].SequenceEqual("WEBP"u8))
            return ".webp";

        return null;
    }

    private void DeletePreviousAvatar(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl) || !avatarUrl.StartsWith(AvatarUrlPrefix, StringComparison.Ordinal))
            return;

        var fileName = Path.GetFileName(avatarUrl);
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");

        var previousAvatar = Path.Combine(webRoot, "avatars", fileName);
        try
        {
            if (File.Exists(previousAvatar))
                File.Delete(previousAvatar);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not remove previous avatar file {AvatarFile}", fileName);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "Could not remove previous avatar file {AvatarFile}", fileName);
        }
    }
}