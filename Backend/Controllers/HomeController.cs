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

        public HomeController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public IActionResult StudentRegister([FromForm] StudentDTO studentDTO)
        {
            _studentService.CreateStudent(studentDTO);
            return Ok("Данные успешно сохранены в базу данных");
        }
    }
}