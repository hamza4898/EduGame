window.EduGameChat = window.EduGameChat || {};

(function (Chat) {
    'use strict';

    const DEFAULT_ROOM_GUID = "EduGame";
    const SEARCH_LIMIT = 10;
    const MEMBERS_LIMIT = 15;

    const state = {
        myUserName: "Гость",
        myUserId: null,
        myAvatarUrl: null,
        myCurrentRoleInRoom: 0,
        isDeveloperAdmin: false,

        currentGroupId: null,
        currentGroupName: DEFAULT_ROOM_GUID,
        previousGroupId: null,
        previousGroupName: null,

        currentMembers: [],
        membersOffset: 0,
        membersLoadedFor: null,
        membersLoadingFor: null,
        membersPageLoading: false,
        membersHasMore: false,

        isSwitchingTab: false,
        isUserScrolledUp: false,

        onlineCache: {},
        unreadCount: {},
        unreadMessages: {},
        roomMeta: {},
        lastReadAt: {},
        historyLoadedRooms: new Set(),
        unreadSyncCursors: new Map()
    };

    const LS = {
        activeRoom:  'edugame_active_room',
        messages:    (roomKey) => `edugame_messages_${roomKey}`
    };

    function lsGet(key, fallback = null) {
        try { const v = localStorage.getItem(key); return v ? JSON.parse(v) : fallback; }
        catch { return fallback; }
    }
    function lsSet(key, value) { localStorage.setItem(key, JSON.stringify(value)); }

    const apiFetch = typeof window.apiFetch === 'function'
        ? window.apiFetch.bind(window)
        : (url, options) => fetch(url, { credentials: 'include', ...options });

    const $ = (id) => document.getElementById(id);

    function escapeHtml(value) {
        const div = document.createElement('div');
        div.textContent = value ?? '';
        return div.innerHTML;
    }

    function readBooleanResult(payload, operation) {
        const value = typeof payload === 'boolean'
            ? payload
            : payload?.value ?? payload?.Value;
        if (typeof value !== 'boolean') {
            throw new Error(`Сервер вернул некорректный результат проверки: ${operation}.`);
        }
        return value;
    }

    function formatTime(date = new Date()) {
        const h = date.getHours().toString().padStart(2, '0');
        const m = date.getMinutes().toString().padStart(2, '0');
        return `${h}:${m}`;
    }

    function parseServerDate(value) {
        if (!value) return null;
        const normalized = typeof value === 'string'
            && !/(Z|[+-]\d{2}:\d{2})$/i.test(value)
            ? `${value}Z`
            : value;
        const date = new Date(normalized);
        return Number.isNaN(date.getTime()) ? null : date;
    }

    function pluralize(n, [one, few, many]) {
        const r10 = n % 10, r100 = n % 100;
        if (r100 >= 11 && r100 <= 19) return many;
        if (r10 === 1) return one;
        if (r10 >= 2 && r10 <= 4) return few;
        return many;
    }

    function isAttachmentUrl(value) {
        if (typeof value !== 'string') return false;
        try {
            const url = new URL(value, window.location.origin);
            return url.origin === window.location.origin
                && /^\/api\/chat\/GetAttachment\/[0-9a-f-]{36}\/[0-9a-f]{32}\.(jpg|jpeg|png|gif|webp|mp4|webm|mov)$/i.test(url.pathname);
        } catch {
            return false;
        }
    }

    function userProfileUrl(userId) {
        return userId ? `Profile.html?id=${encodeURIComponent(userId)}` : 'Profile.html';
    }

    Object.assign(Chat, {
        DEFAULT_ROOM_GUID, SEARCH_LIMIT, MEMBERS_LIMIT,
        state, LS, lsGet, lsSet, apiFetch,
        utils: { $, escapeHtml, readBooleanResult, formatTime, parseServerDate, pluralize, isAttachmentUrl, userProfileUrl }
    });
})(window.EduGameChat);