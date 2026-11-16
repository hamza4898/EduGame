(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, el } = Chat;

    function closeMenu() {
        if (el.sidebarMenu) el.sidebarMenu.classList.remove('open');
        if (el.menuOverlay) el.menuOverlay.classList.remove('active');
    }

    function initUi() {
        if (el.menuBtn && el.sidebarMenu && el.menuOverlay && el.menuCloseBtn) {
            el.menuBtn.addEventListener('click', () => {
                el.sidebarMenu.classList.add('open');
                el.menuOverlay.classList.add('active');
            });
            el.menuCloseBtn.addEventListener('click', closeMenu);
            el.menuOverlay.addEventListener('click', closeMenu);
        }

        if (el.roomInfoBtn && el.clanLobbyPanel && el.clanLobbyCloseBtn) {
            el.roomInfoBtn.addEventListener('click', () => {
                el.clanLobbyPanel.classList.add('open');
                if (state.currentGroupId && state.currentGroupId !== DEFAULT_ROOM_GUID) {
                    if (state.membersLoadedFor !== state.currentGroupId && state.membersLoadingFor !== state.currentGroupId) {
                        void Chat.loadMembers(state.currentGroupId);
                    }
                } else if (el.playerListTable) {
                    el.playerListTable.innerHTML =
                        '<div class="message-search-empty" style="padding:20px;text-align:center;">Список участников доступен в сообществах.</div>';
                }
            });
            el.clanLobbyCloseBtn.addEventListener('click', () => el.clanLobbyPanel.classList.remove('open'));
            el.clanLobbyPanel.addEventListener('click', (e) => {
                if (e.target === el.clanLobbyPanel) el.clanLobbyPanel.classList.remove('open');
            });
        }

        if (el.mobileBackBtn) {
            el.mobileBackBtn.addEventListener('click', () => {
                if (el.chatContentWrapper) el.chatContentWrapper.classList.add('hidden');
                if (el.chatPlaceholder) el.chatPlaceholder.classList.remove('hidden');
            });
        }

        if (el.messagesArea) {
            el.messagesArea.addEventListener('scroll', () => {
                const threshold = 100;
                state.isUserScrolledUp = el.messagesArea.scrollTop + el.messagesArea.clientHeight < el.messagesArea.scrollHeight - threshold;
            });
        }
    }

    Object.assign(Chat, { closeMenu, initUi });
})(window.EduGameChat);