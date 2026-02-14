using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EduGame.Entities
{
    [Index(nameof(ExternalId), IsUnique = true)]
    public class Student
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string? ExternalId { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Education { get; set; }

        public string? Motivation { get; set; }
    }
}