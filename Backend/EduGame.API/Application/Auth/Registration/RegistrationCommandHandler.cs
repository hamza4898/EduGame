using MediatR;
using Ardalis.Result;
using AutoMapper;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.Registration;

public class RegistrationCommandHandler<T, D>(
    IMapper mapper,
    IIdentityService identityService,
    ILogger<RegistrationCommandHandler<T, D>> logger,
    IAuthService authService,
    IEmailService emailService,
    IUserRepository repository
) : IRequestHandler<RegistrationCommand<T, D>, Result<string>>
    where T : BaseUser
    where D : BaseRegistrationDto  
{
    public async Task<Result<string>> Handle(RegistrationCommand<T, D> request, CancellationToken cancellationToken)
    {
        var identityUser = mapper.Map<ApplicationUser>(request.UserDto);

        if (string.IsNullOrWhiteSpace(identityUser.UserName))
        {
            identityUser.UserName = identityUser.Email;
        }

        logger.LogInformation("🚀 Шаг 1: Создаём Identity-пользователя для {Email}", identityUser.Email);

        var result = await identityService.CreateAsync(identityUser, request.UserDto.Password!); 

        if (!result.Succeeded)
        {
            logger.LogError("❌ Шаг 1 провален: {Errors}", string.Join(", ", result.Errors.Select(e => e.Code)));
        
            var errorMessages = string.Join("\n", result.Errors.Select(e => e.Code switch
            {
                "DuplicateUserName" => "Этот никнейм уже занят!",
                "DuplicateEmail" => "Такая почта уже есть в системе!",
                _ => e.Description
            }));

            var validationError = new ValidationError 
            { 
                Identifier = "Email", 
                ErrorMessage = errorMessages 
            };

            return Result.Invalid(validationError);
        } 

        logger.LogInformation("✅ Шаг 1: Identity-пользователь создан, ID = {UserId}", identityUser.Id);

        var userProfile = mapper.Map<T>(request.UserDto);
        userProfile.Id = identityUser.Id;

        switch (identityUser.UserRole)
        {
            case UserRole.Student: await identityService.AddToRole(identityUser, nameof(UserRole.Student).ToUpper()); break;
            case UserRole.Teacher: await identityService.AddToRole(identityUser, nameof(UserRole.Teacher).ToUpper()); break;
        }

        logger.LogInformation("🚀 Шаг 2: Сохраняем бизнес-профиль ({Type})", typeof(T).Name);

        await repository.Add(userProfile);
    
        try
        {
            await repository.Save();
            logger.LogInformation("✅ Шаг 2: Бизнес-профиль сохранён");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Шаг 2: Ошибка сохранения бизнес-профиля. Identity-пользователь {UserId} уже создан, но бизнес-профиль не сохранён!", identityUser.Id);
            throw;
        }

        var emailCode = await authService.GeneratePasswordResetCode(identityUser);

        await emailService.SendCodeToEmail(
            request.UserDto.ClientRoute, 
            emailCode.SessionId, 
            identityUser.Email!, 
            emailCode.Code
        );

        return Result.Success(emailCode.SessionId.ToString());
    }
}