using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using MediatR;

namespace EduGame.Application.Chat.GetChatAttachment;

public class GetChatAttachmentQueryHandler(
    ICommunityRepository communityRepository,
    IWebHostEnvironment environment)
    : IRequestHandler<GetChatAttachmentQuery, Result<ChatAttachmentFileResponse>>
{
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

    public async Task<Result<ChatAttachmentFileResponse>> Handle(GetChatAttachmentQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (request.CommunityId == Guid.Empty
            || !await communityRepository.IsUserInCommunity(request.CommunityId, request.UserId))
            return Result.Forbidden("Вложение доступно только участникам сообщества.");

        var safeFileName = Path.GetFileName(request.FileName);
        var extension = Path.GetExtension(safeFileName);

        if (!string.Equals(safeFileName, request.FileName, StringComparison.Ordinal)
            || !Guid.TryParse(Path.GetFileNameWithoutExtension(safeFileName), out _)
            || !AllowedAttachments.TryGetValue(extension, out var contentType))
        {
            return Result.NotFound("Вложение не найдено.");
        }

        var filePath = Path.Combine(
            environment.ContentRootPath,
            "ChatUploads",
            request.CommunityId.ToString("N"),
            safeFileName);

        if (!File.Exists(filePath))
            return Result.NotFound("Вложение не найдено.");

        return Result.Success(new ChatAttachmentFileResponse(filePath, contentType));
    }
}