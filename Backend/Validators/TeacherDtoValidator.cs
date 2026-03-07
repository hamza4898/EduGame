using EduGame.DTOs;
using FluentValidation;

namespace EduGame.Validators
{
    public class TeacherDtoValidator : BaseRegistrationDtoValidator<TeacherDto>
    {
        public TeacherDtoValidator()
        {   
            RuleFor(s => s.Gender)
                .NotEmpty().WithMessage("Пол указывать обязательно!")
                .Matches("^(Male|Female)$").WithMessage("Выбери из списка: Male (мужской), Female (женский)");
        
            RuleFor(s => s.Subject)
                .NotEmpty().WithMessage("Предмет указывать обязательно!")
                .MaximumLength(20).WithMessage("Превышен лимит символов!")
                .Matches(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$").WithMessage("Только буквы, цифры и базовые знаки препинания!");     
        }
    }    
}
