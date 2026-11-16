namespace EduGame.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendCodeToEmail(string clientRoute, Guid sessionId, string email, string code); 
}