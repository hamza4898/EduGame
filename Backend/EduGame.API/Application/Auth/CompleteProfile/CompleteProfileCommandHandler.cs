using MediatR;
using Ardalis.Result;
using EduGame.Domain.Enums;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Auth.CompleteProfile;

public class CompleteProfileCommandHandler(
    IUserRepository userRepository,
    IIdentityService identityService
) : IRequestHandler<CompleteProfileCommand, Result>
{
    public async Task<Result> Handle(CompleteProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindById(request.UserId);

        if (user == null) return Result.NotFound("Пользователь не найден");

        if (user.IsProfileComplete) return Result.Invalid(new ValidationError { ErrorMessage = "Профиль уже заполнен" });

        if (request.Dto.SelectedRole == UserRole.Admin) return Result.Forbidden("Доступ запрещен");

        user.UserRole = request.Dto.SelectedRole;
        user.UserName = request.Dto.UserName;
        user.IsProfileComplete = true;

        var identityResult = await identityService.Update(user);

        if (!identityResult.Succeeded) return Result.Error("Не удалось обновить данные безопасности пользователя");

        if (request.Dto.SelectedRole == UserRole.Student)
        {
            var newStudent = new Student 
            { 
                Id = user.Id, // Ключ связи
                FirstName = request.Dto.FirstName,
                LastName = request.Dto.LastName,
                DateOfBirth = request.Dto.DateOfBirth
            };

            await userRepository.Add(newStudent);
            await userRepository.Save();
        }
        else if (request.Dto.SelectedRole == UserRole.Teacher)
        {
            var newTeacher = new Teacher 
            { 
                Id = user.Id, 
                FirstName = request.Dto.FirstName,
                LastName = request.Dto.LastName,
                DateOfBirth = request.Dto.DateOfBirth
            };
            
            await userRepository.Add(newTeacher);
            await userRepository.Save();
        }
        
        return Result.Success();
    }
}