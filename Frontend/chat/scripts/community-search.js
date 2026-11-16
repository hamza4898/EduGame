(function (Chat) {
    'use strict';
    const { state, SEARCH_LIMIT, apiFetch, el, connection } = Chat;
    const { escapeHtml, pluralize } = Chat.utils;

    let selectedRoomFromSearch = null;
    let currentFilteredCommunities = [];
    let searchOffset = 0;

    function setJoinButtonState(enabled) {
        if (!el.joinRoomBtn) return;
        el.joinRoomBtn.disabled = !enabled;
        el.joinRoomBtn.style.background = enabled ? 'var(--accent-gradient)' : 'var(--glass)';
        el.joinRoomBtn.style.color = enabled ? 'white' : 'var(--text-muted)';
        el.joinRoomBtn.style.cursor = enabled ? 'pointer' : 'not-allowed';
    }

    function closeModal() {
        if (el.roomModal) el.roomModal.classList.remove('open');
        if (el.roomNameInput) el.roomNameInput.value = "";
        if (el.modalSearchResults) el.modalSearchResults.innerHTML = '';
        currentFilteredCommunities = [];
        searchOffset = 0;
        selectedRoomFromSearch = null;
        setJoinButtonState(false);
    }

    async function fetchCommunities(query, offset) {
        const response = await apiFetch(
            `/api/chat/SearchCommunities?${new URLSearchParams({ query, offset, limit: SEARCH_LIMIT })}`
        );
        return response.ok ? await response.json() : [];
    }

    function formatMembers(count) {
        const n = Number(count) || 0;
        const formatted = n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        return `${formatted} ${pluralize(n, ["участник", "участника", "участников"])}`;
    }

    function renderSearchPortion() {
        if (!el.modalSearchResults) return;
        const portion = currentFilteredCommunities.slice(searchOffset, searchOffset + SEARCH_LIMIT);
        searchOffset += SEARCH_LIMIT;

        if (portion.length === 0 && el.modalSearchResults.innerHTML === '') {
            el.modalSearchResults.innerHTML =
                '<div class="animate__animated animate__fadeIn" style="color:rgba(255,255,255,0.4);padding:16px;text-align:center;font-size:14px;">Ничего не найдено 😢</div>';
            return;
        }

        portion.forEach((room, index) => {
            const item = document.createElement('div');
            item.className = 'modal-search-item animate__animated animate__fadeIn';
            item.style.animationDelay = `${index * 0.04}s`;
            item.setAttribute('tabindex', '0');
            item.innerHTML = `
                <div class="chat-avatar" style="width:38px;height:38px;border-radius:50%;overflow:hidden;border:1px solid var(--border);">
                    <div class="avatar-placeholder" style="width:100%;height:100%;">
                        <img src="${escapeHtml(room.avatarUrl) || 'images/hamzaBro3.png'}" alt="Avatar" style="width:100%;height:100%;object-fit:cover;">
                    </div>
                </div>
                <div class="modal-search-info">
                    <span class="modal-search-name" style="font-weight:600;color:var(--text-primary);font-size:14px;">${escapeHtml(room.name)}</span>
                    <span class="modal-search-desc" style="color:var(--text-muted);font-size:12px;margin-top:2px;">${formatMembers(room.membersCount)}</span>
                </div>`;

            item.addEventListener('click', () => {
                if (el.roomNameInput) el.roomNameInput.value = room.name;
                selectedRoomFromSearch = room;
                setJoinButtonState(true);
            });

            el.modalSearchResults.appendChild(item);
        });
    }

    function initCommunitySearch() {
        if (el.searchBoxBtn && el.roomModal && el.closeModalBtn) {
            el.searchBoxBtn.addEventListener('click', () => {
                el.roomModal.classList.add('open');
                setJoinButtonState(false);
                if (el.roomNameInput) el.roomNameInput.focus();
            });
            el.closeModalBtn.addEventListener('click', closeModal);
            el.roomModal.addEventListener('click', (e) => {
                if (e.target === el.roomModal) {
                    if (el.roomNameInput) el.roomNameInput.value = "";
                    selectedRoomFromSearch = null;
                    setJoinButtonState(false);
                }
            });
        }

        if (el.roomNameInput) {
            el.roomNameInput.addEventListener('input', async (e) => {
                const query = e.target.value.trim().toLowerCase();
                if (!query) {
                    if (el.modalSearchResults) el.modalSearchResults.innerHTML = '';
                    currentFilteredCommunities = [];
                    return;
                }
                searchOffset = 0;
                if (el.modalSearchResults) { el.modalSearchResults.innerHTML = ''; el.modalSearchResults.scrollTop = 0; }
                try {
                    currentFilteredCommunities = await fetchCommunities(query, 0);
                    renderSearchPortion();
                } catch (err) { console.error("Ошибка поиска сообществ:", err); }
            });
        }

        if (el.modalSearchResults) {
            el.modalSearchResults.addEventListener('scroll', async () => {
                const s = el.modalSearchResults;
                if (s.scrollTop + s.clientHeight >= s.scrollHeight - 15) {
                    const query = el.roomNameInput ? el.roomNameInput.value.trim().toLowerCase() : "";
                    if (query) {
                        try {
                            const next = await fetchCommunities(query, searchOffset);
                            if (next.length > 0) {
                                currentFilteredCommunities = currentFilteredCommunities.concat(next);
                                renderSearchPortion();
                            }
                        } catch (err) { console.error("Ошибка пагинации поиска:", err); }
                    }
                }
            });
        }

        if (el.joinRoomBtn) {
            el.joinRoomBtn.addEventListener('click', () => {
                if (!selectedRoomFromSearch) return;
                if (connection.state === signalR.HubConnectionState.Connected) {
                    connection.invoke("JoinCommunity", selectedRoomFromSearch.id)
                        .catch(err => console.error("Ошибка JoinCommunity:", err));
                } else {
                    alert("⚠️ Нет соединения с сервером. Попробуйте позже");
                }
            });
        }

        connection.on("JoinCommunitySuccess", (roomGuid) => {
            if (selectedRoomFromSearch && String(selectedRoomFromSearch.id) === String(roomGuid)) {
                Chat.addRoomToSidebar(
                    selectedRoomFromSearch.name,
                    selectedRoomFromSearch.avatarUrl,
                    false,
                    roomGuid,
                    selectedRoomFromSearch.description ?? selectedRoomFromSearch.Description ?? null);
                state.isSwitchingTab = false;

                if (connection.state === signalR.HubConnectionState.Connected) {
                    connection.invoke("JoinRoom", roomGuid)
                        .then(() => {
                            const active = document.querySelector('.chat-item.active');
                            if (active) active.click();
                        })
                        .catch(err => console.error("Ошибка привязки сокета:", err));
                }
                closeModal();
            }
        });

        connection.on("JoinCommunityError", (errorMessage) => {
            console.warn(`❌ Бэкенд отклонил запрос: ${errorMessage}`);
            alert(`Не удалось вступить: ${errorMessage}`);
        });
    }

    Chat.initCommunitySearch = initCommunitySearch;
})(window.EduGameChat);