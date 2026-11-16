(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, apiFetch } = Chat;
    const { roomMeta, lastReadAt, unreadCount, unreadMessages, historyLoadedRooms, unreadSyncCursors } = state;
    const { formatTime, parseServerDate } = Chat.utils;
    const { getStoredMessages, saveStoredMessages, saveUnreadState, updateUnreadBadge } = Chat;
    const { updateSidebarPreview, renderMessagesForCurrentRoom } = Chat;

    const historyRequests = new Map();
    const historySyncCursors = new Map();
    const unreadSyncRequests = new Map();

    function tryLoadHistoryFromServer(roomKey, forceSync = false) {
        if (historyLoadedRooms.has(roomKey) && !forceSync) return Promise.resolve();
        if (historyRequests.has(roomKey)) return historyRequests.get(roomKey);

        const request = (async () => {
        const meta = roomMeta[roomKey] || {};
        const cid = meta.communityId;
        const isSystemRoom = roomKey === DEFAULT_ROOM_GUID;
        if (!isSystemRoom && (!cid || !/^[0-9a-f-]{36}$/i.test(String(cid)))) return;

        try {
            const syncStartedAt = new Date().toISOString();
            const syncSince = historySyncCursors.get(roomKey);
            const endpoint = isSystemRoom
                ? '/api/chat/GetDeveloperMessages?Offset=0&Limit=50'
                : `/api/chat/GetMessages?${new URLSearchParams({
                    communityId: cid,
                    offset: "0",
                    limit: "50",
                    ...(syncSince ? { since: syncSince } : {})
                })}`;
            const res = await apiFetch(endpoint);
            if (!res.ok) throw new Error(`Не удалось загрузить историю чата (${res.status}).`);
            const payload = await res.json();
            const arr = Array.isArray(payload) ? payload : (payload.value ?? payload.Value ?? []);
            if (!Array.isArray(arr)) {
                throw new Error("Сервер вернул некорректный список сообщений.");
            }
            historyLoadedRooms.add(roomKey);
            historySyncCursors.set(roomKey, syncStartedAt);
            if (arr.length === 0) return;

            let addedUnreadMessages = false;
            if (!lastReadAt[roomKey]) {
                lastReadAt[roomKey] = new Date().toISOString();
            }
            const storedMessages = getStoredMessages(roomKey);
            const knownMessageIds = new Set(storedMessages.map(message => message.id));
            const newMessages = [];
            const isChatVisible = roomKey === state.currentGroupId
                && document.visibilityState === "visible"
                && document.hasFocus();
            const lastRead = new Date(lastReadAt[roomKey]);
            arr.forEach(m => {
                const isMine = (state.myUserId && m.senderId === state.myUserId) || m.senderName === state.myUserName;
                const message = {
                    id: m.messageId ?? String(crypto.randomUUID()),
                    text: m.text,
                    sender: m.senderName ?? m.userName ?? "Участник",
                    senderId: m.senderId ?? m.SenderId,
                    avatarUrl: m.senderAvatarUrl ?? m.SenderAvatarUrl,
                    time: formatTime(parseServerDate(m.sentAt) || new Date()),
                    type: m.messageType ?? 'text',
                    mine: isMine
                };
                if (knownMessageIds.has(message.id)) return;
                knownMessageIds.add(message.id);
                newMessages.push(message);

                const sentAt = parseServerDate(m.sentAt);
                if (!isMine && !isChatVisible && sentAt && sentAt > lastRead) {
                    unreadCount[roomKey] = (unreadCount[roomKey] || 0) + 1;
                    if (!unreadMessages[roomKey]) unreadMessages[roomKey] = [];
                    unreadMessages[roomKey].push(message.id);
                    addedUnreadMessages = true;
                }
            });
            if (newMessages.length > 0) {
                saveStoredMessages(roomKey, [...storedMessages, ...newMessages]);
            }

            const latestMessage = arr[arr.length - 1];
            if (latestMessage) {
                const preview = latestMessage.messageType === "image"
                    ? "📷 Фото"
                    : latestMessage.messageType === "video"
                        ? "🎥 Видео"
                        : latestMessage.text;
                updateSidebarPreview(
                    roomKey,
                    `${latestMessage.senderName || "Участник"}: ${preview}`,
                    formatTime(parseServerDate(latestMessage.sentAt) || new Date())
                );
            }

            if (addedUnreadMessages) {
                updateUnreadBadge(roomKey);
            }
            saveUnreadState();
            if (roomKey === state.currentGroupId) renderMessagesForCurrentRoom();
        } catch (error) {
            console.error("Ошибка загрузки истории чата:", error);
        }
        })();

        historyRequests.set(roomKey, request);
        request.finally(() => historyRequests.delete(roomKey));
        return request;
    }

    function syncUnreadMessagesFromServer(roomKey) {
        if (unreadSyncRequests.has(roomKey)) return unreadSyncRequests.get(roomKey);

        const lastRead = lastReadAt[roomKey];
        const meta = roomMeta[roomKey] || {};
        const communityId = meta.communityId;
        if (!lastRead || !communityId || !/^[0-9a-f-]{36}$/i.test(String(communityId))) {
            return Promise.resolve();
        }

        const request = (async () => {
            const syncSince = unreadSyncCursors.get(roomKey) || lastRead;
            const syncStartedAt = new Date().toISOString();
            const query = new URLSearchParams({
                communityId,
                offset: "0",
                limit: "50",
                since: syncSince
            });
            const response = await apiFetch(`/api/chat/GetMessages?${query}`);
            if (!response.ok) {
                throw new Error(`Не удалось синхронизировать сообщения (${response.status}).`);
            }

            const payload = await response.json();
            const page = Array.isArray(payload) ? payload : (payload.value ?? payload.Value);
            if (!Array.isArray(page)) {
                throw new Error("Сервер вернул некорректный список сообщений.");
            }
            unreadSyncCursors.set(roomKey, syncStartedAt);
            historyLoadedRooms.add(roomKey);
            historySyncCursors.set(roomKey, syncStartedAt);

            page.sort((a, b) =>
                (parseServerDate(a.sentAt)?.getTime() ?? 0) - (parseServerDate(b.sentAt)?.getTime() ?? 0));
            let addedUnreadMessage = false;
            const storedMessages = getStoredMessages(roomKey);
            const knownMessageIds = new Set(storedMessages.map(item => item.id));
            const newMessages = [];
            for (const message of page) {
                const isMine = (state.myUserId && message.senderId === state.myUserId)
                    || message.senderName === state.myUserName;
                const item = {
                    id: message.messageId ?? String(crypto.randomUUID()),
                    text: message.text,
                    sender: message.senderName ?? "Участник",
                    senderId: message.senderId ?? message.SenderId,
                    avatarUrl: message.senderAvatarUrl ?? message.SenderAvatarUrl,
                    time: formatTime(parseServerDate(message.sentAt) || new Date()),
                    type: message.messageType ?? "text",
                    mine: isMine
                };
                if (knownMessageIds.has(item.id)) continue;
                knownMessageIds.add(item.id);
                newMessages.push(item);

                const sentAt = parseServerDate(message.sentAt);
                if (!isMine
                    && roomKey !== state.currentGroupId
                    && sentAt
                    && !(unreadMessages[roomKey] || []).includes(item.id)
                    && sentAt > new Date(lastReadAt[roomKey])) {
                    unreadCount[roomKey] = (unreadCount[roomKey] || 0) + 1;
                    if (!unreadMessages[roomKey]) unreadMessages[roomKey] = [];
                    unreadMessages[roomKey].push(item.id);
                    addedUnreadMessage = true;
                }
            }
            if (newMessages.length > 0) {
                saveStoredMessages(roomKey, [...storedMessages, ...newMessages]);
            }

            const latestMessage = page[page.length - 1];
            if (latestMessage) {
                const preview = latestMessage.messageType === "image"
                    ? "📷 Фото"
                    : latestMessage.messageType === "video"
                        ? "🎥 Видео"
                        : latestMessage.text;
                updateSidebarPreview(
                    roomKey,
                    `${latestMessage.senderName || "Участник"}: ${preview}`,
                    formatTime(parseServerDate(latestMessage.sentAt) || new Date())
                );
            }

            if (addedUnreadMessage) updateUnreadBadge(roomKey);
            saveUnreadState();
        })().catch(error => {
            console.error(`Не удалось восстановить пропущенные сообщения сообщества ${roomKey}:`, error);
        });

        unreadSyncRequests.set(roomKey, request);
        request.finally(() => unreadSyncRequests.delete(roomKey));
        return request;
    }

    Object.assign(Chat, { tryLoadHistoryFromServer, syncUnreadMessagesFromServer });
})(window.EduGameChat);