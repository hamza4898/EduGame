using System.ComponentModel.DataAnnotations;

namespace EduGame.DTOs
{
    public class StudentDto : BaseRegistrationDto
    {
        [Required(ErrorMessage = "Пол указывать обязательно!")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Выбери из списка: Male (мужской), Female (женский)")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Образование указывать обязательно!")]
        [RegularExpression("^(Pupil|Middle School|High School|Student|Other)",
            ErrorMessage = "Выбери из списка: Pupil (дошкольник), Middle School (Ученик средней школы), High School (Ученик старшей школы), Student (Студент), Other (Другое)")]
        public string? Education { get; set; }
    }
}