using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/teacher/[action]")]
    public class TeacherController : ControllerBase
    {
        private readonly IBaseUserService<Teacher, TeacherDTO> _service;
        private readonly IMapper _mapper;

        public TeacherController(IBaseUserService<Teacher, TeacherDTO> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }   

        [HttpPost]
        public async Task<IActionResult> TeacherRegister(TeacherDTO teacherDTO)
        {
            var teacher = await _service.CreateUser(teacherDTO);

            var responseDTO = _mapper.Map<TeacherDTO>(teacher);

            return CreatedAtRoute("GetTeacher", new { id = teacher.Id }, responseDTO);
        }

        [HttpGet("{id}", Name = "GetTeacher")] 
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _service.GetUserById(id);

            if (teacher == null) return NotFound();

            var responce = _mapper.Map<TeacherDTO>(teacher);

            return Ok(responce);
        }
    }
}