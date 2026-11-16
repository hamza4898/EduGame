using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using AutoMapper;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Communities.CreateCommunity;

public class CreateCommunityCommandHandler(
    IMapper mapper,
    ICommunityRepository communityRepository,
    IUserRepository userRepository
) : IRequestHandler<CreateCommunityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCommunityCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId)) return Result.Unauthorized("Вы не авторизованы в EduGame! Создание гильдии запрещено");

        var teacher = await userRepository.GetTeacherById(request.UserId);

        var student = teacher is null
            ? await userRepository.GetStudentById(request.UserId)
            : null;
            
        if (teacher is null && student is null)
            return Result.Forbidden("Создавать сообщества могут только пользователи с профилем ученика или преподавателя.");

        var communityEntity = mapper.Map<Community>(request.Dto);
        communityEntity.TeacherId = teacher?.Id;

        var isCommunityExists = await communityRepository.IsCommunityExistsByName(request.Dto.CommunityName);

        if (isCommunityExists) return Result.Conflict("Сообщество с таким именем уже существует");

        await communityRepository.Add(communityEntity);

        var membership = new CommunityMember
        {
            CommunityId = communityEntity.CommunityId,
            UserId = request.UserId,
            UserName = request.UserName,
            AvatarUrl = "images/hamzaBro3.png",
            Role = ClanRole.Leader
        };

        communityRepository.AddMember(membership);
        await communityRepository.Save();

        return Result.Success(communityEntity.CommunityId);
    }
}