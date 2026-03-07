using EduGame.DTOs;
using FluentValidation;

namespace EduGame.Validators
{
    public class PartnerDtoValidator : BaseRegistrationDtoValidator<PartnerDto>
    {
        public PartnerDtoValidator()
        {
            RuleFor(s => s.Organization)
                .NotEmpty().WithMessage("Организацию указывать обязательно!")
                .Length(2, 50).WithMessage("Название компании от 2 до 50 символов!")
                .Matches(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$")
                .WithMessage("Только буквы, цифры и базовые знаки препинания!");

            RuleFor(s => s.TypeOfCooperation)
                .NotEmpty().WithMessage("Тип партнерства указывать обязательно!")
                .Matches("^(School|University|Company|Franchise|Other)$")
                .WithMessage("Выбери из списка: School (Школа), University (Университет), Company (Компания), Franchise (Франшиза), Other (Другое)");
        }
    }
}