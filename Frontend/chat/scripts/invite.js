(function (Chat) {
    'use strict';
    const { state, apiFetch, el } = Chat;
    const { escapeHtml, userProfileUrl } = Chat.utils;

    let inviteTab = 'friends';
    let nickTimer = null;

    function setInviteTab(tabName) {
        inviteTab = tabName;
        document.querySelectorAll('.search-tab').forEach(tab => tab.classList.toggle('active', tab.dataset.tab === tabName));
        if (el.searchUserInput) {
            el.searchUserInput.value = '';
            el.searchUserInput.placeholder = tabName === 'nickname' ? 'Введите никнейм...' : 'Выберите из друзей';
        }
        if (el.searchUserResults) el.searchUserResults.innerHTML = '';
    }

    function normalizeInviteUser(raw) {
        return {
            userId: raw.userId ?? raw.UserId ?? raw.friendId ?? raw.senderId ?? raw.receiverId ?? raw.id ?? raw.Id ?? null,
            nickname: raw.userName ?? raw.UserName ?? raw.nickname ?? raw.nickName ?? raw.name ?? 'Неизвестный',
            avatarUrl: raw.avatarUrl ?? raw.AvatarUrl ?? raw.avatar ?? null,
            isFriend: Boolean(raw.isFriend ?? raw.IsFriend ?? false),
            requestPending: Boolean(raw.requestPending ?? raw.RequestPending ?? false),
            isOnline: Boolean(raw.isOnline ?? raw.online ?? false)
        };
    }

    async function loadFriendsForInvite() {
        if (!el.searchUserResults) return;
        el.searchUserResults.innerHTML = '<div style="color:var(--text-muted);padding:20px;text-align:center;">Загрузка друзей...</div>';
        try {
            const res = await apiFetch('/api/friend/GetMyFriends');
            if (!res.ok) throw 0;
            const data = await res.json();
            const arr = Array.isArray(data) ? data : (data.value ?? data.Value ?? []);
            const inRoom = new Set(state.currentMembers.map(m => m.userName));
            renderInviteList(arr.map(normalizeInviteUser).filter(u => u.userId && !inRoom.has(u.nickname)));
        } catch {
            el.searchUserResults.innerHTML = '<div style="color:#ff4d6d;padding:20px;text-align:center;">Не удалось загрузить друзей</div>';
        }
    }

    async function searchUsersByNickname(q) {
        try {
            const res = await apiFetch(`/api/friend/SearchUsers?query=${encodeURIComponent(q)}&limit=10`);
            if (!res.ok) throw 0;
            const data = await res.json();
            const arr = Array.isArray(data) ? data : (data.value ?? data.Value ?? []);
            renderInviteList(arr.map(normalizeInviteUser));
        } catch {
            el.searchUserResults.innerHTML = '<div style="color:var(--text-muted);padding:20px;text-align:center;">Поиск по нику недоступен — используй вкладку «Из друзей»</div>';
        }
    }

    function renderInviteList(users) {
        if (!el.searchUserResults) return;
        if (!users.length) {
            el.searchUserResults.innerHTML = '<div style="color:var(--text-muted);padding:20px;text-align:center;">Никого не нашли 🤷</div>';
            return;
        }
        el.searchUserResults.innerHTML = '';
        users.forEach(u => {
            const item = document.createElement('div');
            item.className = 'search-user-item';
            item.innerHTML = `
                <a class="search-user-avatar" href="${escapeHtml(userProfileUrl(u.userId))}" aria-label="Открыть профиль ${escapeHtml(u.nickname)}">
                    ${u.avatarUrl
                        ? `<img src="${escapeHtml(u.avatarUrl)}" alt="">`
                        : `<span style="width:100%;height:100%;display:flex;align-items:center;justify-content:center;background:var(--accent-gradient);color:#fff;font-weight:800;">${escapeHtml(u.nickname.substring(0, 2).toUpperCase())}</span>`}
                </a>
                <div class="search-user-info">
                    <a class="search-user-name" href="${escapeHtml(userProfileUrl(u.userId))}">${escapeHtml(u.nickname)}</a>
                    <span class="search-user-status">${u.isOnline ? '🟢 В сети' : '⚫ Не в сети'}</span>
                </div>
                <button class="add-user-btn" ${inviteTab === 'nickname' && (u.isFriend || u.requestPending) ? 'disabled' : ''}>${inviteTab === 'friends' ? '+ Пригласить' : u.isFriend ? 'Уже друзья' : u.requestPending ? 'Заявка уже есть' : 'Добавить в друзья'}</button>`;
            item.querySelector('.add-user-btn').addEventListener('click', (e) =>
                inviteTab === 'friends' ? inviteFriendToCommunity(u, e.currentTarget) : sendFriendRequest(u, e.currentTarget));
            el.searchUserResults.appendChild(item);
        });
    }

    async function sendFriendRequest(user, btn) {
        btn.disabled = true;
        btn.textContent = 'Отправка...';
        try {
            const response = await apiFetch(`/api/friend/SendFriendRequest?receiverId=${encodeURIComponent(user.userId)}`, { method: 'POST' });
            const payload = await response.json().catch(() => null);
            if (!response.ok) throw new Error(payload?.error || payload?.title || 'Не удалось отправить заявку.');
            btn.textContent = 'Заявка отправлена';
            btn.classList.add('sent');
        } catch (error) {
            btn.disabled = false;
            btn.textContent = 'Добавить в друзья';
            alert(error.message || 'Не удалось отправить заявку.');
        }
    }

    async function inviteFriendToCommunity(user, btn) {
        btn.disabled = true; btn.innerText = 'Зовём...';
        try {
            const response = await apiFetch('/api/chat/InviteFriendToCommunity', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ receiverId: user.userId, communityId: state.currentGroupId, communityName: state.currentGroupName })
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok) throw new Error(payload?.error || payload?.title || 'Не удалось отправить приглашение.');
            btn.innerText = '✓ Приглашение отправлено'; btn.classList.add('sent');
        } catch (error) {
            btn.innerText = 'Ошибка'; btn.disabled = false;
            alert(error.message || 'Не удалось отправить приглашение.');
        }
    }

    function initInvite() {
        if (el.addPlayerBtn && el.searchUserModal) {
            el.addPlayerBtn.addEventListener('click', () => {
                setInviteTab('friends');
                el.searchUserModal.classList.add('open');
                loadFriendsForInvite();
            });
        }
        if (el.closeSearchUserBtn) {
            el.closeSearchUserBtn.addEventListener('click', () => el.searchUserModal?.classList.remove('open'));
        }

        document.querySelectorAll('.search-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                setInviteTab(tab.dataset.tab);
                if (inviteTab === 'friends') loadFriendsForInvite();
            });
        });

        if (el.searchUserInput) {
            el.searchUserInput.addEventListener('input', (e) => {
                if (inviteTab !== 'nickname') return;
                const q = e.target.value.trim();
                clearTimeout(nickTimer);
                if (q.length < 2) { if (el.searchUserResults) el.searchUserResults.innerHTML = ''; return; }
                nickTimer = setTimeout(() => searchUsersByNickname(q), 300);
            });
        }
    }

    Object.assign(Chat, { sendFriendRequest, initInvite });
})(window.EduGameChat);