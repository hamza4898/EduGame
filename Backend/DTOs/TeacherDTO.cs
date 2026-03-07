namespace EduGame.DTOs
{
    public class TeacherDto : BaseRegistrationDto
    {
        public string Gender { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }
}