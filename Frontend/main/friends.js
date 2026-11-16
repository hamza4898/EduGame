import { state, escapeHtmlSafe } from "./core.js";

export function normalizeFriendItem(raw) {
    return {
        requestId: raw.requestId ?? raw.RequestId ?? raw.id ?? raw.Id ?? raw.friendRequestId ?? raw.FriendRequestId ?? null,
        userId:    raw.userId ?? raw.UserId ?? raw.friendId ?? raw.FriendId ?? raw.senderId ?? raw.SenderId ?? raw.receiverId ?? raw.ReceiverId ?? null,
        nickname:  raw.userName ?? raw.UserName ?? raw.nickname ?? raw.Nickname ?? raw.nickName ?? raw.friendName ?? raw.name ?? "Неизвестный боец",
        avatarUrl: raw.avatarUrl ?? raw.AvatarUrl ?? raw.avatar ?? null,
        isOnline:  Boolean(raw.isOnline ?? raw.IsOnline ?? raw.online ?? false),
        level:     raw.level ?? raw.Level ?? null
    };
}

export async function fetchFriendList(kind) {
    const urls = {
        list:     "/api/friend/GetMyFriends",
        incoming: "/api/friend/GetMyIncomingRequests",
        outgoing: "/api/friend/GetMyOutgoingRequests"
    };
    const res = await window.apiFetch(urls[kind]);
    const data = await res.json().catch(() => null);
    if (!res.ok || data?.isSuccess === false || data?.IsSuccess === false) {
        throw new Error(data?.error ?? data?.Error ?? `Не удалось загрузить список: ${res.status}`);
    }
    const arr = Array.isArray(data) ? data : (data?.value ?? data?.Value ?? data?.items ?? data?.Items ?? []);
    if (!Array.isArray(arr)) throw new Error("Сервер вернул некорректный список друзей.");
    return arr.map(normalizeFriendItem);
}

export async function friendApi(method, pathWithQuery) {
    try {
        const res = await window.apiFetch(`/api/friend/${pathWithQuery}`, { method });
        const data = await res.json().catch(() => null);

        if (!res.ok || data?.isSuccess === false || data?.IsSuccess === false) {
            const errors = data?.errors ?? data?.Errors;
            const errorText = Array.isArray(errors)
                ? errors.map(error => typeof error === "string" ? error : error.errorMessage ?? error.ErrorMessage ?? error.message).filter(Boolean).join(", ")
                : "";
            const msg = errorText || data?.error || data?.Error || data?.title || `Ошибка сервера (${res.status})`;
            alert("⚠️ " + msg);
            return false;
        }
        return true;
    } catch (e) {
        console.error("[Friends] Запрос не выполнен:", e);
        alert("⚠️ " + (e.message || "Не удалось связаться с сервером"));
        return false;
    }
}

export function updateFriendsBadges() {
    const totalBadge = document.getElementById("friendsTotalBadge");
    const dockBtn = document.getElementById("hudFriendsBtn");
    const incomingCount = state.friendsCache.incoming.length;

    if (totalBadge) {
        const texts = {
            list:     `${state.friendsCache.list.length} активных друзей`,
            incoming: `${incomingCount} входящих заявок`,
            outgoing: `${state.friendsCache.outgoing.length} исходящих заявок`
        };
        totalBadge.textContent = texts[state.currentFriendsTab];
        totalBadge.style.color = (state.currentFriendsTab === "incoming" && incomingCount > 0) ? "#34d399" : "white";
    }

    if (dockBtn) {
        let badge = dockBtn.querySelector(".dock-badge");
        if (incomingCount > 0) {
            if (!badge) {
                badge = document.createElement("span");
                badge.className = "dock-badge";
                dockBtn.appendChild(badge);
            }
            badge.textContent = incomingCount;
        } else if (badge) {
            badge.remove();
        }
    }
}

export function renderFriendsFeed() {
    const container = document.getElementById("friendsFeedContainer");
    if (!container) return;
    container.innerHTML = "";

    const items = state.friendsCache[state.currentFriendsTab] || [];
    updateFriendsBadges();

    if (state.friendsLoadError) {
        container.innerHTML = `<div style="text-align:center;color:#f87171;padding:40px 20px;font-size:13px;">${escapeHtmlSafe(state.friendsLoadError)}</div>`;
        return;
    }

    if (items.length === 0) {
        const emptyTexts = {
            list:     "Пока нет друзей — добавляй бойцов через сообщества! 🎯",
            incoming: "Нет входящих заявок 📭",
            outgoing: "Нет исходящих заявок ✈️"
        };
        container.innerHTML = `<div style="text-align:center;color:#475569;padding:40px 20px;font-size:13px;">${emptyTexts[state.currentFriendsTab]}</div>`;
        return;
    }

    items.forEach((item, index) => {
        const card = document.createElement("div");
        card.className = "friend-feed-card";
        card.style.cssText = `display:flex;align-items:center;gap:14px;padding:14px 18px;border-radius:12px;background:rgba(255,255,255,0.02);border:1px solid var(--border);animation:fadeIn 0.3s ease;animation-delay:${index * 0.04}s;`;

        const initials = escapeHtmlSafe(item.nickname.substring(0, 2).toUpperCase());
        const avatarInner = item.avatarUrl
            ? `<img src="${escapeHtmlSafe(item.avatarUrl)}" alt="" style="width:100%;height:100%;object-fit:cover;border-radius:50%;">`
            : `<span style="width:100%;height:100%;display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#059669,#34d399);color:white;font-weight:800;font-size:14px;border-radius:50%;">${initials}</span>`;

        const statusHtml = item.isOnline
            ? `<span style="font-size:10px;color:#34d399;font-weight:700;">● В сети</span>`
            : `<span style="font-size:10px;color:#475569;font-weight:600;">○ Не в сети</span>`;

        let actionsHtml = "";
        if (state.currentFriendsTab === "list") {
            actionsHtml = `<button class="friend-action-btn decline" data-action="delete">❌ Удалить</button>`;
        } else if (state.currentFriendsTab === "incoming") {
            actionsHtml = `
                <button class="friend-action-btn accept"  data-action="accept">✅ Принять</button>
                <button class="friend-action-btn decline" data-action="decline">❌ Отклонить</button>
                <button class="friend-action-btn block"   data-action="block">🚫 В блок</button>`;
        } else {
            actionsHtml = `<button class="friend-action-btn decline" data-action="cancel">↩️ Отменить</button>`;
        }

        const profileLink = item.userId
            ? `Profile.html?id=${encodeURIComponent(item.userId)}`
            : null;
        card.innerHTML = `
            <div style="width:44px;height:44px;flex-shrink:0;position:relative;">
                ${profileLink ? `<a href="${escapeHtmlSafe(profileLink)}" aria-label="Открыть профиль ${escapeHtmlSafe(item.nickname)}" style="display:block;width:100%;height:100%;">${avatarInner}</a>` : avatarInner}
                <span style="position:absolute;bottom:0;right:0;width:10px;height:10px;border-radius:50%;border:2px solid #0f1424;background:${item.isOnline ? "#34d399" : "#475569"};"></span>
            </div>
            <div style="flex:1;min-width:0;display:flex;flex-direction:column;gap:2px;">
                ${profileLink ? `<a href="${escapeHtmlSafe(profileLink)}" style="font-size:13px;color:white;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;text-decoration:none;font-weight:700;">${escapeHtmlSafe(item.nickname)}</a>` : `<strong style="font-size:13px;color:white;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${escapeHtmlSafe(item.nickname)}</strong>`}
                <span>${statusHtml}${item.level ? `<span style="font-size:10px;color:#475569;"> · Lvl ${item.level}</span>` : ""}</span>
            </div>
            <div style="display:flex;gap:8px;flex-shrink:0;">${actionsHtml}</div>`;

        card.querySelectorAll("[data-action]").forEach(btn => {
            btn.addEventListener("click", () => handleFriendAction(btn.dataset.action, item, btn));
        });

        container.appendChild(card);
    });
}

export async function handleFriendAction(action, item, btn) {
    if (!item.requestId) {
        alert("Не удалось определить ID запроса. Обновите список и попробуйте снова.");
        return;
    }
    btn.disabled = true;
    const oldHtml = btn.innerHTML;
    btn.innerHTML = "⏳";

    let ok = false;
    try {
        switch (action) {
            case "accept":
                ok = await friendApi("PUT", `AcceptFriendRequest?requestId=${item.requestId}`);
                break;
            case "decline":
            case "cancel":
                ok = await friendApi("DELETE", `DeclineFriendRequest?requestId=${item.requestId}`);
                break;
            case "block":
                ok = await friendApi("PUT", `BlockFriendRequest?requestId=${item.requestId}`);
                break;
            case "delete":
                if (!confirm(`Удалить ${item.nickname} из друзей?`)) break;
                ok = await friendApi("DELETE", `DeleteMyFriend?requestId=${item.requestId}`);
                break;
        }
    } catch (err) {
        console.error("[Friends] Ошибка действия:", err);
    }

    if (ok) {
        await loadAllFriends();
    } else {
        btn.disabled = false;
        btn.innerHTML = oldHtml;
    }
}

export async function loadAllFriends() {
    state.friendsLoadError = "";
    const container = document.getElementById("friendsFeedContainer");
    if (container) container.innerHTML = `<div class="empty-note">Загружаем друзей и запросы…</div>`;
    try {
        const [list, incoming, outgoing] = await Promise.all([
            fetchFriendList("list"),
            fetchFriendList("incoming"),
            fetchFriendList("outgoing")
        ]);
        state.friendsCache = { list, incoming, outgoing };
    } catch (err) {
        state.friendsLoadError = err.message || "Не удалось загрузить друзей и запросы.";
        console.error("[Friends] Ошибка загрузки списков:", err);
    }
    updateFriendsBadges();
    renderFriendsFeed();
}