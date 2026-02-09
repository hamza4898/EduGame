namespace EduGame.DTOs
{
    public abstract class BaseRegistrationDto
    {
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Motivation { get; set; }
        
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}