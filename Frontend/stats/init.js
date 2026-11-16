(function (Stats) {
    'use strict';
    const { fetchStudentRatings } = Stats;
    const { renderAllFocusTrees, initFocusMapEngine } = Stats;
    const { renderSectionRows } = Stats;
    const { openFocusTree, closeFocusTree, openSpecificTree, closeSpecificTree, openCoursesModal, closeCoursesModal } = Stats;
    const { openAchievementsDetail, closeAchievementsModal } = Stats;
    const { switchLeaderboardTab } = Stats;

    document.addEventListener("DOMContentLoaded", async () => {
        fetchStudentRatings();

        const menuBtn = document.getElementById('menuBtn');
        const sidebarMenu = document.getElementById('sidebarMenu');
        const menuCloseBtn = document.getElementById('menuCloseBtn');
        const menuOverlay = document.getElementById('menuOverlay');

        if (menuBtn && sidebarMenu && menuOverlay && menuCloseBtn) {
            menuBtn.addEventListener('click', () => { sidebarMenu.classList.add('open'); menuOverlay.classList.add('active'); });
            menuCloseBtn.addEventListener('click', () => { sidebarMenu.classList.remove('open'); menuOverlay.classList.remove('active'); });
            menuOverlay.addEventListener('click', () => { sidebarMenu.classList.remove('open'); menuOverlay.classList.remove('active'); });
        }

        try {
            const response = await apiFetch("/api/user/GetMyBusinessPassport");
            if (response.ok) {
                const data = await response.json();
                if (data) {
                    console.log("BusinessPassport:", data);
                    const userName = data.userName || data.name || "Хамза (Ты)";
                    if (document.getElementById("statsUsernameDisplay")) {
                        document.getElementById("statsUsernameDisplay").innerText = userName;
                    }
                    const avatarUrl = data.user?.avatarUrl ?? data.user?.AvatarUrl;
                    if (avatarUrl) {
                        document.querySelectorAll(".saas-avatar-mini").forEach(avatar => {
                            avatar.style.backgroundImage = `url("${avatarUrl.replaceAll('"', '%22')}")`;
                        });
                    }
                    document.querySelectorAll(".saas-avatar-mini").forEach(avatar => {
                        avatar.style.cursor = "pointer";
                        avatar.addEventListener("click", () => { window.location.href = "Profile.html"; });
                    });

                    let level = data.levelingProgress?.level || 1;
                    let globalXp = data.user?.globalXp || data.totalXp || data.xp || 0;

                    if (document.getElementById("statsUserLevel")) {
                        document.getElementById("statsUserLevel").innerText = `Lvl ${level}`;
                    }

                    const xpValues = document.getElementById("xpBarValues");
                    const xpFill = document.getElementById("xpBarFill");

                    const MAX_GLOBAL_XP = 1000000;
                    const xpPercent = Math.min(100, (globalXp / MAX_GLOBAL_XP) * 100);

                    if (xpValues) xpValues.innerText = `${globalXp.toLocaleString()} / ${MAX_GLOBAL_XP.toLocaleString()} XP`;
                    if (xpFill) xpFill.style.width = `${xpPercent}%`;
                }
            }
        } catch (error) {
            console.warn("Офлайн-режим дашборда:", error);
        }

        renderAllFocusTrees();
        renderSectionRows();
        initFocusMapEngine();
    });

    window.openFocusTree = openFocusTree;
    window.closeFocusTree = closeFocusTree;
    window.openSpecificTree = openSpecificTree;
    window.closeSpecificTree = closeSpecificTree;
    window.openCoursesModal = openCoursesModal;
    window.closeCoursesModal = closeCoursesModal;
    window.openAchievementsDetail = openAchievementsDetail;
    window.closeAchievementsModal = closeAchievementsModal;
    window.switchLeaderboardTab = switchLeaderboardTab;
})(window.EduGameStats);