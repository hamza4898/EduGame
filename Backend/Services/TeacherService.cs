using EduGame.DTOs;
using EduGame.Entities;
using BCrypt.Net;
using System.Threading.Tasks;
using AutoMapper;

namespace EduGame.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly EFCoreDbContext _eduGameDbContext;
        private readonly IMapper _mapper;

        public TeacherService(EFCoreDbContext eduGameDbContext, IMapper mapper)
        {
            _eduGameDbContext = eduGameDbContext;
            _mapper = mapper;
        }

        public async Task CreateTeacher(TeacherDTO teacherDTO)
        {
            var teacher = _mapper.Map<Teacher>(teacherDTO);

            teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacherDTO.Password);

            _eduGameDbContext.Add(teacher);
            await _eduGameDbContext.SaveChangesAsync();
        }
    }
}