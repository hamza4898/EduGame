using System.ComponentModel.DataAnnotations;

namespace EduGame.DTOs
{
    public abstract class BaseRegistrationDto
    {
        public string? UserName { get; set; }
        
        public string? FirstName { get; set; }
         
        public string? LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Motivation { get; set; }
        
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}