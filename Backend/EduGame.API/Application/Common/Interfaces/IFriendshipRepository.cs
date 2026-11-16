using EduGame.Application.Common.Contracts;
using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IFriendshipRepository : IRepository<Friendship>
{
    Task<List<BaseUser>> GetUserFriends(string userId);

    Task<List<FriendResponseDto>> GetUserFriendDetails(string userId);

    Task<List<UserSearchResponseDto>> SearchUsers(string userId, string query, int limit);

    Task<List<FriendshipRequestResponseDto>> GetIncomingRequests(string userId);

    Task<List<FriendshipRequestResponseDto>> GetOutgoingRequests(string userId);

    Task<bool> IsFriendRequestExists(string senderId, string receiverId);

    Task<Friendship?> GetTrackedFriendshipByIdAsync(int id, CancellationToken cancellationToken);

    void DeleteFriendship(Friendship friendship);

    Task<bool> FriendshipExistsAsync(string senderId, string receiverId, CancellationToken cancellationToken);

    void AddFriendship(Friendship friendship);

    void AddNotification(Notification notification);
}