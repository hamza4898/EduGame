using System.ComponentModel.DataAnnotations;

namespace EduGame.DTOs
{
    public abstract class BaseRegistrationDto
    {
        [Required(ErrorMessage = "Никнейм обязателен!")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Никнейм должен иметь размер от 3 до 20 символов!")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Только латиница, цифры, точки и тире!")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна!")]
        [MaxLength(20, ErrorMessage = "Фамилия не должна быть длиннее 20 символов!")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s-]+$", 
            ErrorMessage = "В фамилии только буквы!")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Имя обязательна!")]
        [MaxLength(20, ErrorMessage = "Имя не должно быть длиннее 20 символов!")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s-]+$", 
            ErrorMessage = "В имени только буквы!")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Дата рождения обязательна!")]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2020", ErrorMessage = "Введите корректную дату рождения (от 1900 до 2020 года)")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен!")]
        [Phone(ErrorMessage = "Неправильно введен номер телефона!")]
        [MaxLength(20, ErrorMessage = "Слишком много цифр для номера телефона!")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email обязателен!")]
        [EmailAddress(ErrorMessage = "Неверный формат Email!")]
        [MaxLength(50, ErrorMessage = "Слишком много символов для Email!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,32}$", 
            ErrorMessage = "Пароль должен быть от 8 до 32 символов и содержать: цифру, заглавную и строчную букву и спецсимвол (!@#$%^&*)")]
        public string? Password { get; set; }

        [MaxLength(200, ErrorMessage = "Слишком много символов!")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:""'()\-]*$", 
            ErrorMessage = "Только буквы, цифры и базовые знаки препинания!")]
        public string? Motivation { get; set; }
    }
}