using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Courses.CreateCourseWithCommunity;
using MediatR;

namespace EduGame.Application.Courses.GetCourseCreationOptions;

public class GetCourseCreationOptionsQueryHandler(ISkillTreeRepository skillTreeRepository)
    : IRequestHandler<GetCourseCreationOptionsQuery, Result<List<CourseCreationPathDto>>>
{
    public async Task<Result<List<CourseCreationPathDto>>> Handle(
        GetCourseCreationOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var paths = await skillTreeRepository.GetSkillPathsWithSubSectionsAsync();
        
        var options = paths
            .OrderBy(path => path.Id)
            .Select(path => new CourseCreationPathDto(
                path.Id,
                path.Name,
                path.SubSections
                    .OrderBy(section => section.Id)
                    .Select(section => new CourseCreationSubSectionDto(section.Id, section.Name))
                    .ToList()))
            .ToList();

        return Result.Success(options);
    }
}