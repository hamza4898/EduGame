namespace EduGame.DTOs
{
    public class StudentDto : BaseRegistrationDto
    {
        public string Gender { get; set; } = string.Empty;
        
        public string Education { get; set; } = string.Empty;
    }
}