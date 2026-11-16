using FluentValidation;

namespace EduGame.Application.Chat.SupportAiChat;

public class UserPromptDtoValidator : AbstractValidator<UserPromptDto>
{
    public UserPromptDtoValidator()
    {
        RuleFor(x => x.UserPrompt)
        .NotEmpty().WithMessage("Вопрос не может быть пустым.")
        .MaximumLength(300).WithMessage("Бро, твой вопрос слишком длинный! Изложи суть проблемы компактнее (до 300 символов)");
    }
}