using System.Collections.Concurrent;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Services;

public class RoomTracker : IRoomTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _rooms = new();

    public void ConnectionJoined(string roomName, string connectionId)
    {
        _rooms.AddOrUpdate(roomName,
            _ => new HashSet<string> { connectionId },
            (_, currentHashSet) =>
            {
                lock (currentHashSet)
                {
                    currentHashSet.Add(connectionId);
                }
                return currentHashSet;
            }
        );
    }

    public void ConnectionLeft(string roomName, string connectionId)
    {
        if (_rooms.TryGetValue(roomName, out var currentHashSet))
        {
            lock (currentHashSet)
            {
                currentHashSet.Remove(connectionId);
            }
        }
    }

    public void RemoveConnectionFully(string connectionId)
    {
        foreach (var room in _rooms)
        {
            lock (room.Value)
            {
                room.Value.Remove(connectionId);
            }
        }
    }

    public int GetOnlineCountInRoom(string roomName)
    {
        if (_rooms.TryGetValue(roomName, out var currentHashSet))
        {
            lock (currentHashSet)
            {
                return currentHashSet.Count;
            }
        }
        return 0;
    }  

    public bool IsUserOnlineInRoom(string roomName, string userId)
    {
        if (_rooms.TryGetValue(roomName, out var usersInRoom))
        {
            return usersInRoom.Contains(userId);
        }
        return false;
    }
}