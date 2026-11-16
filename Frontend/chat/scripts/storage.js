(function (Chat) {
    'use strict';
    const { state, LS, lsGet, lsSet, DEFAULT_ROOM_GUID, el } = Chat;
    const { unreadCount, unreadMessages, lastReadAt } = state;

    const inMemoryMessages = new Map([[DEFAULT_ROOM_GUID, []]]);
    const LS_UNREAD = () => `edugame_unread_${state.myUserId}`;

    function loadUnreadState() {
        if (!state.myUserId) return;
        const data = lsGet(LS_UNREAD(), {});
        Object.assign(unreadCount, data.counts || {});
        Object.assign(unreadMessages, data.messages || {});
        Object.assign(lastReadAt, data.lastReadAt || {});
        delete unreadCount[DEFAULT_ROOM_GUID];
        delete unreadMessages[DEFAULT_ROOM_GUID];
        saveUnreadState();
        Object.keys(unreadCount).forEach(guid => updateUnreadBadge(guid));
    }

    function saveUnreadState() {
        if (!state.myUserId) return;
        lsSet(LS_UNREAD(), { counts: unreadCount, messages: unreadMessages, lastReadAt });
    }

    function updateUnreadBadge(roomGuid) {
        const item = el.chatList?.querySelector(`.chat-item[data-id="${roomGuid}"]`);
        if (!item) return;

        let badge = item.querySelector('.unread-badge');
        const count = unreadCount[roomGuid] || 0;

        if (count > 0) {
            item.classList.add('unread');
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'unread-badge';
                item.appendChild(badge);
            }
            badge.textContent = count > 99 ? '99+' : count;
        } else {
            item.classList.remove('unread');
            if (badge) badge.remove();
        }
    }

    function getStoredMessages(roomKey) {
        if (roomKey === DEFAULT_ROOM_GUID) return inMemoryMessages.get(roomKey) || [];
        const messages = lsGet(LS.messages(roomKey), []);
        return Array.isArray(messages) ? messages : [];
    }

    function saveStoredMessages(roomKey, messages) {
        const boundedMessages = messages.length > 300 ? messages.slice(-300) : messages;
        if (roomKey === DEFAULT_ROOM_GUID) {
            inMemoryMessages.set(roomKey, boundedMessages);
        } else {
            lsSet(LS.messages(roomKey), boundedMessages);
        }
    }

    function storeMessage(roomKey, msg) {
        if (!roomKey) return;
        const arr = getStoredMessages(roomKey);
        if (msg.id && arr.some(m => m.id === msg.id)) return;
        arr.push(msg);
        saveStoredMessages(roomKey, arr);
    }

    function updateStoredStatus(roomKey, messageId, statusText) {
        const arr = getStoredMessages(roomKey);
        const m = arr.find(x => x.id === messageId);
        if (m) {
            m.status = statusText;
            saveStoredMessages(roomKey, arr);
        }
    }

    function initStorage() {
        localStorage.removeItem(LS.messages(DEFAULT_ROOM_GUID));
    }

    Object.assign(Chat, {
        initStorage, loadUnreadState, saveUnreadState, updateUnreadBadge,
        getStoredMessages, saveStoredMessages, storeMessage, updateStoredStatus
    });
})(window.EduGameChat);