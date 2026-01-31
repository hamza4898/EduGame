using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/student/[action]")]
    public class StudentController : ControllerBase
    {
        private readonly IBaseUserService<Student, StudentDTO> _service;

        public StudentController(IBaseUserService<Student, StudentDTO> service)
        {
            _service = service;
        }    

        [HttpPost]
        public async Task<IActionResult> StudentRegister(StudentDTO studentDTO)
        {
            await _service.CreateUser(studentDTO);
            return Ok("Студент успешно добавлен в базу данных!");
        }
    }
}