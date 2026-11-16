namespace EduGame.Application.Common.Interfaces;

public interface IRoomTracker
{
    void ConnectionJoined(string roomName, string connectionId);

    void ConnectionLeft(string roomName, string connectionId);

    void RemoveConnectionFully(string connectionId);
    
    int GetOnlineCountInRoom(string roomName);
}