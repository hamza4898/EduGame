using EduGame.Domain.Enums;

namespace EduGame.Domain.Entities;

public class Community
{
    public int Id { get; set; }

    public Guid CommunityId { get; set; } = Guid.NewGuid();

    public string CommunityName { get; set; } = null!;

    public int MembersCount { get; set; } = 0;

    public CommunityMembersLimit MembersLimit { get; set; }

    public bool IsFull => 
        MembersLimit != CommunityMembersLimit.Unlimited &&
        MembersCount >= (int)MembersLimit;

    public string? AvatarUrl { get; set; }

    public string? Description { get; set; }

    public string? TeacherId { get; set; }
    
    public Teacher Teacher { get; set; } = null!; 

    public CommunityPrivacyType PrivacyType { get; set; } = CommunityPrivacyType.Public;

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}