using FluentValidation;

namespace EduGame.Application.Users.UpdateMyProfile;

public sealed class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequestDto>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Имя обязательно для заполнения.")
            .Length(1, 100)
            .WithMessage("Имя не должно превышать 100 символов.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Фамилия обязательна для заполнения.")
            .Length(1, 100)
            .WithMessage("Фамилия не должна превышать 100 символов.");

        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .WithMessage("Укажите дату рождения.");

        RuleFor(x => x.Gender)
            .MaximumLength(30)
            .WithMessage("Длина поля 'Пол' не должна превышать 30 символов.");

        RuleFor(x => x.Education)
            .MaximumLength(150)
            .WithMessage("Название учебного заведения не должно превышать 150 символов.");

        RuleFor(x => x.Subject)
            .MaximumLength(150)
            .WithMessage("Название предмета не должно превышать 150 символов.");
    }
}