using Microsoft.AspNetCore.Mvc;
using EduGame.Services;
using EduGame.DTOs;
using EduGame.Entities;

namespace EduGame.Controllers
{
    [ApiController]
    [Route("api/teacher/[action]")]
    public class TeacherController : ControllerBase
    {
        private readonly IBaseUserService<Teacher, TeacherDTO> _service;

        public TeacherController(IBaseUserService<Teacher, TeacherDTO> service)
        {
            _service = service;
        }   

        [HttpPost]
        public async Task<IActionResult> TeacherRegister(TeacherDTO teacherDTO)
        {
            await _service.CreateUser(teacherDTO);
            return Ok("Учитель успешно добавлен в базу данных!");
        } 
    }
}