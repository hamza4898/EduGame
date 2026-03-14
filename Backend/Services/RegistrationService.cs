using EduGame.Data;
using AutoMapper;
using EduGame.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EduGame.Entities;
using Ardalis.Result;

namespace EduGame.Services
{
    public class RegistrationService<T, D>(
        EduGameDbContext eduGameDbContext,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ILogger<RegistrationService<T, D>> logger
    ) : IRegistrationService<T, D>
        where T: class
        where D: BaseRegistrationDto
    {
        public async Task<Result<T>> CreateUser(D userDto)
        {
            var identityUser = mapper.Map<ApplicationUser>(userDto);

            logger.LogInformation("Registration started for user with {ID} ID", identityUser.Id);

            var result = await userManager.CreateAsync(identityUser, userDto.Password!);      

            if (!result.Succeeded)
            {
                var errorMessages = string.Join("\n", result.Errors.Select(e => e.Code switch
                {
                    "DuplicateUserName" => "Этот никнейм уже занят!",
                    "DuplicateEmail" => "Такая почта уже есть в системе!",
                    _ => "Системная ошибка"
                }));

                logger.LogWarning("Failed registration with Identity validation errors: {errorMessages} for user with {ID} ID", errorMessages, identityUser.Id);

                return Result.Invalid(new ValidationError(errorMessages));
            } 

            var userProfile = mapper.Map<T>(userDto);
            ((dynamic)userProfile).ExternalId = identityUser.Id;

            await eduGameDbContext.Set<T>().AddAsync(userProfile);
            await eduGameDbContext.SaveChangesAsync();

            logger.LogInformation("Added a new user with {ID} ID to database contexts", identityUser.Id);

            return Result.Success(userProfile);
        }

        public async Task<Result<T>> GetUserByExternalId(Guid externalId)
        {
            var user = await eduGameDbContext.Set<T>()
                .FirstOrDefaultAsync(user => EF.Property<Guid>(user, "ExternalId") == externalId);

            if (user == null)
            {
                logger.LogInformation("Failed to find user with {externalId} ID in database", externalId);
                return Result.NotFound("Объект не найден в базе данных");
            }

            return Result.Success(user);
        }
    }
}