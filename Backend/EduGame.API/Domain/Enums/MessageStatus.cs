namespace EduGame.Domain.Enums;

public enum MessageStatus
{
    Pending = 0,

    SentToServer = 1,

    DeliveredToClients = 2,

    ReadByAll = 3
}