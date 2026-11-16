let refreshSessionPromise = null;

function refreshEduGameSession() {
    if (!refreshSessionPromise) {
        const refresh = () => fetch('/api/auth/refreshTokens', {
            method: 'POST',
            credentials: 'include'
        });

        const request = navigator.locks?.request
            ? navigator.locks.request('edugame-auth-refresh', refresh)
            : refresh();

        refreshSessionPromise = Promise.resolve(request)
            .finally(() => { refreshSessionPromise = null; });
    }

    return refreshSessionPromise;
}

window.refreshEduGameSession = refreshEduGameSession;

window.apiFetch = async (url, options = {}) => {
    const requestUrl = typeof url === 'string' ? url : url.url;
    const requestOptions = { ...options, credentials: 'include' };
    const response = await fetch(url, requestOptions);
    const isAuthRequest = requestUrl.includes('/api/login/')
        || requestUrl.includes('/api/auth/refreshTokens');

    if (response.status !== 401 || isAuthRequest) return response;

    try {
        const refreshResponse = await refreshEduGameSession();
        if (!refreshResponse.ok) {
            window.location.href = '/Login.html';
            throw new Error(`Сессия истекла: не удалось обновить токен (${refreshResponse.status}).`);
        }

        const retryResponse = await fetch(url, requestOptions);
        if (retryResponse.status === 401) {
            window.location.href = '/Login.html';
        }
        return retryResponse;
    } catch (error) {
        console.error('Ошибка при обновлении сессии EduGame:', error);
        window.location.href = '/Login.html';
        throw error;
    }
};