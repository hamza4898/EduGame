using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using EduGame.DTOs;
using EduGame.Entities;
using BCrypt.Net;

namespace EduGame.Services
{
    public class StudentService : IStudentService
    {
        private readonly EFCoreDbContext _eduGameDbContext;

        public StudentService(EFCoreDbContext eduGameDBContext)
        {
            _eduGameDbContext = eduGameDBContext;
        }

        public void CreateStudent(StudentDTO studentDTO)
        {
            var student = new Student()
            {
                FirstName = studentDTO.FirstName,
                LastName = studentDTO.LastName,
                Gender = studentDTO.Gender,
                Phone = studentDTO.Phone,
                Education = studentDTO.Education,
                Email = studentDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentDTO.Password),
                Motivation = studentDTO.Motivation
            };

            _eduGameDbContext.Students.Add(student);

            _eduGameDbContext.SaveChanges();     
        }
    }
}
