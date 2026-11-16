using EduGame.Domain.Entities;
using EduGame.Application.Chat.SearchCommunities;
using EduGame.Application.Chat.GetCommunityMembers;
using EduGame.Application.Chat.GetMyCommunities;

namespace EduGame.Application.Common.Interfaces;

public interface ICommunityRepository : IRepository<Community>
{
    Task<Community?> GetCommunityByName(string communityName);

    Task<Community?> GetCommunityByGuid(Guid communityId);

    Task<List<SearchCommunitiesResponseDto>> SearchCommunities(string query, int offset, int limit);

    Task<bool> IsCommunityExistsByName(string communityName);

    Task<bool> IsUserInCommunity(Guid CommunityId, string UserId);

    Task<List<GetCommunityMembersResponseDto>> GetCommunityMembers(Guid CommunityId, string RequestingUserId, int Offset, int Limit);

    Task<CommunityMember?> GetCommunityMember(Guid communityId, string userId);

    Task<List<string>> GetCommunityMemberIds(Guid communityId);

    Task<List<MyCommunitiesResponseDto>> GetUserCommunities(string userId);

    void AddMember(CommunityMember member);
    
    Task<List<Message>> GetMessagesByCommunityId(int communityId, int offset, int limit, DateTime? since = null);

    Task<List<Message>> GetDeveloperMessages(int offset, int limit, CancellationToken cancellationToken);

    Task<List<Message>> SearchMessages(int communityId, string query, int limit, CancellationToken cancellationToken);

    Task<Notification?> GetCommunityInviteNotificationAsync(int notificationId, string userId, Guid communityId, CancellationToken cancellationToken);
    
    Task<bool> IsMemberExistsAsync(Guid communityId, string userId, CancellationToken cancellationToken);
    
    Task<(string UserName, string? AvatarUrl)?> GetUserShortInfoAsync(string userId, CancellationToken cancellationToken);

    Task<string?> GetSenderUserNameAsync(string senderId, CancellationToken cancellationToken);
    
    Task<bool> IsUserCommunityManagerAsync(Guid communityId, string userId, CancellationToken cancellationToken);
    
    Task<bool> AreUsersFriendsAsync(string user1Id, string user2Id, CancellationToken cancellationToken);
    
    Task<bool> IsUserExistsAsync(string userId, CancellationToken cancellationToken);
    
    void AddNotification(Notification notification);
}