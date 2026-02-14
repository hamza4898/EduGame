using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EduGame.Entities
{
    [Index(nameof(ExternalId), IsUnique = true)]
    public class Partner
    {
        [JsonIgnore]
        public int Id { get; set; }
        
        public string? ExternalId { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public string? Company { get; set; }

        public string? TypeOfCooperation { get; set; }

        public string? Motivation { get; set; }  
    }
}