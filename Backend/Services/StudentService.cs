using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using EduGame.DTOs;
using EduGame.Entities;
using BCrypt.Net;
using System.Threading.Tasks;
using AutoMapper;

namespace EduGame.Services
{
    public class StudentService : IStudentService
    {
        private readonly EFCoreDbContext _eduGameDbContext;
        private readonly IMapper _mapper;

        public StudentService(EFCoreDbContext eduGameDBContext, IMapper mapper)
        {
            _eduGameDbContext = eduGameDBContext;
            _mapper = mapper;
        }

        public async Task CreateStudent(StudentDTO studentDTO)
        {
            var student = _mapper.Map<Student>(studentDTO);

            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentDTO.Password);

            _eduGameDbContext.Students.Add(student);
            await _eduGameDbContext.SaveChangesAsync();     
        }
    }
}
