(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, MEMBERS_LIMIT, LS, el, connection } = Chat;
    const { unreadCount, unreadMessages } = state;
    const { escapeHtml, userProfileUrl } = Chat.utils;
    const { saveUnreadState } = Chat;

    async function loadMembers(communityGuid) {
        try {
            if (state.membersLoadingFor === communityGuid) return;
            state.membersOffset = 0;
            state.currentMembers = [];
            state.membersLoadedFor = null;
            state.membersHasMore = false;
            if (el.playerListTable) {
                el.playerListTable.innerHTML =
                    '<div style="color:rgba(255,255,255,0.5);padding:20px;text-align:center;">Загружаем участников...</div>';
                el.playerListTable.scrollTop = 0;
            }

            if (!communityGuid || communityGuid === DEFAULT_ROOM_GUID) {
                if (el.playerListTable) {
                    el.playerListTable.innerHTML =
                        '<div class="message-search-empty" style="padding:20px;text-align:center;">Список участников доступен в сообществах.</div>';
                }
                return;
            }
            if (communityGuid !== state.currentGroupId) return;
            if (connection.state !== signalR.HubConnectionState.Connected) {
                if (el.playerListTable) {
                    el.playerListTable.innerHTML =
                        '<div style="color:#ff4d6d;padding:20px;text-align:center;">Нет подключения к чату. Повторите попытку через несколько секунд.</div>';
                }
                return;
            }
            state.membersLoadingFor = communityGuid;
            await connection.invoke("GetCommunityMembers", communityGuid, 0, MEMBERS_LIMIT);
        } catch (err) {
            state.membersLoadingFor = null;
            console.error("Ошибка загрузки участников:", err);
            if (el.playerListTable) {
                el.playerListTable.innerHTML = '<div style="color:#ff4d6d;padding:20px;text-align:center;">⚠️ Ошибка загрузки участников</div>';
            }
        }
    }

    function renderMembersPortion() {
        if (!el.playerListTable) return;
        const portion = state.currentMembers.slice(state.membersOffset, state.membersOffset + MEMBERS_LIMIT);
        state.membersOffset += MEMBERS_LIMIT;

        if (portion.length === 0 && el.playerListTable.innerHTML === '') {
            el.playerListTable.innerHTML = '<div style="color:rgba(255,255,255,0.3);padding:20px;text-align:center;font-size:13px;">В группе еще нет участников 👥</div>';
            return;
        }

        portion.forEach((member, index) => {
            const userId = member.userId ?? member.UserId ?? '';
            const userName = member.userName ?? member.UserName ?? 'Участник';
            const avatarUrl = member.avatarUrl ?? member.AvatarUrl ?? '';
            const isFriend = Boolean(member.isFriend ?? member.IsFriend);
            const requestPending = Boolean(member.requestPending ?? member.RequestPending);
            const row = document.createElement('div');
            row.className = 'player-row animate__animated animate__fadeIn';
            row.style.animationDelay = `${index * 0.03}s`;
            row.style.flexDirection = 'column';
            row.style.alignItems = 'stretch';
            row.style.gap = '0';

            let roleClass = 'member', roleText = 'УЧАСТНИК', itemRole = 0;
            const roleString = (member.userRole ?? member.UserRole ?? '').toString().toLowerCase();
            if (roleString === 'leader' || roleString === '2') { roleClass = 'leader'; roleText = 'СОЗДАТЕЛЬ'; itemRole = 2; }
            else if (roleString === 'officer' || roleString === '1') { roleClass = 'officer'; roleText = 'АДМИН'; itemRole = 1; }

            const statusClass = (member.isOnline ?? member.IsOnline) ? 'online' : 'offline';
            let avatarHTML = `<img src="${escapeHtml(avatarUrl)}" alt="">`;
            if (!avatarUrl || avatarUrl === 'null') {
                const initials = userName ? escapeHtml(userName.substring(0, 2).toUpperCase()) : '??';
                avatarHTML = `<div class="avatar-letter-placeholder">${initials}</div>`;
            }

            const isItMe = userId === state.myUserId || (!userId && userName === state.myUserName);
            const roleActions = [];
            if (state.myCurrentRoleInRoom > itemRole && !isItMe) {
                if (state.myCurrentRoleInRoom === 2 && itemRole === 0) roleActions.push('<button class="clan-action-btn promote-btn" type="button">⚡ Сделать админом</button>');
                if (state.myCurrentRoleInRoom === 2 && itemRole === 1) roleActions.push('<button class="clan-action-btn demote-btn" type="button">🛡️ Снять админа</button>');
                roleActions.push('<button class="clan-action-btn kick-btn" type="button">❌ Исключить</button>');
            }
            if (!isItMe) {
                const friendLabel = isFriend ? 'Уже друзья' : requestPending ? 'Заявка отправлена' : '👥 В друзья';
                roleActions.unshift(`<button class="clan-action-btn friend-request-btn" type="button" ${isFriend || requestPending || !userId ? 'disabled' : ''}>${friendLabel}</button>`);
            }
            const actionsHTML = roleActions.length
                ? `<div class="player-actions-panel">${roleActions.join('')}</div>`
                : '';

            row.innerHTML = `
                <div class="player-row-main" style="display:flex;align-items:center;gap:14px;width:100%;">
                    <div class="player-avatar-wrapper">${avatarHTML}<div class="player-status-dot ${statusClass}"></div></div>
                    <div class="player-meta">
                        <span class="player-nickname">${isItMe ? `${escapeHtml(userName)} (Ты)` : escapeHtml(userName)}</span>
                        <span class="player-role ${roleClass}">${roleText}</span>
                    </div>
                </div>
                ${actionsHTML}`;

            const memberMain = row.querySelector('.player-row-main');
            if (memberMain && userId) {
                memberMain.style.cursor = 'pointer';
                memberMain.title = `Открыть профиль ${userName}`;
                memberMain.addEventListener('click', () => {
                    window.location.href = userProfileUrl(userId);
                });
            }

            const kickBtn = row.querySelector('.kick-btn');
            if (kickBtn) kickBtn.addEventListener('click', () => {
                if (confirm(`Исключить игрока ${userName}?`)) {
                    connection.invoke("KickPlayerFromCommunity", state.currentGroupId, userId)
                        .catch(err => {
                            console.error("Ошибка исключения:", err);
                            alert(err.message || "Не удалось исключить участника.");
                        });
                }
            });
            const promoteBtn = row.querySelector('.promote-btn');
            if (promoteBtn) promoteBtn.addEventListener('click', () => {
                if (confirm(`Назначить ${userName} админом?`)) {
                    connection.invoke("ChangePlayerRole", state.currentGroupId, userId, 1)
                        .catch(err => {
                            console.error("Ошибка смены роли:", err);
                            alert(err.message || "Не удалось назначить администратора.");
                        });
                }
            });
            const demoteBtn = row.querySelector('.demote-btn');
            if (demoteBtn) demoteBtn.addEventListener('click', () => {
                if (confirm(`Снять админа с ${userName}?`)) {
                    connection.invoke("ChangePlayerRole", state.currentGroupId, userId, 0)
                        .catch(err => {
                            console.error("Ошибка смены роли:", err);
                            alert(err.message || "Не удалось снять роль администратора.");
                        });
                }
            });
            const friendBtn = row.querySelector('.friend-request-btn');
            if (friendBtn) friendBtn.addEventListener('click', async () => {
                friendBtn.disabled = true;
                await Chat.sendFriendRequest({ userId, nickname: userName }, friendBtn);
                if (friendBtn.classList.contains('sent')) {
                    member.requestPending = true;
                    friendBtn.textContent = 'Заявка отправлена';
                } else {
                    friendBtn.disabled = false;
                }
            });

            el.playerListTable.appendChild(row);
        });
    }

    function initMembers() {
        if (el.playerListTable) {
            el.playerListTable.addEventListener('scroll', () => {
                const t = el.playerListTable;
                if (t.scrollTop + t.clientHeight >= t.scrollHeight - 15 && state.currentGroupId) {
                    if (!state.membersHasMore
                        || state.membersPageLoading
                        || state.membersLoadedFor !== state.currentGroupId
                        || connection.state !== signalR.HubConnectionState.Connected) return;
                    state.membersPageLoading = true;
                    connection.invoke("GetCommunityMembers", state.currentGroupId, state.membersOffset, MEMBERS_LIMIT)
                        .catch(err => {
                            state.membersPageLoading = false;
                            console.error("Ошибка пагинации участников:", err);
                        });
                }
            });
        }

        connection.on("CommunityMembersChanged", communityId => {
            if (String(communityId).toLowerCase() === String(state.currentGroupId).toLowerCase()) {
                state.membersLoadedFor = null;
                loadMembers(state.currentGroupId);
            }
        });

        connection.on("CommunityActionError", message => {
            console.error("Ошибка управления сообществом:", message);
            alert(message || "Не удалось выполнить действие.");
        });

        connection.on("RemovedFromCommunity", async communityId => {
            const communityKey = String(communityId);
            try {
                await connection.invoke("LeaveCommunity", communityId);
            } catch (error) {
                console.error("Не удалось покинуть группу сообщества:", error);
            }

            const item = Array.from(el.chatList?.querySelectorAll('.chat-item') ?? [])
                .find(chatItem => String(chatItem.dataset.id).toLowerCase() === communityKey.toLowerCase());
            item?.remove();
            localStorage.removeItem(LS.messages(communityKey));
            delete unreadCount[communityKey];
            delete unreadMessages[communityKey];
            saveUnreadState();

            if (String(state.currentGroupId).toLowerCase() === communityKey.toLowerCase()) {
                state.currentGroupId = null;
                state.currentGroupName = DEFAULT_ROOM_GUID;
                state.previousGroupId = null;
                localStorage.removeItem(LS.activeRoom);
                el.clanLobbyPanel?.classList.remove('open');
                el.chatContentWrapper?.classList.add('hidden');
                el.chatPlaceholder?.classList.remove('hidden');
                alert("Вас исключили из сообщества.");
            }
        });

        connection.on("ReceiveMembersPortion", (data) => {
            if (data.communityId && String(data.communityId).toLowerCase() !== String(state.currentGroupId).toLowerCase()) return;
            if (state.membersLoadingFor && state.membersLoadingFor !== state.currentGroupId) return;
            const serverMembers = data.members || [];
            state.myCurrentRoleInRoom = data.myRole ?? 0;

            if (data.offset === 0) {
                state.currentMembers = [];
                state.membersOffset = 0;
                state.membersLoadedFor = state.currentGroupId;
                state.membersLoadingFor = null;
                state.membersPageLoading = false;
                state.membersHasMore = serverMembers.length === MEMBERS_LIMIT;
                if (el.playerListTable) el.playerListTable.innerHTML = '';
            } else {
                state.membersPageLoading = false;
                state.membersHasMore = serverMembers.length === MEMBERS_LIMIT;
            }

            if (el.addPlayerBtn) el.addPlayerBtn.style.display = state.myCurrentRoleInRoom >= 1 ? 'block' : 'none';

            if (serverMembers.length > 0) {
                state.currentMembers = state.currentMembers.concat(serverMembers);
                renderMembersPortion();
            } else if (state.currentMembers.length === 0 && el.playerListTable && el.playerListTable.innerHTML === '') {
                el.playerListTable.innerHTML = '<div style="color:rgba(255,255,255,0.3);padding:20px;text-align:center;font-size:13px;">В группе еще нет участников 👥</div>';
            }
        });

        connection.on("GetMembersError", (errorMessage) => {
            state.membersLoadingFor = null;
            state.membersPageLoading = false;
            state.membersHasMore = false;
            console.error(`❌ Ошибка доступа к дашборду: ${errorMessage}`);
            if (el.playerListTable) {
                el.playerListTable.innerHTML = `<div style="color:#ff4d6d;padding:20px;text-align:center;font-size:13px;font-weight:600;">${escapeHtml(errorMessage)}</div>`;
            }
        });
    }

    Object.assign(Chat, { loadMembers, initMembers });
})(window.EduGameChat);