using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Chat.UploadChatAttachment;

public sealed record UploadChatAttachmentCommand(Guid CommunityId, string UserId, IFormFile? File)
    : IRequest<Result<ChatAttachmentUploadResponse>>;
