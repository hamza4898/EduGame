using System.ComponentModel.DataAnnotations;

namespace EduGame.DTOs
{
    public class TeacherDto : BaseRegistrationDto
    {
        [Required(ErrorMessage = "Пол указывать обязательно!")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Выбери из списка: Male (мужской), Female (женский)")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Предмет указывать обязательно!")]
        [MaxLength(20, ErrorMessage = "Превышен лимит букв!")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$", 
            ErrorMessage = "Только буквы, цифры и базовые знаки препинания!")]
        public string? Subject { get; set; }
    }
}