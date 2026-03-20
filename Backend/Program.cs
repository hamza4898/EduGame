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
using Serilog;
using EduGame.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Rewrite;

var ecdsaAlgorithm = ECDsa.Create();

ecdsaAlgorithm.ImportFromPem(File.ReadAllText("public.key"));

var authKey = new ECDsaSecurityKey(ecdsaAlgorithm);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting application");

var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "../Frontend");

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultValidationFilter>();
});

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
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<EduGameIdentityContext>()
.AddSignInManager();

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
        
        Log.Warning("Failed FluentValidation with errors: {errors}", errors);
        return new BadRequestObjectResult(new { error = errors });
    }; 
});

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "EduGameServer",

            ValidateAudience = true,
            ValidAudience = "EduGameClient",

            IssuerSigningKey = authKey,
            ValidateIssuerSigningKey = true,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["EduGame-Access-Token"];
                return Task.CompletedTask;
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var jwtError = new { error = "Доступ запрещен. Пожалуйста, авторизуйтесь в EduGame, чтобы продолжить!" };

                await context.Response.WriteAsJsonAsync(jwtError);
            }
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