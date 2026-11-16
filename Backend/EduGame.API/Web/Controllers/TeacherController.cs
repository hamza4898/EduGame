using Microsoft.AspNetCore.Mvc;
using MediatR;
using EduGame.Domain.Entities;
using EduGame.Application.Auth.Registration;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/teacher/[action]")]
public class TeacherController(IMediator mediator) 
    : RegisterController<Teacher, TeacherDto>(mediator)
{
    [HttpPost]
    [ActionName("RegisterTeacher")]
    public override async Task<IActionResult> RegisterUser(TeacherDto teacherDto) 
        => await base.RegisterUser(teacherDto);
}