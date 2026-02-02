using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/student/[action]")]
    public class StudentController : RegisterController<Student, StudentDTO>
    {
        public StudentController(IBaseUserService<Student, StudentDTO> service, IMapper mapper) : base(service, mapper) {}

        [ActionName("RegisterStudent")]
        public override async Task<IActionResult> RegisterUser(StudentDTO studentDTO) => await base.RegisterUser(studentDTO);

        [HttpGet("{externalId}", Name = "GetStudent")]
        [ActionName("GetStudent")]
        public override async Task<IActionResult> GetUser(Guid externalId) => await base.GetUser(externalId);
    }
}