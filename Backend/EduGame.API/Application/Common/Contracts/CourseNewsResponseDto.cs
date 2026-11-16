namespace EduGame.Application.Common.Contracts;

public sealed record CourseNewsResponseDto(int Id, string Author, string Text, DateTime CreatedAt, int NotifiedStudents = 0);