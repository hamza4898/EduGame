using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Repositories;

public class FriendshipRepository(EduGameDbContext context) : Repository<Friendship>(context), IFriendshipRepository
{
    public async Task<List<FriendshipRequestResponseDto>> GetIncomingRequests(string userId) =>
        await Context.Friendships
            .AsNoTracking()
            .Include(r => r.Sender)
            .Where(r => r.ReceiverId == userId && r.Status == FriendshipStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FriendshipRequestResponseDto(
                r.Id,
                r.SenderId,
                r.Sender.ApplicationUser.UserName ?? r.SenderId,
                r.Sender.AvatarUrl,
                r.CreatedAt
            ))
            .ToListAsync();

    public async Task<List<FriendshipRequestResponseDto>> GetOutgoingRequests(string userId) =>
        await Context.Friendships
            .AsNoTracking()
            .Include(r => r.Receiver)
            .Where(r => r.SenderId == userId && r.Status == FriendshipStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FriendshipRequestResponseDto(
                r.Id,
                r.ReceiverId,
                r.Receiver.ApplicationUser.UserName ?? r.ReceiverId,
                r.Receiver.AvatarUrl,
                r.CreatedAt
            ))
            .ToListAsync();

    public async Task<List<BaseUser>> GetUserFriends(string userId) =>
        await Context.Friendships
            .AsNoTracking()
            .Where(r => r.Status == FriendshipStatus.Accepted && (r.SenderId == userId || r.ReceiverId == userId))
            .Select(r => r.SenderId == userId ? r.Receiver : r.Sender)
            .ToListAsync();

    public async Task<List<FriendResponseDto>> GetUserFriendDetails(string userId) =>
        await Context.Friendships
            .AsNoTracking()
            .Where(friendship => friendship.Status == FriendshipStatus.Accepted
                && (friendship.SenderId == userId || friendship.ReceiverId == userId))
            .Select(friendship => new
            {
                friendship.Id,
                UserId = friendship.SenderId == userId ? friendship.ReceiverId : friendship.SenderId,
                UserName = friendship.SenderId == userId
                    ? friendship.Receiver.ApplicationUser.UserName ?? friendship.ReceiverId
                    : friendship.Sender.ApplicationUser.UserName ?? friendship.SenderId,
                AvatarUrl = friendship.SenderId == userId
                    ? friendship.Receiver.AvatarUrl
                    : friendship.Sender.AvatarUrl
            })
            .OrderBy(friend => friend.UserName)
            .Select(friend => new FriendResponseDto(friend.Id, friend.UserId, friend.UserName, friend.AvatarUrl))
            .ToListAsync();

    public async Task<List<UserSearchResponseDto>> SearchUsers(string userId, string query, int limit) =>
        await Context.Users
            .AsNoTracking()
            .Where(user => user.Id != userId && user.UserName != null
                && user.UserName.ToLower().Contains(query.ToLower()))
            .OrderBy(user => user.UserName)
            .Take(limit)
            .Select(user => new UserSearchResponseDto(user.Id, user.UserName ?? user.Id,
                user.BaseUser == null ? null : user.BaseUser.AvatarUrl,
                Context.Friendships.Any(friendship => friendship.Status == FriendshipStatus.Accepted
                    && ((friendship.SenderId == userId && friendship.ReceiverId == user.Id)
                        || (friendship.ReceiverId == userId && friendship.SenderId == user.Id))),
                Context.Friendships.Any(friendship => friendship.Status == FriendshipStatus.Pending
                    && ((friendship.SenderId == userId && friendship.ReceiverId == user.Id)
                        || (friendship.ReceiverId == userId && friendship.SenderId == user.Id)))))
            .ToListAsync();

    public async Task<bool> IsFriendRequestExists(string senderId, string receiverId) =>
        await Context.Friendships
            .AsNoTracking()
            .AnyAsync(f => 
                (f.SenderId == senderId && f.ReceiverId == receiverId) || 
                (f.SenderId == receiverId && f.ReceiverId == senderId)
            );

    public void DeleteFriendship(Friendship friendship) =>
        Context.Friendships.Remove(friendship);

    public Task<Friendship?> GetTrackedFriendshipByIdAsync(int id, CancellationToken cancellationToken) =>
        Context.Friendships.FirstOrDefaultAsync(friendship => friendship.Id == id, cancellationToken);

    public async Task<bool> FriendshipExistsAsync(string senderId, string receiverId, CancellationToken cancellationToken) =>
        await Context.Friendships.AsNoTracking().AnyAsync(f =>
            (f.SenderId == senderId && f.ReceiverId == receiverId)
            || (f.SenderId == receiverId && f.ReceiverId == senderId),
            cancellationToken);

    public void AddFriendship(Friendship friendship) => Context.Friendships.Add(friendship);

    public void AddNotification(Notification notification) => Context.Notifications.Add(notification);
}