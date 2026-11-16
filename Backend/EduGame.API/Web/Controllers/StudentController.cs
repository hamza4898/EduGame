using Microsoft.AspNetCore.Mvc;
using MediatR;
using EduGame.Domain.Entities;
using EduGame.Application.Auth.Registration;

namespace EduGame.Web.Controllers;

[ApiController]
[Route("api/student/[action]")] 
public class StudentController(IMediator mediator) 
    : RegisterController<Student, StudentDto>(mediator)
{
    [HttpPost]
    [ActionName("RegisterStudent")]
    public override async Task<IActionResult> RegisterUser(StudentDto studentDto) 
        => await base.RegisterUser(studentDto);
}