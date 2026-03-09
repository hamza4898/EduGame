using System;
using Microsoft.EntityFrameworkCore;

namespace EduGame.Entities
{
    public abstract class BaseUser
    {
        public string ExternalId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string? Motivation { get; set; }
    }
}