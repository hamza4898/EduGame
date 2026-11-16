namespace EduGame.Application.Common.Contracts;

public sealed class RawTeacherDataDto
{
    public required string Id { get; set; }

    public required string UserName { get; set; }

    public string? AvatarUrl { get; set; }

    public List<RawCourseMetricsDto> Courses { get; set; } = [];

    public int UniqueStudentsCount { get; set; }
}