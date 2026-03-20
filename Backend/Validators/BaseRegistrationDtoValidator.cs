using EduGame.DTOs;
using FluentValidation;

namespace EduGame.Validators
{
    public abstract class BaseRegistrationDtoValidator<T> : AbstractValidator<T>
        where T : BaseRegistrationDto
    {
        public BaseRegistrationDtoValidator()
        {
            RuleFor(s => s.UserName)
                .NotEmpty().WithMessage("Никнейм обязателен!")
                .Length(3, 20).WithMessage("Никнейм должен иметь размер от 3 до 20 символов!")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Только латиница, цифры, точки и тире!");

            RuleFor(s => s.FirstName)
                .NotEmpty().WithMessage("Фамилия обязательна!")
                .MaximumLength(20).WithMessage("Фамилия не должна быть длиннее 20 символов!")
                .Matches(@"^[a-zA-Zа-яА-ЯёЁ\s-]+$").WithMessage("В фамилии только буквы!");
            
            RuleFor(s => s.LastName)
                .NotEmpty().WithMessage("Имя обязательно!")
                .MaximumLength(20).WithMessage("Имя не должно быть длиннее 20 символов!")
                .Matches(@"^[a-zA-Zа-яА-ЯёЁ\s-]+$").WithMessage("В имени только буквы!");

            RuleFor(s => s.DateOfBirth)
                .NotEmpty().WithMessage("Дата рождения обязательна!")
                .GreaterThanOrEqualTo(new DateTime(1900, 1, 1)).WithMessage("Дата рождения должна быть больше 1900 года!")
                .LessThanOrEqualTo(DateTime.Today.AddYears(-7)).WithMessage("Возраст не должен быть меньше 7!");

            RuleFor(s => s.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефона обязателен!")
                .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Введите корректный номер телефона!")
                .MaximumLength(20).WithMessage("Слишком много цифр для номера телефона!");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email обязателен!")
                .EmailAddress().WithMessage("Неверный формат Email!")
                .MaximumLength(50).WithMessage("Слишком много символов для Email!");

            RuleFor(s => s.Password)
                .NotEmpty().WithMessage("Пароль обязателен!")
                .Length(8, 32).WithMessage("Пароль должен быть от 8 до 32 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,32}$")
                .WithMessage("Пароль должен содержать: цифру, заглавную и строчную букву и спецсимвол (!@#$%^&*)");

            RuleFor(s => s.Motivation)
                .MaximumLength(200).WithMessage("Слишком много символов!")
                .Matches(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$").WithMessage("Только буквы, цифры и базовые знаки препинания!");
        }
    }
}