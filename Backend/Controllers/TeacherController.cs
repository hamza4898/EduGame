using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/teacher/[action]")]
    public class TeacherController : RegisterController<Teacher, TeacherDTO> 
    {
        public TeacherController(IBaseUserService<Teacher, TeacherDTO> service, IMapper mapper) : base(service, mapper) {}

        [ActionName("RegisterTeacher")]
        public override async Task<IActionResult> RegisterUser(TeacherDTO teacherDTO) => await base.RegisterUser(teacherDTO);

        [HttpGet("{id}", Name = "GetTeacher")]
        [ActionName("GetTeacher")]
        public override async Task<IActionResult> GetUser(int id) => await base.GetUser(id);
    }
}