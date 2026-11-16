using FluentValidation;
using EduGame.Domain.Entities;

namespace EduGame.Application.Chat.SaveMessage;

public class MessageValidator : AbstractValidator<Message>
{
    public MessageValidator()
    {
        RuleFor(m => m.MessageId)
            .NotEmpty().WithMessage("Message ID не может быть пустым")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Message ID must be a valid GUID string.");

        RuleFor(m => m.CommunityId)
            .NotEmpty().WithMessage("Идентификатор сообщества обязателен")
            .GreaterThan(0).WithMessage("Некорректный идентификатор сообщества");

        RuleFor(m => m.SenderId)
            .NotEmpty().WithMessage("Id отправителя обязательно");

        RuleFor(m => m.Text)
            .NotEmpty().WithMessage("Текст сообщения обязателен")
            .MaximumLength(1000).WithMessage("Сообщение не может быть длиннее 1000 символов");
    }
}