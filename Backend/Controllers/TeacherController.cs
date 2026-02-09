using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/teacher/[action]")]
    public class TeacherController : RegisterController<Teacher, TeacherDto> 
    {
        public TeacherController(IRegistrationService<Teacher, TeacherDto> registrationService, IMapper mapper) : base(registrationService, mapper) {}

        [ActionName("RegisterTeacher")]
        public override async Task<IActionResult> RegisterUser(TeacherDto teacherDto) => await base.RegisterUser(teacherDto);

        [HttpGet("{externalId}", Name = "GetTeacher")]
        [ActionName("GetTeacher")]
        public override async Task<IActionResult> GetUser(Guid externalId) => await base.GetUser(externalId);
    }
}