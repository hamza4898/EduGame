(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, el } = Chat;
    const { roomMeta, onlineCache } = state;
    const { $, escapeHtml, formatTime, parseServerDate, pluralize, isAttachmentUrl, userProfileUrl } = Chat.utils;
    const { getStoredMessages, saveStoredMessages, storeMessage } = Chat;

    function roomAvatarHTML(roomKey) {
        const item = Array.from(el.chatList?.querySelectorAll('.chat-item[data-id]') ?? [])
            .find(chatItem => chatItem.dataset.id === String(roomKey));
        return item?.querySelector('.chat-avatar')?.innerHTML
            || '<div class="avatar-placeholder"><img src="images/hamzaBro3.png" alt="A"></div>';
    }

    function currentUserAvatarHTML() {
        return state.myAvatarUrl
            ? `<div class="avatar-placeholder"><img src="${escapeHtml(state.myAvatarUrl)}" alt="${escapeHtml(state.myUserName || 'Мой аватар')}"></div>`
            : '<div class="avatar-placeholder">U</div>';
    }

    function renderMessageRow(msg, animate = true) {
        const row = document.createElement('div');

        if (msg.sys) {
            row.className = 'date-divider' + (animate ? ' animate__animated animate__fadeIn animate__fast' : '');
            row.innerHTML = `<span style="background:rgba(74,222,128,0.15);color:#4ade80;padding:4px 14px;border-radius:20px;font-size:11px;font-weight:bold;border:1px solid rgba(74,222,128,0.2);">${escapeHtml(msg.text)}</span>`;
            return row;
        }

        row.className = `message-row ${msg.mine ? 'outbound' : 'inbound'}` + (animate ? ' animate__animated animate__fadeInUp animate__fast' : '');
        row.dataset.messageId = msg.id || '';
        row.style.cssText = `display:flex;justify-content:${msg.mine ? 'flex-end' : 'flex-start'};align-items:flex-end;gap:10px;margin:12px 0;width:100%;`;

        const avatar = msg.avatarUrl
            ? `<div class="avatar-placeholder"><img src="${escapeHtml(msg.avatarUrl)}" alt="${escapeHtml(msg.sender || 'Аватар пользователя')}"></div>`
            : msg.mine
                ? currentUserAvatarHTML()
                : '<div class="avatar-placeholder"><img src="images/hamzaBro3.png" alt="Аватар участника"></div>';
        const profileHref = userProfileUrl(msg.senderId);
        const avatarLink = msg.senderId
            ? `<a href="${escapeHtml(profileHref)}" aria-label="Открыть профиль ${escapeHtml(msg.sender || 'пользователя')}" style="display:block;width:100%;height:100%;">${avatar}</a>`
            : avatar;

        const senderHtml = msg.mine ? '' :
            `<a class="sender-name" href="${escapeHtml(profileHref)}" style="font-size:12px;color:#ff4d6d;font-weight:800;display:block;margin-bottom:4px;user-select:none;text-decoration:none;">${escapeHtml(msg.sender)}</a>`;

        const statusHtml = msg.mine
            ? `<span class="message-status" id="${escapeHtml(msg.id || '')}" style="font-size:11px;color:rgba(255,255,255,0.6);font-weight:bold;">${escapeHtml(msg.status || '✓')}</span>` : '';

        let contentHtml = `<p class="message-text" style="margin:0;color:#fff;font-size:14px;word-break:break-word;">${escapeHtml(msg.text)}</p>`;
        if (isAttachmentUrl(msg.text) && msg.type === 'image') {
            contentHtml = `<a class="message-media-link" href="${escapeHtml(msg.text)}" target="_blank" rel="noopener"><img class="message-media" src="${escapeHtml(msg.text)}" alt="Изображение" loading="lazy"></a>`;
        } else if (isAttachmentUrl(msg.text) && msg.type === 'video') {
            contentHtml = `<video class="message-media" src="${escapeHtml(msg.text)}" controls preload="metadata"></video>`;
        }

        row.innerHTML = `
            ${msg.mine ? '' : `<div class="chat-avatar" style="width:38px;height:38px;flex-shrink:0;border-radius:50%;overflow:hidden;border:1px solid var(--border);">${avatarLink}</div>`}
            <div class="message-bubble" style="padding:10px 14px;padding-right:${msg.mine ? 75 : 55}px;background:${msg.mine ? 'linear-gradient(135deg,#ff4d6d,#c9184a)' : 'rgba(255,255,255,0.07)'};border-radius:${msg.mine ? '14px 14px 4px 14px' : '14px 14px 14px 4px'};position:relative;width:fit-content;max-width:70%;min-width:110px;">
                ${senderHtml}
                ${contentHtml}
                <div style="position:absolute;bottom:6px;right:12px;display:flex;align-items:center;gap:5px;user-select:none;">
                    <span class="message-time" style="font-size:10px;color:rgba(255,255,255,0.5);">${escapeHtml(msg.time || '')}</span>
                    ${statusHtml}
                </div>
            </div>
            ${msg.mine ? `<div class="chat-avatar" style="width:38px;height:38px;flex-shrink:0;border-radius:50%;overflow:hidden;border:1px solid var(--border);">${msg.senderId ? avatarLink : avatar}</div>` : ''}`;
        return row;
    }

    function renderMessagesForCurrentRoom() {
        if (!el.messagesArea) return;
        let msgs = getStoredMessages(state.currentGroupId);
        if (msgs.length === 0 && state.currentGroupId !== DEFAULT_ROOM_GUID) {
            const meta = roomMeta[state.currentGroupId] || {};
            const previewText = meta.isCreator
                ? "Вы создали это сообщество ✨ Собирайте пати и общайтесь! 🔥"
                : "Вы вошли в это сообщество ✨";
            storeMessage(state.currentGroupId, { id: 'welcome-' + state.currentGroupId, text: previewText, sender: state.currentGroupName, time: formatTime(), mine: false });
            msgs = getStoredMessages(state.currentGroupId);
        }

        el.messagesArea.replaceChildren();
        if (msgs.length > 0) {
            const dateDivider = document.createElement('div');
            dateDivider.className = 'date-divider';
            dateDivider.innerHTML = '<span>Сегодня</span>';
            el.messagesArea.appendChild(dateDivider);
        }
        msgs.forEach(m => el.messagesArea.appendChild(renderMessageRow(m, false)));
        scrollToBottom();
    }

    function scrollToBottom() {
        if (!el.messagesArea) return;
        if (!state.isUserScrolledUp) {
            el.messagesArea.scrollTop = el.messagesArea.scrollHeight;
        }
    }

    function updateSidebarPreview(roomKey, text, time) {
        const item = el.chatList?.querySelector(`.chat-item[data-id="${roomKey}"]`);
        if (!item) return;
        const p = item.querySelector('.chat-preview'); if (p) p.innerText = text;
        const t = item.querySelector('.chat-time');  if (t) t.innerText = time;
    }

    function markStatus(messageId, text, color) {
        const s = $(messageId);
        if (!s) return;
        s.innerText = text;
        s.style.color = color;
        s.style.animation = "none";
    }

    function updateMessageStatus(messageId, textStatus, colorStatus) {
        const s = $(messageId);
        if (!s) return;
        s.innerText = textStatus;
        if (colorStatus) s.style.color = colorStatus;
        if (textStatus === "✓✓✓") {
            s.style.color = "#4ade80";
            s.style.fontWeight = "800";
            s.style.textShadow = "0 0 10px rgba(74,222,128,0.7), 0 0 20px rgba(74,222,128,0.4)";
            s.classList.add('animate__animated', 'animate__bounceIn', 'animate__faster');
        }
        s.style.animation = "none";
    }

    function updateStoredMessageTime(roomKey, messageId, sentAt) {
        const sentDate = parseServerDate(sentAt);
        if (!sentDate) return;

        const storedMessages = getStoredMessages(roomKey);
        const message = storedMessages.find(item => item.id === messageId);
        if (message) {
            message.time = formatTime(sentDate);
            saveStoredMessages(roomKey, storedMessages);
        }

        const statusElement = $(messageId);
        const timeElement = statusElement?.closest('.message-row')?.querySelector('.message-time');
        if (timeElement) timeElement.textContent = formatTime(sentDate);
    }

    function setOnlineStatus(roomGuid) {
        if (!el.status) return;
        const isSystemRoom = roomGuid === DEFAULT_ROOM_GUID;
        el.status.style.display = isSystemRoom ? 'none' : '';
        if (el.lobbyOnlineCount) el.lobbyOnlineCount.style.display = isSystemRoom ? 'none' : '';
        if (isSystemRoom) return;
        const count = onlineCache[roomGuid];
        if (count === undefined) {
            el.status.innerText = "в сети";
            el.status.style.color = "var(--text-muted)";
            el.status.style.textShadow = "none";
            return;
        }
        const word = pluralize(count, ["участник онлайн", "участника онлайн", "участников онлайн"]);
        el.status.innerText = count > 0 ? `${count} ${word}` : "участников: 0";
        el.status.style.color = count > 0 ? "#4ade80" : "var(--text-muted)";
        el.status.style.textShadow = count > 0 ? "0 0 8px rgba(74, 222, 128, 0.3)" : "none";
    }

    Object.assign(Chat, {
        roomAvatarHTML, renderMessageRow, renderMessagesForCurrentRoom, scrollToBottom,
        updateSidebarPreview, markStatus, updateMessageStatus, updateStoredMessageTime, setOnlineStatus
    });
})(window.EduGameChat);