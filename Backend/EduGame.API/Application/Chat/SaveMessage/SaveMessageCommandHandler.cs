using MediatR;
using AutoMapper;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Chat.SaveMessage;

public class SaveMessageCommandHandler(
    IMapper mapper,
    IRepository<Message> messageRepository,
    ICommunityRepository communityRepository,
    IIdentityRepository identityRepository
) : IRequestHandler<SaveMessageCommand, Result<SaveMessageResponseDto>>
{
    public async Task<Result<SaveMessageResponseDto>> Handle(SaveMessageCommand request, CancellationToken cancellationToken)
    {
        var key = request.MessageDto.CommunityName;

        if (key == "EduGame") return Result.Forbidden("Чат EduGame доступен только для чтения.");

        var senderCommunity = Guid.TryParse(key, out var communityGuid)
            ? await communityRepository.GetCommunityByGuid(communityGuid)
            : await communityRepository.GetCommunityByName(key);

        if (senderCommunity == null) return Result.NotFound($"Сообщество '{key}' не найдено.");

        if (!await communityRepository.IsUserInCommunity(senderCommunity.CommunityId, request.MessageDto.SenderId))
            return Result.Forbidden("Чтобы отправлять сообщения, нужно вступить в сообщество.");

        var senderRole = await identityRepository.GetUserRole(request.MessageDto.SenderId);

        var messageEntity = mapper.Map<Message>(request.MessageDto);
        messageEntity.CommunityId = senderCommunity.Id;
        messageEntity.Community = null;
        messageEntity.Status = MessageStatus.SentToServer;

        await messageRepository.Add(messageEntity);
        await messageRepository.Save();

        return Result.Success(new SaveMessageResponseDto(messageEntity, senderRole));
    }
}