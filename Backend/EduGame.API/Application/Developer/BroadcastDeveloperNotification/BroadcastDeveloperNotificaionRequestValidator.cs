using FluentValidation;

namespace EduGame.Application.Developer.BroadcastDeveloperNotification;

public sealed class BroadcastDeveloperNotificationRequestValidator : AbstractValidator<BroadcastDeveloperNotificationRequest>
{
    public BroadcastDeveloperNotificationRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Уведомление разработчика не может быть пустым.")
            
            .Length(1, 2000)
            .WithMessage("Текст уведомления должен содержать от 1 до 2000 символов.");
    }
}