using EduGame.DTOs;

namespace EduGame.Services
{
    public interface ITeacherService
    {
        Task CreateTeacher(TeacherDTO teacherDTO);
    }
}