using EduGame.DTOs;
using FluentValidation;

namespace EduGame.Validators
{
    public class StudentDtoValidator : BaseRegistrationDtoValidator<StudentDto>
    {
        public StudentDtoValidator()
        {
            RuleFor(s => s.Gender)
                .NotEmpty().WithMessage("Пол указывать обязательно!")
                .Matches("^(Male|Female)$").WithMessage("Выбери из списка: Male (мужской), Female (женский)");

            RuleFor(s => s.Education)
                .NotEmpty().WithMessage("Образование указывать обязательно!")
                .Matches("^(Pupil|Middle School|High School|Student|Other)")
                .WithMessage("Выбери из списка: Pupil (дошкольник), Middle School (Ученик средней школы), High School (Ученик старшей школы), Student (Студент), Other (Другое)");
        }
    }
}