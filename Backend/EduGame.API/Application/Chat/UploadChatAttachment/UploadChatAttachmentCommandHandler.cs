using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;

namespace EduGame.Application.Chat.UploadChatAttachment;

public sealed class UploadChatAttachmentCommandHandler(
    ICommunityRepository communityRepository,
    IWebHostEnvironment environment)
    : IRequestHandler<UploadChatAttachmentCommand, Result<ChatAttachmentUploadResponse>>
{
    private const long MaxAttachmentSize = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedAttachments =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
            [".mp4"] = "video/mp4",
            [".webm"] = "video/webm",
            [".mov"] = "video/quicktime"
        };

    public async Task<Result<ChatAttachmentUploadResponse>> Handle(UploadChatAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        
        if (request.CommunityId == Guid.Empty
            || !await communityRepository.IsUserInCommunity(request.CommunityId, request.UserId))
            return Result.Forbidden("Загружать вложения могут только участники сообщества.");

        var file = request.File;

        if (file is null || file.Length == 0 || file.Length > MaxAttachmentSize)
            return Result.Invalid(new ValidationError("Файл пустой или превышает лимит 10 МБ."));

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedAttachments.TryGetValue(extension, out var contentType)
            || !string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Invalid(new ValidationError("Поддерживаются только изображения и видео разрешённых форматов."));
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var directory = Path.Combine(environment.ContentRootPath, "ChatUploads", request.CommunityId.ToString("N"));

        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var messageType = contentType.StartsWith("image/", StringComparison.Ordinal) ? "image" : "video";

        return Result.Success(new ChatAttachmentUploadResponse(
            $"/api/chat/GetAttachment/{request.CommunityId:D}/{fileName}",
            messageType));
    }
}