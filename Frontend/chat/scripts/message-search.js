(function (Chat) {
    'use strict';
    const { state, apiFetch, el } = Chat;
    const { roomMeta } = state;
    const { escapeHtml, formatTime, parseServerDate } = Chat.utils;
    const { renderMessageRow, renderMessagesForCurrentRoom } = Chat;

    async function runMessageSearch(query) {
        if (!el.messageSearchResults) return;
        const searchText = query.trim();
        if (!searchText) {
            el.messageSearchResults.innerHTML = '<div class="message-search-empty">Введите текст сообщения.</div>';
            return;
        }

        const roomId = state.currentGroupId;
        const communityId = roomMeta[roomId]?.communityId;
        if (!communityId || !/^[0-9a-f-]{36}$/i.test(String(communityId))) {
            el.messageSearchResults.innerHTML = '<div class="message-search-empty">Поиск истории доступен в сообществах.</div>';
            return;
        }

        el.messageSearchResults.innerHTML = '<div class="message-search-empty">Ищем сообщения...</div>';
        try {
            const response = await apiFetch(
                `/api/chat/SearchMessages?communityId=${encodeURIComponent(communityId)}&query=${encodeURIComponent(searchText)}`
            );
            const payload = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(payload?.error || payload?.title || 'Не удалось выполнить поиск.');
            }
            if (roomId !== state.currentGroupId || !el.messageSearchResults) return;

            const results = Array.isArray(payload) ? payload : (payload?.value ?? payload?.Value ?? []);
            el.messageSearchResults.replaceChildren();
            if (!results.length) {
                el.messageSearchResults.innerHTML = '<div class="message-search-empty">Ничего не найдено.</div>';
                return;
            }

            results.forEach(message => {
                const messageId = message.messageId ?? message.MessageId;
                const text = message.text ?? message.Text ?? '';
                const senderName = message.senderName ?? message.SenderName ?? 'Участник';
                const sentAt = parseServerDate(message.sentAt ?? message.SentAt);
                const result = document.createElement('button');
                result.type = 'button';
                result.className = 'message-search-result';
                result.innerHTML = `<strong>${escapeHtml(senderName)} · ${escapeHtml(sentAt ? sentAt.toLocaleString() : '')}</strong><span class="message-search-result-message">${escapeHtml(text)}</span>`;
                result.addEventListener('click', () => {
                    if (roomId !== state.currentGroupId || !el.messagesArea) return;
                    let row = Array.from(el.messagesArea.querySelectorAll('.message-row'))
                        .find(item => item.dataset.messageId === messageId);
                    if (!row) {
                        const senderId = message.senderId ?? message.SenderId;
                        row = renderMessageRow({
                            id: messageId,
                            text,
                            sender: senderName,
                            senderId: message.senderId ?? message.SenderId,
                            avatarUrl: message.senderAvatarUrl ?? message.SenderAvatarUrl,
                            time: sentAt ? formatTime(sentAt) : '',
                            type: message.messageType ?? message.MessageType ?? 'text',
                            mine: Boolean(state.myUserId && senderId === state.myUserId)
                        }, false);
                        row.classList.add('message-search-result-row');
                        el.messagesArea.appendChild(row);
                    }
                    row.classList.add('message-search-highlight');
                    row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    setTimeout(() => row?.classList.remove('message-search-highlight'), 2500);
                });
                el.messageSearchResults.appendChild(result);
            });
        } catch (error) {
            console.error('Ошибка поиска сообщений:', error);
            el.messageSearchResults.innerHTML = `<div class="message-search-empty">${escapeHtml(error.message || 'Ошибка поиска сообщений.')}</div>`;
        }
    }

    function initMessageSearch() {
        if (el.searchBtn && el.messageSearchPanel) {
            el.searchBtn.addEventListener('click', () => {
                const isOpening = el.messageSearchPanel.classList.contains('hidden');
                el.messageSearchPanel.classList.toggle('hidden', !isOpening);
                el.searchBtn.setAttribute('aria-expanded', String(isOpening));
                if (isOpening) el.messageSearchInput?.focus();
            });
        }
        el.messageSearchForm?.addEventListener('submit', event => {
            event.preventDefault();
            runMessageSearch(el.messageSearchInput?.value || '');
        });
        el.messageSearchClose?.addEventListener('click', () => {
            el.messageSearchPanel?.classList.add('hidden');
            el.searchBtn?.setAttribute('aria-expanded', 'false');
            if (el.messageSearchResults) el.messageSearchResults.replaceChildren();
            renderMessagesForCurrentRoom();
        });
    }

    Chat.initMessageSearch = initMessageSearch;
})(window.EduGameChat);