(function (Chat) {
    'use strict';
    const { state, DEFAULT_ROOM_GUID, apiFetch, el, connection } = Chat;
    const { formatTime } = Chat.utils;
    const { storeMessage, updateStoredStatus } = Chat;
    const { renderMessageRow, scrollToBottom, updateSidebarPreview, markStatus } = Chat;

    let lastTypedTime = 0;
    let typingTimeout = null;

    async function sendMessage(textOverride = null, messageType = "text") {
        if (!el.messageInput || !el.messagesArea || !state.currentGroupId) return;
        const isDeveloperChannel = state.currentGroupId === DEFAULT_ROOM_GUID;
        if (isDeveloperChannel && !state.isDeveloperAdmin) return;

        const text = textOverride ?? el.messageInput.value.trim();
        if (!text) return;

        if (messageType === "text" && text.length > 1000) {
            alert(`⚠️ Сообщение слишком длинное! Максимум 1000 символов (сейчас ${text.length}).`);
            return;
        }

        const roomKey = state.currentGroupId;
        const currentTime = formatTime();
        const messageId = crypto.randomUUID();
        const message = {
            id: messageId,
            text,
            sender: state.myUserName,
            time: currentTime,
            type: messageType,
            mine: true,
            senderId: state.myUserId,
            avatarUrl: state.myAvatarUrl,
            status: "..."
        };

        storeMessage(roomKey, message);
        if (roomKey === state.currentGroupId) {
            el.messagesArea.appendChild(renderMessageRow(message));
            scrollToBottom();
        }
        updateSidebarPreview(roomKey, messageType === "text" ? text : (messageType === "image" ? "📷 Фото" : "🎥 Видео"), currentTime);

        if (connection.state !== signalR.HubConnectionState.Connected) {
            markStatus(messageId, "⚠️", "#ff4d6d");
            updateStoredStatus(roomKey, messageId, "⚠️");
            if (messageType === "text") {
                alert("Нет подключения к чату. Текст сообщения оставлен в поле ввода.");
            }
            return;
        }

        try {
            let accepted;
            if (isDeveloperChannel) {
                accepted = await connection.invoke("PublishDeveloperMessage", messageId, text);
            } else {
                accepted = await connection.invoke("SendMessageToGroup", {
                    messageId,
                    communityName: roomKey,
                    text,
                    messageType
                });
            }
            if (accepted === false) {
                markStatus(messageId, "⚠️", "#ff4d6d");
                updateStoredStatus(roomKey, messageId, "⚠️");
                if (messageType === "text" && !isDeveloperChannel) {
                    alert("Сообщение отклонено сервером. Текст оставлен в поле ввода.");
                }
                return;
            }
            if (messageType === "text" && el.messageInput.value.trim() === text) {
                el.messageInput.value = "";
                el.messageInput.focus();
            }
        } catch (error) {
            console.error("Ошибка отправки сообщения:", error);
            markStatus(messageId, "⚠️", "#ff4d6d");
            updateStoredStatus(roomKey, messageId, "⚠️");
            if (messageType === "text") {
                alert("Не удалось отправить сообщение. Текст оставлен в поле ввода.");
            }
        }
    }

    const emojiList = [
        "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "🙃", "😉", "😊", "😇", "🥰", "😍", "🤩",
        "😘", "😗", "😚", "😋", "😛", "😜", "🤪", "😝", "🤑", "🤗", "🤭", "🫢", "🫣", "🤫", "🤔", "🫡",
        "🤐", "🤨", "😐", "😑", "😶", "🫥", "😏", "😒", "🙄", "😬", "🤥", "😌", "😔", "😪", "🤤", "😴",
        "😷", "🤒", "🤕", "🤢", "🤮", "🥵", "🥶", "🥴", "😵", "🤯", "🥳", "🥸", "😎", "🤓", "🧐", "😕",
        "😟", "🙁", "☹️", "😮", "😯", "😲", "😳", "🥺", "😦", "😧", "😨", "😰", "😥", "😢", "😭", "😱",
        "😖", "😣", "😞", "😓", "😩", "😫", "🥱", "😤", "😡", "😠", "🤬", "😈", "👿", "💀", "☠️", "💩",
        "🤡", "👻", "👽", "🤖", "🎃", "😺", "😸", "😹", "😻", "😼", "😽", "🙀", "😿", "😾", "👍", "👎",
        "👏", "🙌", "👐", "🤲", "🤝", "🙏", "✍️", "💪", "🦾", "🫶", "❤️", "🧡", "💛", "💚", "💙", "💜",
        "🖤", "🤍", "🤎", "💔", "❣️", "💕", "💞", "💓", "💗", "💖", "💘", "💝", "🔥", "✨", "⭐", "🌟",
        "🎉", "🎊", "🎁", "🎈", "🚀", "💯", "✅", "❌", "⚡", "💡", "🎮", "🏆", "🥇", "🥈", "🥉", "⚽",
        "🏀", "🎯", "🎵", "🎶", "☕", "🍕", "🍔", "🍟", "🍰", "🍩", "🌈", "☀️", "🌙", "☁️", "🌧️", "❄️"
    ];

    async function uploadAndSendFiles(files) {
        const roomKey = state.currentGroupId;
        if (!roomKey || state.currentGroupName === DEFAULT_ROOM_GUID) return;

        for (const file of files) {
            if (file.size > 10 * 1024 * 1024) {
                alert(`Файл «${file.name}» больше 10 МБ и не будет отправлен.`);
                continue;
            }
            if (!file.type.startsWith('image/') && !file.type.startsWith('video/')) {
                alert(`Файл «${file.name}» не является фото или видео.`);
                continue;
            }

            const formData = new FormData();
            formData.append('file', file);
            try {
                const response = await apiFetch(`/api/chat/UploadAttachment?communityId=${encodeURIComponent(roomKey)}`, {
                    method: 'POST',
                    body: formData
                });
                const result = await response.json().catch(() => ({}));
                if (!response.ok || !result.url || !result.messageType) {
                    throw new Error(result.error || `Не удалось загрузить файл (${response.status}).`);
                }
                if (roomKey !== state.currentGroupId) {
                    throw new Error("Вы переключили чат во время загрузки. Файл не отправлен.");
                }
                await sendMessage(result.url, result.messageType);
            } catch (error) {
                console.error("Ошибка отправки файла:", error);
                alert(error.message || "Не удалось отправить файл.");
            }
        }
    }

    function initSend() {
        if (el.sendBtn) el.sendBtn.addEventListener('click', () => sendMessage());

        if (el.emojiPicker) {
            emojiList.forEach(emoji => {
                const button = document.createElement('button');
                button.type = 'button';
                button.textContent = emoji;
                button.setAttribute('aria-label', emoji);
                button.addEventListener('click', () => {
                    if (!el.messageInput || el.messageInput.disabled) return;
                    const start = el.messageInput.selectionStart ?? el.messageInput.value.length;
                    const end = el.messageInput.selectionEnd ?? start;
                    el.messageInput.setRangeText(emoji, start, end, 'end');
                    el.messageInput.focus();
                    el.emojiPicker.classList.add('hidden');
                    el.emojiBtn?.setAttribute('aria-expanded', 'false');
                });
                el.emojiPicker.appendChild(button);
            });
        }

        if (el.emojiBtn && el.emojiPicker) {
            el.emojiBtn.addEventListener('click', () => {
                const isOpening = el.emojiPicker.classList.contains('hidden');
                el.emojiPicker.classList.toggle('hidden', !isOpening);
                el.emojiBtn.setAttribute('aria-expanded', String(isOpening));
            });
            document.addEventListener('click', event => {
                if (!el.emojiPicker.contains(event.target) && event.target !== el.emojiBtn) {
                    el.emojiPicker.classList.add('hidden');
                    el.emojiBtn.setAttribute('aria-expanded', 'false');
                }
            });
        }

        if (el.attachBtn && el.fileInput) {
            el.attachBtn.addEventListener('click', () => el.fileInput.click());
            el.fileInput.addEventListener('change', async () => {
                const files = Array.from(el.fileInput.files || []);
                el.fileInput.value = "";
                await uploadAndSendFiles(files);
            });
        }

        if (el.messageInput) {
            el.messageInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
            });
            el.messageInput.addEventListener('input', () => {
                if (!state.currentGroupName || state.currentGroupName === DEFAULT_ROOM_GUID) return;
                if (connection.state === signalR.HubConnectionState.Connected && Date.now() - lastTypedTime > 1000) {
                    lastTypedTime = Date.now();
                    connection.invoke("SendTypingStatus", state.currentGroupId, true).catch(error => console.error("Не удалось отправить статус набора текста:", error));
                }
                clearTimeout(typingTimeout);
                typingTimeout = setTimeout(() => {
                    if (connection.state === signalR.HubConnectionState.Connected && state.currentGroupId) {
                        connection.invoke("SendTypingStatus", state.currentGroupId, false).catch(error => console.error("Не удалось сбросить статус набора текста:", error));
                    }
                }, 1500);
            });
        }
    }

    Chat.initSend = initSend;
})(window.EduGameChat);