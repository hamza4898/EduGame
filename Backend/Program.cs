using Microsoft.OpenApi;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel;
using EduGame.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using EduGame.Services;
using AutoMapper;
using EduGame.Data;
using Microsoft.AspNetCore.Mvc;
using EduGame.Middlewares;
using EduGame.Exceptions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting application");

var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "../Frontend");

builder.Services.AddControllers();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EduGameDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")
)));

builder.Services.AddDbContext<EduGameIdentityContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")
)));

builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<EduGameIdentityContext>();

builder.Services.AddScoped(typeof(IRegistrationService<,>), typeof(RegistrationService<,>));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = string.Join("\n", context.ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage));
        
        Log.Warning("Failed Data Annotations validation with errors: {errors}", errors);
        throw new AuthException(errors);
    }; 
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionMiddleware>();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();