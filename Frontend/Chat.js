window.addEventListener('DOMContentLoaded', () => {
    const Chat = window.EduGameChat;

    Chat.initDom();
    Chat.initStorage();
    Chat.initNotifications();
    Chat.initConnection();
    Chat.initUi();
    Chat.initMessageSearch();
    Chat.initCommunitySearch();
    Chat.initRooms();
    Chat.initSend();
    Chat.initSocketEvents();
    Chat.initMembers();
    Chat.initCreateCommunity();
    Chat.initInvite();

    Chat.loadUnreadState();
    void Chat.startConnection();
});