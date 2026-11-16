(function (Chat) {
    'use strict';

    const notificationsConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    let notificationSound = null;

    function playNotificationSound() {
        try { notificationSound.play().catch(() => {}); } catch {}
    }

    function showIncomingNotification(notification) {
        const message = notification?.message ?? notification?.Message ?? 'У вас новое уведомление.';
        const toast = document.createElement('div');
        toast.className = 'chat-notification-toast';
        toast.setAttribute('role', 'status');
        toast.textContent = message;
        document.body.appendChild(toast);
        window.setTimeout(() => toast.remove(), 6000);
        playNotificationSound();
    }

    async function startNotificationsConnection() {
        if (notificationsConnection.state !== signalR.HubConnectionState.Disconnected) return;
        try {
            await notificationsConnection.start();
        } catch (error) {
            console.error("Не удалось подключиться к каналу уведомлений:", error);
            window.setTimeout(startNotificationsConnection, 5000);
        }
    }

    function initNotifications() {
        notificationSound = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgipGBcGBkdZmXkH9rW12BkY+Fd2xkd5WRkIZ3bGB7mY2Nf3Vqa2+KjIZ8bmlug4N7eG11gIJ9e251gYJ+e250f4J+e250gIF+fG91gIJ+fG90f4F/fG91gIF/fG90f4F/fG91gIF/fG90f4F/fG91gIF/fG90');
        notificationSound.volume = 0.45;

        notificationsConnection.on("ReceiveNotification", showIncomingNotification);
        notificationsConnection.onclose(() => window.setTimeout(startNotificationsConnection, 5000));

        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.getRegistrations().then(async registrations => {
                const chatRegistrations = registrations.filter(registration =>
                    [registration.active, registration.waiting, registration.installing]
                        .some(worker => worker && new URL(worker.scriptURL).pathname === '/service-worker.js'));

                await Promise.all(chatRegistrations.map(async registration => {
                    const notifications = await registration.getNotifications();
                    notifications
                        .filter(notification => notification.tag?.startsWith('edugame-'))
                        .forEach(notification => notification.close());
                    await registration.unregister();
                }));
            }).catch(error => {
                console.error('Не удалось отключить ранее зарегистрированные уведомления EduGame:', error);
            });
        }
    }

    Object.assign(Chat, { notificationsConnection, playNotificationSound, startNotificationsConnection, initNotifications });
})(window.EduGameChat);