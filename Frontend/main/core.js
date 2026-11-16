export const eduGameRanksConfig = {
    "Novice":      { title: "Новичок",        badge: "🪨", color: "#94a3b8" },
    "Apprentice":  { title: "Ученик",         badge: "🛠️", color: "#cd7f32" },
    "Adept":       { title: "Адепт",          badge: "🦾", color: "#cbd5e1" },
    "Virtuoso":    { title: "Виртуоз",        badge: "🌀", color: "#3b82f6" },
    "Master":      { title: "Мастер",   badge: "⚡", color: "#a855f7" },
    "Expert":      { title: "Эксперт",        badge: "💎", color: "#10b981" },
    "Magister":    { title: "Магистр",        badge: "🧪", color: "#ef4444" },
    "Archmage":    { title: "Архимаг",        badge: "🔮", color: "#8b5cf6" },
    "Grandmaster": { title: "Грандмастер",     badge: "🌌", color: "#6366f1" },
    "Legend":      { title: "Легенда",        badge: "👑", color: "#e2e8f0" },

    "Assistant":          { title: "Ассистент",            badge: "📋", color: "#94a3b8" },
    "Instructor":         { title: "Инструктор",           badge: "📐", color: "#cbd5e1" },
    "Lector":             { title: "Лектор",               badge: "🎙️", color: "#3b82f6" },
    "Curator":            { title: "Куратор",              badge: "🛡️", color: "#a855f7" },
    "Mentor":             { title: "Ментор",               badge: "🧠", color: "#f59e0b" },
    "Professor":          { title: "Профессор",            badge: "🎓", color: "#10b981" },
    "AcademyMagister":    { title: "Магистр Академии",     badge: "🏛️", color: "#ef4444" },
    "Dean":               { title: "Декан",                badge: "⚖️", color: "#8b5cf6" },
    "KnowledgeArchitect": { title: "Архитектор Знаний",    badge: "🏗️", color: "#6366f1" },
    "Grandmaster":        { title: "Великий Магистр",      badge: "👑", color: "#fbbf24" }
};

export const state = {
    cards: [],
    currentIndex: 0,
    isSwitching: false,
    currentFriendsTab: "list",
    notifications: [],
    notificationConnection: null,
    currentUserRole: "",
    aiSupportHistoryLoaded: false,
    aiSupportHistoryLoadPromise: null,
    friendsCache: { list: [], incoming: [], outgoing: [] },
    friendsLoadError: ""
};

export function escapeHtmlSafe(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
}