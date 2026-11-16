(function (Chat) {
    'use strict';
    const { state, apiFetch } = Chat;
    const { readBooleanResult } = Chat.utils;
    const { loadUnreadState } = Chat;
    const { tryLoadHistoryFromServer } = Chat;
    const { startNotificationsConnection } = Chat;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/chat")
        .withAutomaticReconnect()
        .build();

    let connectionStartPromise = null;
    let connectionRetryTimer = null;

    async function loadCurrentUser() {
        try {
            const [response, accessResponse] = await Promise.all([
                apiFetch('/api/user/GetMyBusinessPassport'),
                apiFetch('/api/developer/GetAdminAccess')
            ]);
            if (!response.ok) {
                throw new Error(`Не удалось загрузить профиль пользователя (${response.status}).`);
            }
            const data = await response.json();
            const passport = data.value ?? data;
            const user = passport.user ?? passport.User ?? {};
            if (passport.userName ?? passport.UserName) state.myUserName = passport.userName ?? passport.UserName;
            const userId = passport.userId ?? passport.id ?? user.externalId ?? user.ExternalId ?? user.id ?? user.Id;
            if (userId) state.myUserId = userId;
            state.myAvatarUrl = user.avatarUrl ?? user.AvatarUrl ?? null;
            if (!accessResponse.ok) {
                throw new Error(`Не удалось проверить доступ разработчика (${accessResponse.status}).`);
            }
            const accessResult = await accessResponse.json();
            state.isDeveloperAdmin = readBooleanResult(accessResult, "доступ разработчика");
            loadUnreadState();
        } catch (error) {
            console.error("Ошибка загрузки профиля пользователя:", error);
        }
    }

    function scheduleConnectionRetry() {
        if (connectionRetryTimer !== null) return;
        connectionRetryTimer = window.setTimeout(() => {
            connectionRetryTimer = null;
            void startConnection();
        }, 5000);
    }

    async function startConnection() {
        if (connection.state !== signalR.HubConnectionState.Disconnected) return;
        if (connectionStartPromise) return connectionStartPromise;

        connectionStartPromise = (async () => {
            try {
                const refreshResponse = await window.refreshEduGameSession();
                if (!refreshResponse.ok) {
                    if (refreshResponse.status === 401 || refreshResponse.status === 403) {
                        window.location.href = '/Login.html';
                        return;
                    }
                    throw new Error(`Не удалось обновить токен (${refreshResponse.status}).`);
                }
                await loadCurrentUser();
                await Promise.all([
                    startNotificationsConnection(),
                    connection.start()
                ]);
                await Chat.loadUserCommunities();
            } catch (error) {
                console.error("Сбой подключения к чату SignalR:", error);
                scheduleConnectionRetry();
            }
        })().finally(() => { connectionStartPromise = null; });

        return connectionStartPromise;
    }

    function initConnection() {
        connection.onreconnected(async () => {
            try {
                if (state.currentGroupId) {
                    await connection.invoke("JoinRoom", state.currentGroupId);
                    state.previousGroupId = state.currentGroupId;
                    state.previousGroupName = state.currentGroupName;
                    await tryLoadHistoryFromServer(state.currentGroupId, true);
                }
            } catch (error) {
                console.error("Не удалось восстановить соединение и историю чата:", error);
            }
        });

        connection.onclose(scheduleConnectionRetry);
    }

    Object.assign(Chat, { connection, initConnection, startConnection });
})(window.EduGameChat);