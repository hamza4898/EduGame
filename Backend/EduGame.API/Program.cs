using Microsoft.Extensions.FileProviders;
using EduGame.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EduGame.Infrastructure.Services;
using EduGame.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentEmail.MailKitSmtp;
using EduGame.Infrastructure.Repositories;
using MediatR;
using EduGame.Web.Filters;
using EduGame.Web.Middlewares;
using EduGame.Web.Hubs;
using EduGame.Application.Common.Behaviours;
using Npgsql;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Auth.Registration;
using EduGame.Application.Chat.SaveMessage;
using EduGame.Application.Communities.CreateCommunity;
using EduGame.Application.Courses.GetCatalogCourses;
using EduGame.Application.Courses.GetCourse;
using EduGame.Application.Courses.GetStudentsRating;
using EduGame.Application.Courses.GetCourseRatings;

var ecdsaAlgorithm = ECDsa.Create();

ecdsaAlgorithm.ImportFromPem(File.ReadAllText("public.key"));

var authKey = new ECDsaSecurityKey(ecdsaAlgorithm);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting application");

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultValidationFilter>();
});

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

var postgresConnection = new NpgsqlConnectionStringBuilder(connectionString);

if (string.IsNullOrWhiteSpace(postgresConnection.Password))
{
    postgresConnection.Password = builder.Configuration["POSTGRES_PASSWORD"]
        ?? throw new InvalidOperationException(
            "The PostgreSQL password is missing. Set POSTGRES_PASSWORD or provide Password in ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<EduGameDbContext>(options =>
    options.UseNpgsql(postgresConnection.ConnectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddRoles<IdentityRole>()
.AddSignInManager()
.AddEntityFrameworkStores<EduGameDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<RegisterMappingProfile>();
    cfg.AddProfile<MessageMappingProfile>();
    cfg.AddProfile<CommunityMappingProfile>();
    cfg.AddProfile<CatalogMappingProfile>();
    cfg.AddProfile<CourseDetailMappingProfile>();
    cfg.AddProfile<GetStudentsRatingMappingProfile>();
    cfg.AddProfile<GetCourseRatingsMappingProfile>();
    cfg.AddProfile<CatalogMappingProfile>();
    
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

builder.Services.AddScoped<IJwtService, JwtService>();

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
                if (context.Request.Cookies.TryGetValue("EduGame-Access-Token", out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                else if (context.Request.Path.StartsWithSegments("/hubs/chat") && 
                    context.Request.Query.TryGetValue("access_token", out var queryToken))
                {
                    context.Token = queryToken;
                }

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
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.SaveTokens = true;
    });

var emailSettings = builder.Configuration.GetSection("EmailSettings");

builder.Services
    .AddFluentEmail(
        emailSettings["DefaultFromEmail"], 
        emailSettings["DefaultFromName"]
    )
    .AddMailKitSender(new SmtpClientOptions
    { 
        Server = emailSettings["SmtpServer"], 
        Port = int.Parse(emailSettings["SmtpPort"] ?? "1025"),
        UseSsl = false,
        RequiresAuthentication = false
    });

builder.Services.AddScoped<ResultValidationFilter>();

builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IDeveloperUpdateRepository, DeveloperUpdateRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICourseManagementService, CourseManagementService>();
builder.Services.AddScoped<ITeacherProgressionService, TeacherProgressionService>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

    cfg.RegisterGenericHandlers = true; 
});

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddTransient<IValidator<Message>, MessageValidator>();

builder.Services.AddSingleton<IRoomTracker, RoomTracker>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddTransient<ILevelService, LevelService>();

builder.Services.AddScoped<IRewardBalanceService, RewardBalanceService>();

builder.Services.AddSingleton<IGamificationService, GamificationService>();

builder.Services.AddScoped<IRatingCalculatorService, RatingCalculatorService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();

builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();

builder.Services.AddScoped<ISkillTreeRepository, SkillTreeRepository>();

builder.Services.AddScoped<IRatingRepository, RatingRepository>();

builder.Services.AddScoped<IChatRepository, ChatRepository>();

builder.Services.AddTransient<IQrCodeService, QrCodeService>();

builder.Services.AddTransient<IQuizValidationService, QuizValidationService>();

builder.Services.AddScoped<ICommunityRatingService, CommunityRatingService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddTransient<ICourseReadinessService, CourseReadinessService>();

builder.Services.AddTransient<ICourseContentService, CourseContentService>();

builder.Services.AddScoped<ITeacherRatingService, TeacherRatingService>();

string frontendPath;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    frontendPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
else
    frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Frontend");

var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";

if (!Uri.TryCreate(ollamaBaseUrl, UriKind.Absolute, out var ollamaUri)
    || (ollamaUri.Scheme != Uri.UriSchemeHttp && ollamaUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("Ollama:BaseUrl must be an absolute HTTP or HTTPS URL.");
}

builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = ollamaUri;
    client.Timeout = TimeSpan.FromMinutes(3);
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

    app.UseStaticFiles();

    app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = "",
    DefaultFileNames = new List<string> { "Intro.html" } 
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""  
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();