using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Application.Courses.CreateCourseWithCommunity;

public class CreateCourseWithCommunityCommandHandler(
    ICourseRepository courseRepository,
    ICommunityRepository communityRepository,
    ISkillTreeRepository skillTreeRepository,
    IUserRepository userRepository)
    : IRequestHandler<CreateCourseWithCommunityCommand, Result<CourseWithCommunityCreatedDto>>
{
    public async Task<Result<CourseWithCommunityCreatedDto>> Handle(CreateCourseWithCommunityCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (string.IsNullOrWhiteSpace(request.TeacherId)) 
            return Result.Unauthorized("Для создания курса необходимо войти в аккаунт.");

        var teacher = await userRepository.GetTeacherById(request.TeacherId);
        if (teacher is null) 
            return Result.Forbidden("Создавать курсы может только пользователь с профилем преподавателя.");

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Trim().Length < 5 || dto.Title.Length > 200)
            return Result.Invalid(new ValidationError("Название курса должно содержать от 5 до 200 символов."));

        if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Trim().Length < 10 || dto.Description.Length > 2000)
            return Result.Invalid(new ValidationError("Описание курса должно содержать от 10 до 2000 символов."));
            
        if (dto.PriceCoins < 0 || (!dto.IsPaid && dto.PriceCoins != 0))
            return Result.Invalid(new ValidationError("Цена должна быть неотрицательной и равна нулю для бесплатного курса."));

        if (!Enum.IsDefined(dto.Difficulty))
            return Result.Invalid(new ValidationError("Выберите корректную сложность курса."));

        if (!Enum.IsDefined(dto.PrivacyType) || !Enum.IsDefined(dto.MembersLimit))
            return Result.Invalid(new ValidationError("Выберите корректные настройки сообщества."));

        var communityName = dto.CommunityName.Trim();
        if (communityName.Length is < 3 or > 30)
            return Result.Invalid(new ValidationError("Название сообщества должно содержать от 3 до 30 символов."));
            
        if (await communityRepository.IsCommunityExistsByName(communityName))
            return Result.Conflict("Сообщество с таким именем уже существует.");

        var skillSubSection = await skillTreeRepository.GetSkillSubSection(dto.SkillSubSectionId);
        if (skillSubSection is null) 
            return Result.Invalid(new ValidationError("Выбранная подкатегория обучения не найдена."));

        var community = new Community
        {
            CommunityName = communityName,
            Description = string.IsNullOrWhiteSpace(dto.CommunityDescription)
                ? dto.Description.Trim()
                : dto.CommunityDescription.Trim(),
            TeacherId = teacher.Id,
            MembersCount = 1,
            MembersLimit = dto.MembersLimit,
            PrivacyType = dto.PrivacyType
        };

        var membership = new CommunityMember
        {
            CommunityId = community.CommunityId,
            UserId = teacher.Id,
            UserName = request.TeacherName,
            AvatarUrl = teacher.AvatarUrl ?? "images/hamzaBro3.png",
            Role = ClanRole.Leader
        };

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            IsPaid = dto.IsPaid,
            PriceCoins = dto.IsPaid ? dto.PriceCoins : 0,
            Difficulty = dto.Difficulty,
            SkillSubSectionId = skillSubSection.Id,
            Community = community
        };
        
        communityRepository.AddMember(membership); 
        
        await courseRepository.Add(course);

        await courseRepository.Save();

        return Result.Success(new CourseWithCommunityCreatedDto(course.Id, community.CommunityId, community.CommunityName));
    }
}