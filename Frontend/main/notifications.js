import { state, escapeHtmlSafe } from "./core.js";

export async function loadNotifications() {
    const response = await window.apiFetch("/api/notifications/GetMyNotifications");
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw new Error(payload?.error || `Ошибка загрузки уведомлений (${response.status}).`);
    state.notifications = Array.isArray(payload) ? payload : [];
    renderNotificationsFeed();
}

export async function acceptCommunityInvite(notification, button) {
    button.disabled = true;
    button.textContent = "Принимаем...";
    try {
        const response = await window.apiFetch("/api/chat/AcceptCommunityInvite", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ communityId: notification.targetId, notificationId: notification.id })
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok) throw new Error(payload?.error || payload?.title || "Не удалось принять приглашение.");
        await loadNotifications();
        window.location.href = "Chat.html";
    } catch (error) {
        button.disabled = false;
        button.textContent = "Принять приглашение";
        alert(error.message || "Не удалось принять приглашение.");
    }
}

export async function connectNotifications() {
    if (!window.signalR) {
        console.error("[Notifications] SignalR client library is unavailable.");
        return;
    }

    state.notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();
    state.notificationConnection.on("ReceiveNotification", async () => {
        try {
            await loadNotifications();
        } catch (error) {
            console.error("[Notifications] Failed to refresh after SignalR event:", error);
        }
    });

    try {
        await state.notificationConnection.start();
    } catch (error) {
        console.error("[Notifications] SignalR connection failed:", error);
    }
}

export function renderNotificationsFeed() {
    const container = document.getElementById("notificationsFeedContainer");
    const totalBadge = document.getElementById("notifTotalBadge");
    const notifBtn = document.getElementById("notificationBtn");
    if (!container) return;

    container.innerHTML = "";
    const unreadCount = state.notifications.filter(n => !n.isRead).length;

    if (totalBadge) {
        totalBadge.textContent = unreadCount === 0 ? "У вас нет новых сообщений" : `${unreadCount} новых сообщения`;
        totalBadge.style.color = unreadCount === 0 ? "#64748b" : "white";
    }
    if (notifBtn) {
        unreadCount > 0 ? notifBtn.setAttribute("data-count", unreadCount) : notifBtn.removeAttribute("data-count");
    }

    state.notifications.forEach(notif => {
        const isUnread = !notif.isRead;
        const icon = notif.type === "CourseAccess" ? "fa-user-plus" : notif.type === "CourseNews" ? "fa-bullhorn" : "fa-bell";
        const iconColor = notif.type === "CourseAccess" ? "#a855f7" : notif.type === "CourseNews" ? "#3b82f6" : "#10b981";
        const time = new Date(notif.createdAt).toLocaleString("ru-RU");
        const card = document.createElement("div");
        card.className = `notif-feed-card ${isUnread ? 'unread-glow' : 'read-dim'}`;
        card.style.cursor = "pointer";

        const unreadDot = isUnread ? `<span class="unread-ping-dot" style="width: 6px; height: 6px; background: #e63946; border-radius: 50%; display: block; position: absolute; top: 18px; right: 16px; box-shadow: 0 0 8px #e63946;"></span>` : "";

        card.innerHTML = `
            <div style="font-size: 16px; color: ${isUnread ? iconColor : '#475569'}; margin-top: 2px;"><i class="fa-solid ${icon}"></i></div>
            <div style="flex: 1; display: flex; flex-direction: column; gap: 4px; position: relative;">
                <div style="display: flex; justify-content: space-between; align-items: center; width: 100%; padding-right: 15px;">
                    <strong style="font-size: 13px; color: ${isUnread ? 'white' : '#64748b'};">${escapeHtmlSafe(notif.sender)}</strong>
                    <span style="font-size: 10px; color: #475569;">${escapeHtmlSafe(time)}</span>
                </div>
                <p style="font-size: 12px; color: ${isUnread ? '#94a3b8' : '#475569'}; line-height: 1.5; margin: 0;">${escapeHtmlSafe(notif.message)}</p>
                ${notif.type === "CommunityInvite" ? `<button class="hud-btn confirm" data-accept-community-invite style="width: fit-content; margin-top: 8px; padding: 7px 12px; font-size: 11px;">Принять приглашение</button>` : ""}
                ${unreadDot}
            </div>`;

        const acceptButton = card.querySelector("[data-accept-community-invite]");
        if (acceptButton) acceptButton.addEventListener("click", event => {
            event.stopPropagation();
            acceptCommunityInvite(notif, acceptButton);
        });

        card.addEventListener("click", async () => {
            if (isUnread) {
                try {
                    const response = await window.apiFetch(`/api/notifications/MarkNotificationRead?notificationId=${notif.id}`, { method: "POST" });
                    if (!response.ok) throw new Error(`Не удалось отметить уведомление (${response.status}).`);
                    notif.isRead = true;
                    renderNotificationsFeed();
                } catch (error) {
                    console.error("[Notifications] Failed to mark as read:", error);
                }
            }
            const mOverlay = document.getElementById("modalNotifDetail");
            if (mOverlay) {
                document.getElementById("modalNotifSender").textContent = notif.sender;
                document.getElementById("modalNotifTime").textContent = time;
                document.getElementById("modalNotifMessage").textContent = notif.message;
                const mIcon = document.getElementById("modalNotifIconBox");
                mIcon.innerHTML = `<i class="fa-solid ${icon}"></i>`;
                mIcon.style.color = iconColor;
                const acceptInviteButton = document.getElementById("btnAcceptCommunityInvite");
                if (acceptInviteButton) {
                    acceptInviteButton.style.display = notif.type === "CommunityInvite" ? "block" : "none";
                    acceptInviteButton.onclick = () => acceptCommunityInvite(notif, acceptInviteButton);
                }
                mOverlay.classList.add("open");
            }
        });
        container.appendChild(card);
    });
}