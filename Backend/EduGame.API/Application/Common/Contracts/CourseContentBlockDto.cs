using EduGame.Domain.Enums;

namespace EduGame.Application.Common.Contracts;

public record CourseContentBlockDto(
    int Id,
    BlockType Type,
    string Content,
    int Order
);