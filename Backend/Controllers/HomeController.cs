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
        private readonly IPartnerService _partnerService;

        public HomeController(IStudentService studentService, ITeacherService teacherService, IPartnerService partnerService)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _partnerService = partnerService;
        }

        [HttpPost]
        public IActionResult StudentRegister([FromBody] StudentDTO studentDTO)
        {
            _studentService.CreateStudent(studentDTO);
            return Ok();
        }

        [HttpPost]
        public IActionResult TeacherRegister([FromBody] TeacherDTO teacherDTO)
        {
            _teacherService.CreateTeacher(teacherDTO);
            return Ok("Сенсей успешно сохранен в базе данных");
        }

        [HttpPost]
        public IActionResult PartnerRegister([FromBody] PartnerDTO partnerDTO)
        {
            _partnerService.CreatePartner(partnerDTO);
            return Ok(); 
        }
    }
}