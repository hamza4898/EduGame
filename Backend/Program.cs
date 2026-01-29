using Microsoft.OpenApi;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel;
using EduGame.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using EduGame.Services;

var builder = WebApplication.CreateBuilder(args);

var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "../Frontend");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EFCoreDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")
)));

builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<ITeacherService, TeacherService>();

builder.Services.AddScoped<IPartnerService, PartnerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = "",
    DefaultFileNames = new List<string> { "Main.html" }
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""  
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
