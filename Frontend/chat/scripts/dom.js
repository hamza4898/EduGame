(function (Chat) {
    'use strict';
    const { $ } = Chat.utils;

    const el = {};

    function initDom() {
        Object.assign(el, {
            searchBoxBtn: $('searchBoxBtn'), roomModal: $('roomModal'), closeModalBtn: $('closeModalBtn'),
            roomNameInput: $('roomNameInput'), joinRoomBtn: $('joinRoomBtn'), modalSearchResults: $('modalSearchResults'),
            messagesArea: $('messagesArea'), sendBtn: $('sendBtn'), messageInput: $('messageInput'),
            attachBtn: $('attachBtn'), fileInput: $('fileInput'), emojiBtn: $('emojiBtn'), emojiPicker: $('emojiPicker'),
            menuBtn: $('menuBtn'), sidebarMenu: $('sidebarMenu'), menuCloseBtn: $('menuCloseBtn'), menuOverlay: $('menuOverlay'),
            roomInfoBtn: $('roomInfoBtn'), clanLobbyPanel: $('clanLobbyPanel'), clanLobbyCloseBtn: $('clanLobbyCloseBtn'),
            clanBigAvatar: $('clanBigAvatar'), clanNameNeon: $('clanNameNeon'), clanDesc: $('clanDesc'),
            lobbyOnlineCount: $('lobbyOnlineCount'), addPlayerBtn: $('addPlayerBtn'), playerListTable: $('playerListTable'),
            createCommunityBtn: $('createCommunityBtn'), createCommunityModal: $('createCommunityModal'),
            createCommunityCloseBtn: $('createCommunityCloseBtn'), avatarUploadZone: $('avatarUploadZone'),
            communityFileAvatar: $('communityFileAvatar'), communityPreviewImg: $('communityPreviewImg'),
            sidebarPreviewAvatar: $('sidebarPreviewAvatar'), commNameInput: $('commNameInput'),
            sidebarPreviewName: $('sidebarPreviewName'), commNameHint: $('commNameHint'),
            commDescInput: $('commDescInput'), descCharCount: $('descCharCount'),
            submitCreateCommunityBtn: $('submitCreateCommunityBtn'), mobileBackBtn: $('mobileBackBtn'),
            chatPlaceholder: $('chatPlaceholder'), chatContentWrapper: $('chatContentWrapper'),
            chatHeaderName: $('chatHeaderName'), status: $('status'),
            searchBtn: $('searchBtn'), messageSearchPanel: $('messageSearchPanel'),
            messageSearchForm: $('messageSearchForm'), messageSearchInput: $('messageSearchInput'),
            messageSearchResults: $('messageSearchResults'), messageSearchClose: $('messageSearchClose'),
            searchUserModal: $('searchUserModal'), closeSearchUserBtn: $('closeSearchUserBtn'),
            searchUserInput: $('searchUserInput'), searchUserResults: $('searchUserResults'),
            chatList: document.querySelector('.chat-list'),
            headerAvatar: document.querySelector('.chat-header-left .chat-avatar'),
        });
    }

    Chat.el = el;
    Chat.initDom = initDom;
})(window.EduGameChat);