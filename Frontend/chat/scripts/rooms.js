(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, LS, lsGet, lsSet, el, connection } = Chat;
    const { roomMeta, unreadCount, unreadMessages, lastReadAt, historyLoadedRooms, unreadSyncCursors } = state;
    const { escapeHtml, formatTime } = Chat.utils;
    const { updateUnreadBadge, saveUnreadState } = Chat;
    const { roomAvatarHTML, setOnlineStatus, renderMessagesForCurrentRoom } = Chat;
    const { tryLoadHistoryFromServer, syncUnreadMessagesFromServer } = Chat;

    let hasSyncedUnreadOnInitialLoad = false;

    function addRoomToSidebar(roomName, avatarUrl, isCreator = false, roomGuid, description = null) {
        if (!el.chatList) return;

        const already = Array.from(el.chatList.querySelectorAll('.chat-item'))
            .some(item => item.getAttribute('data-id') === String(roomGuid));
        if (already) return;

        roomMeta[roomGuid] = {
            ...roomMeta[roomGuid],
            name: roomName,
            avatarUrl,
            description: description ?? roomMeta[roomGuid]?.description ?? null,
            isCreator,
            communityId: roomName === DEFAULT_ROOM_GUID ? null : roomGuid
        };

        const chatItem = document.createElement('div');
        chatItem.className = 'chat-item animate__animated animate__fadeInLeft animate__fast';
        chatItem.setAttribute('data-creator', isCreator ? 'true' : 'false');
        if (roomGuid) chatItem.setAttribute('data-id', roomGuid);
        chatItem.setAttribute('data-name', roomName);

        const previewLabel = isCreator ? "Вы создали это сообщество ✨" : "Вы вошли в это сообщество ✨";

        chatItem.innerHTML = `
            <div class="chat-avatar">
                <div class="avatar-placeholder"><img src="${escapeHtml(avatarUrl) || 'images/hamzaBro3.png'}" alt="Avatar"></div>
            </div>
            <div class="chat-info">
                <div class="chat-top">
                    <span class="chat-name">${escapeHtml(roomName)}</span>
                    <span class="chat-time">${formatTime()}</span>
                </div>
                <div class="chat-bottom"><span class="chat-preview">${previewLabel}</span></div>
            </div>`;

        chatItem.addEventListener('click', (e) => {
            if (e.target.closest('.chat-avatar')) return;
            el.chatList.querySelectorAll('.chat-item').forEach(i => i.classList.remove('active'));
            chatItem.classList.add('active');

            selectCommunity(roomGuid, roomName, isCreator);

            if (roomGuid && roomGuid !== DEFAULT_ROOM_GUID) {
                state.currentMembers = [];
                state.membersOffset = 0;
                Chat.loadMembers(roomGuid);
            } else if (roomGuid === DEFAULT_ROOM_GUID && el.playerListTable) {
                el.playerListTable.innerHTML = '<div class="message-search-empty" style="padding:20px;text-align:center;">Список участников доступен в сообществах.</div>';
                if (el.addPlayerBtn) el.addPlayerBtn.style.display = 'none';
            }
        });

        el.chatList.appendChild(chatItem);
        updateUnreadBadge(roomGuid);
    }

    function selectCommunity(roomGuid, roomName, isCreator = false) {
        if (roomGuid === state.currentGroupId) return;
        if (el.chatPlaceholder) el.chatPlaceholder.classList.add('hidden');
        if (el.chatContentWrapper) {
            el.chatContentWrapper.classList.remove('hidden');
            el.chatContentWrapper.className = "chat-content-wrapper animate__animated animate__fadeIn animate__fast";
        }

        state.currentGroupId = roomGuid;
        state.currentGroupName = roomName;
        const isSystemRoom = roomGuid === DEFAULT_ROOM_GUID;
        if (el.roomInfoBtn) el.roomInfoBtn.hidden = isSystemRoom;
        if (isSystemRoom) el.clanLobbyPanel?.classList.remove('open');
        roomMeta[roomGuid] = {
            ...roomMeta[roomGuid],
            name: roomName,
            isCreator,
            communityId: roomName === DEFAULT_ROOM_GUID ? null : roomGuid
        };
        state.isSwitchingTab = true;
        lsSet(LS.activeRoom, roomGuid);

        if (unreadCount[roomGuid] && unreadCount[roomGuid] > 0) {
            const pending = unreadMessages[roomGuid] || [];
            unreadCount[roomGuid] = 0;
            unreadMessages[roomGuid] = [];
            updateUnreadBadge(roomGuid);
            saveUnreadState();

            if (connection.state === signalR.HubConnectionState.Connected) {
                pending.forEach(mid => {
                    connection.invoke("MarkMessageAsRead", mid, roomGuid).catch(() => {});
                });
            }
        } else {
            updateUnreadBadge(roomGuid);
        }
        lastReadAt[roomGuid] = new Date().toISOString();
        unreadSyncCursors.set(roomGuid, lastReadAt[roomGuid]);
        saveUnreadState();

        if (connection.state === signalR.HubConnectionState.Connected) {
            if (state.previousGroupId && state.previousGroupId !== roomGuid) {
                connection.invoke("LeaveRoom", state.previousGroupId, state.myUserName)
                    .then(() => connection.invoke("JoinRoom", roomGuid))
                    .then(() => { state.previousGroupId = roomGuid; state.previousGroupName = roomName; })
                    .catch(err => console.error("Ошибка переключения комнат:", err));
            } else if (state.previousGroupId !== roomGuid) {
                connection.invoke("JoinRoom", roomGuid)
                    .then(() => { state.previousGroupId = roomGuid; state.previousGroupName = roomName; })
                    .catch(err => console.error("Ошибка входа в комнату:", err));
            }
        }

        let leftAvatarHTML = roomAvatarHTML(roomGuid);
        let previewText = "Вы успешно вошли в сообщество! ✨";
        let messageTime = formatTime();

        if (el.chatList) {
            const activeItem = Array.from(el.chatList.querySelectorAll('.chat-item')).find(item => {
                const nameEl = item.querySelector('.chat-name');
                return nameEl && nameEl.innerText.includes(roomName);
            });
            if (activeItem) {
                const previewEl = activeItem.querySelector('.chat-preview');
                if (previewEl) previewText = previewEl.innerText.trim();
                const timeEl = activeItem.querySelector('.chat-time');
                if (timeEl) messageTime = timeEl.innerText.trim();
            }
        }

        if (el.headerAvatar) el.headerAvatar.innerHTML = leftAvatarHTML;

        if (el.chatHeaderName) {
            const tickHTML = roomName === "EduGame" ? ` <img src="images/галочка.png" alt="✓" class="verified-tick">` : "";
            el.chatHeaderName.innerHTML = `${escapeHtml(roomName)}${tickHTML}`;
        }

        const readOnly = isSystemRoom && !state.isDeveloperAdmin;
        if (el.messageInput) {
            el.messageInput.disabled = readOnly;
            el.messageInput.placeholder = readOnly
                ? "Новостной канал — сообщения доступны только для чтения"
                : isSystemRoom
                    ? "Написать объявление всем пользователям..."
                    : "Написать сообщение...";
        }
        [el.sendBtn, el.emojiBtn].forEach(button => {
            if (button) button.disabled = readOnly;
        });
        if (el.attachBtn) el.attachBtn.disabled = readOnly || isSystemRoom;
        if (el.fileInput) el.fileInput.disabled = readOnly || isSystemRoom;
        if (el.emojiPicker) el.emojiPicker.classList.add('hidden');
        if (el.messageSearchResults) el.messageSearchResults.replaceChildren();
        if (el.messageSearchInput) el.messageSearchInput.value = '';

        setOnlineStatus(roomGuid);
        renderMessagesForCurrentRoom();
        tryLoadHistoryFromServer(roomGuid, historyLoadedRooms.has(roomGuid));

        if (el.clanNameNeon) el.clanNameNeon.innerText = roomName;
        if (el.clanBigAvatar) {
            const temp = document.createElement('div');
            temp.innerHTML = leftAvatarHTML;
            const img = temp.querySelector('img');
            if (img) el.clanBigAvatar.src = img.src;
        }
        if (el.clanDesc) {
            el.clanDesc.innerText = isSystemRoom
                ? "Главный хаб EduGame."
                : roomMeta[roomGuid].description?.trim() || "Описание сообщества не добавлено.";
        }
    }

    function initRooms() {
        const defaultChatItem = document.querySelector('.chat-list .chat-item');
        if (defaultChatItem) {
            defaultChatItem.setAttribute('data-id', DEFAULT_ROOM_GUID);
            roomMeta[DEFAULT_ROOM_GUID] = { name: "EduGame", isCreator: false };

            defaultChatItem.addEventListener('click', () => {
                document.querySelectorAll('.chat-list .chat-item').forEach(i => i.classList.remove('active'));
                defaultChatItem.classList.add('active');
                selectCommunity(DEFAULT_ROOM_GUID, "EduGame");
            });
        }
    }

    async function loadUserCommunities() {
        try {
            const response = await apiFetchCommunities();
            const payload = await response.json();
            const rawCommunities = Array.isArray(payload) ? payload : (payload.value ?? payload.Value);
            if (!Array.isArray(rawCommunities)) {
                throw new Error("Сервер вернул некорректный список сообществ.");
            }

            const communities = rawCommunities
                .map(community => ({
                    id: community.communityId ?? community.CommunityId,
                    name: community.communityName ?? community.CommunityName,
                    avatarUrl: community.avatarUrl ?? community.AvatarUrl ?? null,
                    description: community.description ?? community.Description ?? null
                }))
                .filter(community =>
                    typeof community.id === "string"
                    && /^[0-9a-f-]{36}$/i.test(community.id)
                    && typeof community.name === "string"
                    && community.name.trim().length > 0);

            const validIds = new Set(communities.map(community => community.id.toLowerCase()));
            const previousCommunities = lsGet('edugame_communities', []);
            if (Array.isArray(previousCommunities)) {
                previousCommunities.forEach(community => {
                    const id = String(community.id ?? "");
                    if (id && !validIds.has(id.toLowerCase()) && id !== DEFAULT_ROOM_GUID) {
                        localStorage.removeItem(LS.messages(id));
                        delete unreadCount[id];
                        delete unreadMessages[id];
                    }
                });
            }
            localStorage.removeItem('edugame_communities');
            saveUnreadState();

            el.chatList?.querySelectorAll('.chat-item[data-id]').forEach(item => {
                const id = item.dataset.id;
                if (id !== DEFAULT_ROOM_GUID && !validIds.has(String(id).toLowerCase())) {
                    item.remove();
                }
            });
            Object.keys(roomMeta).forEach(id => {
                if (id !== DEFAULT_ROOM_GUID && !validIds.has(id.toLowerCase())) delete roomMeta[id];
            });

            communities.forEach(community => {
                addRoomToSidebar(community.name, community.avatarUrl, false, community.id, community.description);
            });

            const savedActive = lsGet(LS.activeRoom);
            const activeItem = Array.from(el.chatList?.querySelectorAll('.chat-item') ?? [])
                .find(item => item.dataset.id === savedActive);
            if (activeItem && savedActive !== state.currentGroupId) {
                activeItem.click();
            } else if (savedActive) {
                const activeRoomStillExists = Boolean(activeItem);
                if (!activeRoomStillExists) localStorage.removeItem(LS.activeRoom);
            }

            if (!hasSyncedUnreadOnInitialLoad) {
                hasSyncedUnreadOnInitialLoad = true;
                const inactiveCommunities = communities.filter(community => community.id !== state.currentGroupId);
                void (async () => {
                    for (let i = 0; i < inactiveCommunities.length; i += 4) {
                        await Promise.all(inactiveCommunities.slice(i, i + 4).map(community =>
                            syncUnreadMessagesFromServer(community.id)
                        ));
                    }
                })().catch(error => {
                    console.error("Не удалось синхронизировать непрочитанные сообщения:", error);
                });
            }
        } catch (error) {
            localStorage.removeItem('edugame_communities');
            el.chatList?.querySelectorAll('.chat-item').forEach(item => {
                if (item.dataset.id !== DEFAULT_ROOM_GUID) item.remove();
            });
            console.error("Ошибка синхронизации сообществ с сервером:", error);
        }
    }

    async function apiFetchCommunities() {
        const response = await Chat.apiFetch('/api/chat/GetMyCommunities');
        if (!response.ok) {
            throw new Error(`Не удалось загрузить список сообществ (${response.status}).`);
        }
        return response;
    }

    Object.assign(Chat, { addRoomToSidebar, initRooms, loadUserCommunities });
})(window.EduGameChat);