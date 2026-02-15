using System.ComponentModel.DataAnnotations;

namespace EduGame.DTOs
{
    public class PartnerDto : BaseRegistrationDto
    {
        [Required(ErrorMessage = "Компанию указывать обязательно!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название компании - от 2 до 50 символов!")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$", 
            ErrorMessage = "Только буквы, цифры и базовые знаки препинания!")]
        public string? Company { get; set; }
        
        [Required(ErrorMessage = "Тип партнерства указывать обязательно!")]
        [RegularExpression("^(School|University|Company|Franchise|Other)$", 
            ErrorMessage = "Выбери из списка: School (Школа), University (Университет), Company (Компания), Franchise (Франшиза), Other (Другое)")]
        public string? TypeOfCooperation { get; set; }
    }
}