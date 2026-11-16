using FluentValidation;

namespace EduGame.Application.Developer.CreateDeveloperUpdate;

public class CreateDeveloperUpdateRequestValidator : AbstractValidator<CreateDeveloperUpdateRequest>
{
    public CreateDeveloperUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Заголовок статьи не может быть пустым.")
            .Length(3, 120)
            .WithMessage("Заголовок статьи должен содержать от 3 до 120 символов.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Категория обновления должна быть указана.")
            .Length(2, 32)
            .WithMessage("Метка категории должна содержать от 2 до 32 символов.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Текст статьи обновления не может быть пустым.")
            .Length(10, 10000)
            .WithMessage("Текст статьи должен содержать от 10 до 10 000 символов.");
    }
}