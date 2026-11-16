using EduGame.Application.Common.Interfaces;
using FluentEmail.Core;

namespace EduGame.Infrastructure.Services;

public class EmailService(
    IFluentEmail fluentEmail
) : IEmailService
{
    public async Task SendCodeToEmail(string clientRoute, Guid sessionId, string email, string code)
    {
        var magicLink = $"{clientRoute}?session={sessionId}&code={code}";

        await fluentEmail
            .To(email)
            .Subject("Восстановление пароля")
            .Body($"Ваш одноразовый код: {code}. Или воспользуйтесь магической ссылкой: {magicLink}")
            .SendAsync();
    }
}