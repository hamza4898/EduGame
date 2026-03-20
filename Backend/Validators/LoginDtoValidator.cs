using EduGame.DTOs;
using FluentValidation;

namespace EduGame.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email обязателен!")
                .EmailAddress().WithMessage("Неверный формат Email!")
                .MaximumLength(50).WithMessage("Слишком много символов для Email!");

            RuleFor(s => s.Password)
                .NotEmpty().WithMessage("Пароль обязателен!")
                .Length(8, 32).WithMessage("Пароль должен быть от 8 до 32 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,32}$")
                .WithMessage("Пароль должен содержать: цифру, заглавную и строчную букву и спецсимвол (!@#$%^&*)");    
        }
    }
}