using EduGame.DTOs;
using EduGame.Entities;
using BCrypt.Net;
using System.Threading.Tasks;

namespace EduGame.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly EFCoreDbContext _eduGameDbContext;

        public TeacherService(EFCoreDbContext eduGameDbContext)
        {
            _eduGameDbContext = eduGameDbContext;
        }

        public async Task CreateTeacher(TeacherDTO teacherDTO)
        {
            var teacher = new Teacher
            {
                FirstName = teacherDTO.FirstName,
                LastName = teacherDTO.LastName,
                Gender = teacherDTO.Gender,
                Phone = teacherDTO.Phone,
                Subject = teacherDTO.Subject,
                Email = teacherDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacherDTO.Password),
                Motivation = teacherDTO.Motivation
            };

            _eduGameDbContext.Add(teacher);

            await _eduGameDbContext.SaveChangesAsync();
        }
    }
}