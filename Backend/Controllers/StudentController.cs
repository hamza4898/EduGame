using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;
using AutoMapper;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/student/[action]")]
    public class StudentController : ControllerBase
    {
        private readonly IBaseUserService<Student, StudentDTO> _service;
        private readonly IMapper _mapper;

        public StudentController(IBaseUserService<Student, StudentDTO> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }    

        [HttpPost]
        public async Task<IActionResult> StudentRegister(StudentDTO studentDTO)
        {
            var student = await _service.CreateUser(studentDTO);

            var responseDTO = _mapper.Map<StudentDTO>(student);

            return CreatedAtRoute("GetStudent", new { id = student.Id }, responseDTO);
        }

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _service.GetUserById(id);

            if (student == null) return NotFound();

            var responce = _mapper.Map<StudentDTO>(student);

            return Ok(responce);
        }
    }
}