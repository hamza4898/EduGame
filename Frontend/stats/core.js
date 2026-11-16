window.EduGameStats = window.EduGameStats || {};

(function (Stats) {
    'use strict';

    const state = {};

    const PATH_META = {
        it:       { title: "Путь IT",           icon: "fa-laptop-code",      color: "#06b6d4" },
        design:   { title: "Путь Творца",       icon: "fa-palette",          color: "#10b981" },
        lang:     { title: "Путь Полиглота",    icon: "fa-language",         color: "#a855f7" },
        business: { title: "Путь Стратега",     icon: "fa-chess",            color: "#f59e0b" },
        eng:      { title: "Путь Зодчего",      icon: "fa-compass-drafting", color: "#06b6d4" },
        growth:   { title: "Путь Восхождения",  icon: "fa-arrow-up-right-dots", color: "#ef4444" }
    };

    const realSubSectionXp = {};
    const realBranchNodes = {};

    function escapeHtml(s) {
        return String(s ?? "").replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    window.escapeHtml = escapeHtml;

    Object.assign(Stats, { state, PATH_META, realSubSectionXp, realBranchNodes, escapeHtml });
})(window.EduGameStats);