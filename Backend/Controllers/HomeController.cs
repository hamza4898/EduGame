using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EduGame.Services;
using EduGame.DTOs;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/home/[action]")]
    public class HomeController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public HomeController(IStudentService studentService, ITeacherService teacherService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
        }

        [HttpPost]
        public IActionResult StudentRegister([FromForm] StudentDTO studentDTO)
        {
            _studentService.CreateStudent(studentDTO);
            return Ok("Студент успешно сохранен в базе данных");
        }

        [HttpPost]
        public IActionResult TeacherRegister([FromForm] TeacherDTO teacherDTO)
        {
            _teacherService.CreateTeacher(teacherDTO);
            return Ok("Сенсей успешно сохранен в базе данных");
        }
    }
}