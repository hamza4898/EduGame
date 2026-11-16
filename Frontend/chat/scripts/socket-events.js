(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, el, connection } = Chat;
    const { onlineCache, unreadCount, unreadMessages, lastReadAt, unreadSyncCursors } = state;
    const { escapeHtml, formatTime, parseServerDate } = Chat.utils;
    const { getStoredMessages, storeMessage, updateStoredStatus, updateUnreadBadge, saveUnreadState } = Chat;
    const { renderMessageRow, scrollToBottom, updateSidebarPreview, updateMessageStatus, updateStoredMessageTime, markStatus, setOnlineStatus } = Chat;
    const { playNotificationSound } = Chat;

    let typingStatusTimeout = null;

    function markCurrentRoomAsRead() {
        if (!state.currentGroupId || document.visibilityState !== "visible" || !document.hasFocus()) return;
        const pending = unreadMessages[state.currentGroupId] || [];
        unreadCount[state.currentGroupId] = 0;
        unreadMessages[state.currentGroupId] = [];
        lastReadAt[state.currentGroupId] = new Date().toISOString();
        unreadSyncCursors.set(state.currentGroupId, lastReadAt[state.currentGroupId]);
        updateUnreadBadge(state.currentGroupId);
        saveUnreadState();

        if (connection.state === signalR.HubConnectionState.Connected) {
            pending.forEach(messageId => {
                connection.invoke("ConfirmDelivery", messageId, state.currentGroupId)
                    .then(() => connection.invoke("MarkMessageAsRead", messageId, state.currentGroupId))
                    .catch(error => console.error("Не удалось отметить сообщение прочитанным:", error));
            });
        }
    }

    function initSocketEvents() {
        connection.on("DeveloperMessageRejected", (messageId, errorMessage) => {
            markStatus(messageId, "⚠️", "#ff4d6d");
            updateStoredStatus(DEFAULT_ROOM_GUID, messageId, "⚠️");
            alert(errorMessage || "Объявление не удалось отправить.");
        });

        connection.on("MessageStatusChanged", (messageId, statusText, statusColor, roomKey, sentAt) => {
            updateMessageStatus(messageId, statusText, statusColor);
            updateStoredStatus(roomKey || state.currentGroupId, messageId, statusText);
            if (sentAt) updateStoredMessageTime(roomKey || state.currentGroupId, messageId, sentAt);
        });

        connection.on("RoomAccessDenied", roomKey => {
            console.warn("Сервер отклонил вход в сообщество:", roomKey);
            if (roomKey === state.currentGroupId) {
                alert("У вас нет доступа к этому сообществу.");
                if (state.previousGroupId) connection.invoke("JoinRoom", state.previousGroupId).catch(console.error);
            }
        });

        connection.on("ReceiveMessage", (messageId, groupName, text, userName, messageType, senderConnectionId, senderUserId, _communityName, sentAt, senderAvatarUrl) => {
            if (senderConnectionId === connection.connectionId) {
                updateMessageStatus(messageId, "✓", "rgba(255,255,255,0.6)");
                updateStoredStatus(groupName, messageId, "✓");
                if (sentAt) updateStoredMessageTime(groupName, messageId, sentAt);
                return;
            }

            if (getStoredMessages(groupName).some(message => message.id === messageId)) return;

            const currentTime = formatTime(parseServerDate(sentAt) || new Date());
            const previewText = messageType === "image" ? "📷 Фото" : messageType === "video" ? "🎥 Видео" : text;
            const isMine = Boolean(state.myUserId && senderUserId === state.myUserId);
            storeMessage(groupName, {
                id: messageId,
                text,
                sender: userName || "Участник",
                senderId: senderUserId,
                avatarUrl: senderAvatarUrl,
                time: currentTime,
                type: messageType || "text",
                mine: isMine
            });
            updateSidebarPreview(groupName, `${userName || "Участник"}: ${previewText}`, currentTime);

            const isChatVisible = groupName === state.currentGroupId
                && document.visibilityState === "visible"
                && document.hasFocus();

            if (groupName === state.currentGroupId && el.messagesArea) {
                el.messagesArea.appendChild(renderMessageRow({
                    id: messageId,
                    text,
                    sender: userName,
                    senderId: senderUserId,
                    avatarUrl: senderAvatarUrl,
                    time: currentTime,
                    type: messageType || "text",
                    mine: isMine
                }));
                scrollToBottom();

                if (isChatVisible && connection.state === signalR.HubConnectionState.Connected) {
                    connection.invoke("ConfirmDelivery", messageId, groupName).catch(() => {});
                    setTimeout(() => connection.invoke("MarkMessageAsRead", messageId, groupName).catch(() => {}), 1000);
                }
            }

            if (!isChatVisible
                && !isMine
                && !(unreadMessages[groupName] || []).includes(messageId)) {
                unreadCount[groupName] = (unreadCount[groupName] || 0) + 1;
                if (!unreadMessages[groupName]) unreadMessages[groupName] = [];
                unreadMessages[groupName].push(messageId);

                updateUnreadBadge(groupName);
                updateSidebarPreview(groupName, `${userName || "Участник"}: ${previewText}`, currentTime);
                saveUnreadState();

                playNotificationSound();
            } else if (isChatVisible) {
                lastReadAt[groupName] = new Date().toISOString();
                saveUnreadState();
            }
        });

        connection.on("UserTyping", (groupName, userName, isTyping) => {
            if (groupName !== state.currentGroupId) return;
            if (el.status) el.status.textContent = isTyping ? `${userName} печатает...` : "в сети";
            clearTimeout(typingStatusTimeout);
            if (isTyping) {
                typingStatusTimeout = setTimeout(() => setOnlineStatus(state.currentGroupId), 2500);
            }
        });

        connection.on("ReceiveSystemNotification", (roomGuid, notificationText, actionType, currentOnline) => {
            onlineCache[roomGuid] = currentOnline || 0;
            if (roomGuid !== state.currentGroupId) return;

            setOnlineStatus(roomGuid);
            if (el.lobbyOnlineCount) {
                const c = currentOnline || 0;
                el.lobbyOnlineCount.innerText = c > 0 ? `${c} онлайн` : "0 онлайн";
            }

            if (state.isSwitchingTab) { state.isSwitchingTab = false; return; }

            if (el.messagesArea && notificationText && notificationText.trim() !== "") {
                const isJoin = actionType === "join";
                const sysRow = document.createElement('div');
                sysRow.className = "date-divider animate__animated animate__fadeIn animate__fast";
                sysRow.style.cssText = "margin:16px 0;text-align:center;";
                sysRow.innerHTML = `
                    <span style="background:${isJoin ? "rgba(74,222,128,0.15)" : "rgba(255,255,255,0.05)"};color:${isJoin ? "#4ade80" : "#718096"};padding:4px 14px;border-radius:20px;font-size:11px;font-weight:bold;letter-spacing:0.5px;border:1px solid ${isJoin ? "rgba(74,222,128,0.2)" : "rgba(255,255,255,0.05)"};text-shadow:${isJoin ? "0 0 8px rgba(74,222,128,0.4)" : "none"};">
                        ${escapeHtml(notificationText)}
                    </span>`;
                el.messagesArea.appendChild(sysRow);
                scrollToBottom();
                storeMessage(roomGuid, { id: crypto.randomUUID(), sys: true, text: notificationText });
            }
        });

        connection.on("OnlineCountUpdated", (roomGuid, currentOnline) => {
            onlineCache[roomGuid] = currentOnline || 0;
            if (roomGuid !== state.currentGroupId) return;

            state.isSwitchingTab = false;
            setOnlineStatus(roomGuid);
            if (el.lobbyOnlineCount) {
                el.lobbyOnlineCount.innerText = currentOnline > 0 ? `${currentOnline} онлайн` : "0 онлайн";
            }
        });

        document.addEventListener("visibilitychange", markCurrentRoomAsRead);
        window.addEventListener("focus", markCurrentRoomAsRead);
    }

    Chat.initSocketEvents = initSocketEvents;
})(window.EduGameChat);