using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/student/[action]")]
    public class StudentController : RegisterController<Student, StudentDto>
    {
        public StudentController(IRegistrationService<Student, StudentDto> registrationService) : base(registrationService) {}

        [ActionName("RegisterStudent")]
        public override async Task<IActionResult> RegisterUser(StudentDto studentDto) => await base.RegisterUser(studentDto);

        [HttpGet("{externalId}", Name = "GetStudent")]
        [ActionName("GetStudent")]
        public override async Task<IActionResult> GetUser(Guid externalId) => await base.GetUser(externalId);
    }
}