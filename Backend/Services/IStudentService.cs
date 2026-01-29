using EduGame.DTOs;

namespace EduGame.Services
{
    public interface IStudentService
    {
        Task CreateStudent(StudentDTO studentDTO);
    }
}