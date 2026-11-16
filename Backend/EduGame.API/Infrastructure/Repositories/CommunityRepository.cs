using EduGame.Infrastructure.Data;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Chat.SearchCommunities;
using EduGame.Application.Chat.GetMyCommunities;
using EduGame.Application.Chat.GetCommunityMembers;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Repositories;

public class CommunityRepository(EduGameDbContext context) : Repository<Community>(context), ICommunityRepository
{
    public async Task<Community?> GetCommunityByName(string communityName) =>
        await Context.Communities.FirstOrDefaultAsync(c => c.CommunityName == communityName);

    public async Task<bool> IsCommunityExistsByName(string communityName) =>
        await Context.Communities.AnyAsync(c => c.CommunityName == communityName);

    public async Task<bool> IsUserInCommunity(Guid communityId, string userId) =>
        await Context.CommunityMembers
            .AnyAsync(c => c.CommunityId == communityId && c.UserId == userId);

    public async Task<CommunityMember?> GetCommunityMember(Guid communityId, string userId) =>
        await Context.CommunityMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommunityId == communityId && c.UserId == userId);

    public async Task<List<SearchCommunitiesResponseDto>> SearchCommunities(string query, int offset, int limit) =>
        await Context.Communities
            .AsNoTracking()
            .Where(c => c.CommunityName.ToLower().StartsWith(query.ToLower()) && c.PrivacyType != CommunityPrivacyType.Private)
            .OrderBy(c => c.CommunityName)
            .Skip(offset)
            .Take(limit)
            .Select(c => new SearchCommunitiesResponseDto(
                c.CommunityId,
                c.CommunityName,
                c.MembersCount,
                c.AvatarUrl ?? "images/hamzaBro3.png",
                c.Description
            ))
            .ToListAsync();

    public async Task<List<MyCommunitiesResponseDto>> GetUserCommunities(string userId) =>
        await Context.CommunityMembers
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new MyCommunitiesResponseDto
            {
                CommunityId   = c.Community.CommunityId,
                CommunityName = c.Community.CommunityName,
                AvatarUrl     = c.Community.AvatarUrl,
                Description   = c.Community.Description,
                MembersCount  = c.Community.MembersCount
            })
            .ToListAsync();

    public async Task<List<GetCommunityMembersResponseDto>> GetCommunityMembers(Guid communityId, string requestingUserId, int offset, int limit) =>
        await Context.CommunityMembers
            .AsNoTracking()
            .Where(c => c.CommunityId == communityId)
            .OrderBy(c => c.JoinedAt)
            .Skip(offset)
            .Take(limit)
            .Select(c => new GetCommunityMembersResponseDto(
                c.Community.CommunityName,
                Context.Users
                    .Where(user => user.Id == c.UserId)
                    .Select(user => user.UserName)
                    .FirstOrDefault() ?? c.UserName,
                c.Role,
                Context.BaseUsers
                    .Where(user => user.Id == c.UserId)
                    .Select(user => user.AvatarUrl)
                    .FirstOrDefault() ?? c.AvatarUrl,
                c.UserId,
                Context.Friendships.Any(friendship =>
                    friendship.Status == FriendshipStatus.Accepted
                    && ((friendship.SenderId == requestingUserId && friendship.ReceiverId == c.UserId)
                        || (friendship.ReceiverId == requestingUserId && friendship.SenderId == c.UserId))),
                Context.Friendships.Any(friendship =>
                    friendship.Status == FriendshipStatus.Pending
                    && ((friendship.SenderId == requestingUserId && friendship.ReceiverId == c.UserId)
                        || (friendship.ReceiverId == requestingUserId && friendship.SenderId == c.UserId)))
            ))
            .ToListAsync();

    public async Task<List<Message>> GetMessagesByCommunityId(int communityId, int offset, int limit, DateTime? since = null)
    {
        IQueryable<Message> query = Context.Messages
            .AsNoTracking()
            .Include(m => m.Community)
            .Where(m => m.CommunityId == communityId);

        if (since.HasValue)
            query = query.Where(message => message.SentAt > since.Value);

        return await query
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.MessageId)
            .Skip(offset)
            .Take(limit)
            .OrderBy(m => m.SentAt)
            .ThenBy(m => m.MessageId)
            .ToListAsync();
    }

    public async Task<List<Message>> GetDeveloperMessages(
        int offset,
        int limit,
        CancellationToken cancellationToken) =>
        await Context.Messages
            .AsNoTracking()
            .Where(message => message.CommunityId == null)
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.MessageId)
            .Skip(offset)
            .Take(limit)
            .OrderBy(message => message.SentAt)
            .ThenBy(message => message.MessageId)
            .ToListAsync(cancellationToken);

    public async Task<List<Message>> SearchMessages(
        int communityId,
        string query,
        int limit,
        CancellationToken cancellationToken) =>
        await Context.Messages
            .AsNoTracking()
            .Where(message => message.CommunityId == communityId
                && message.Text.ToLower().Contains(query.ToLower()))
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.MessageId)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetCommunityMemberIds(Guid communityId) =>
        await Context.CommunityMembers
            .AsNoTracking()
            .Where(m => m.CommunityId == communityId)
            .Select(m => m.UserId)
            .ToListAsync();

    public async Task<Community?> GetCommunityByGuid(Guid communityId) =>
        await Context.Communities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommunityId == communityId);

    public async Task<Notification?> GetCommunityInviteNotificationAsync(int notificationId, string userId, Guid communityId, CancellationToken cancellationToken) =>
        await Context.Notifications.FirstOrDefaultAsync(item =>
            item.Id == notificationId
            && item.ReceiverId == userId
            && item.Type == "CommunityInvite"
            && item.TargetId == communityId, cancellationToken);

    public async Task<bool> IsMemberExistsAsync(Guid communityId, string userId, CancellationToken cancellationToken) =>
        await Context.CommunityMembers.AnyAsync(member =>
            member.CommunityId == communityId && member.UserId == userId, cancellationToken);

    public async Task<(string UserName, string? AvatarUrl)?> GetUserShortInfoAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await Context.Users
            .Where(item => item.Id == userId)
            .Select(item => new { item.UserName, AvatarUrl = item.BaseUser == null ? null : item.BaseUser.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return null;

        return (user.UserName!, user.AvatarUrl);
    }

    public void AddMember(CommunityMember member) => 
        Context.CommunityMembers.Add(member);

    public async Task<string?> GetSenderUserNameAsync(string senderId, CancellationToken cancellationToken) =>
        await Context.BaseUsers.AsNoTracking()
            .Where(item => item.Id == senderId)
            .Select(item => item.ApplicationUser.UserName)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> IsUserCommunityManagerAsync(Guid communityId, string userId, CancellationToken cancellationToken) =>
        await Context.CommunityMembers.AnyAsync(member =>
            member.CommunityId == communityId
            && member.UserId == userId
            && member.Role != ClanRole.Member, cancellationToken);

    public async Task<bool> AreUsersFriendsAsync(string user1Id, string user2Id, CancellationToken cancellationToken) =>
        await Context.Friendships.AnyAsync(friendship =>
            friendship.Status == FriendshipStatus.Accepted
            && ((friendship.SenderId == user1Id && friendship.ReceiverId == user2Id)
                || (friendship.SenderId == user2Id && friendship.ReceiverId == user1Id)), cancellationToken);

    public async Task<bool> IsUserExistsAsync(string userId, CancellationToken cancellationToken) =>
        await Context.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    public void AddNotification(Notification notification) =>
        Context.Notifications.Add(notification);
}