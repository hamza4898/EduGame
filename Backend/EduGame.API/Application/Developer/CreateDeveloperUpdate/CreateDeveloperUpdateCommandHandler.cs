using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Developer.CreateDeveloperUpdate;

public sealed class CreateDeveloperUpdateCommandHandler(
    IRepository<DeveloperUpdate> repository)
    : IRequestHandler<CreateDeveloperUpdateCommand, Result<DeveloperUpdateResponseDto>>
{
    public async Task<Result<DeveloperUpdateResponseDto>> Handle(CreateDeveloperUpdateCommand request, CancellationToken cancellationToken)
    {
        var title = request.Request.Title?.Trim();

        var category = request.Request.Category?.Trim();

        var content = request.Request.Content?.Trim();

        if (string.IsNullOrWhiteSpace(title) || title.Length > 120 || title.Length < 3)
            return Result.Invalid(new ValidationError("Title", "Заголовок должен содержать от 3 до 120 символов."));

        if (string.IsNullOrWhiteSpace(category) || category.Length > 32 || category.Length < 2)
            return Result.Invalid(new ValidationError("Category", "Метка должна содержать от 2 до 32 символов."));

        if (string.IsNullOrWhiteSpace(content) || content.Length > 10_000 || content.Length < 10)
            return Result.Invalid(new ValidationError("Content", "Текст статьи должен содержать от 10 до 10000 символов."));

        var update = new DeveloperUpdate
        {
            Title = title,
            Category = category,
            Content = content,
            AuthorSignature = "Хамза — Главный разработчик EduGame",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repository.Add(update);
        await repository.Save();

        return Result.Success(new DeveloperUpdateResponseDto(
            update.Id,
            update.Title,
            update.Category,
            update.Content,
            update.AuthorSignature,
            update.CreatedAt));
    }
}