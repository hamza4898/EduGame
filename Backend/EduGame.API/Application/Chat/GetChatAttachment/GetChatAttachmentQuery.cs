using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Chat.GetChatAttachment;

public record GetChatAttachmentQuery(Guid CommunityId, string FileName, string UserId)
    : IRequest<Result<ChatAttachmentFileResponse>>;