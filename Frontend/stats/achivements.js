(function (Stats) {
    'use strict';
    const { realSubSectionXp, realBranchNodes } = Stats;

    async function fetchCourseAchievements() {
        try {
            return [];
        } catch (error) {
            console.warn("Достижения курсов пока не реализованы:", error);
            return [];
        }
    }

    function generateEduGameAchievements() {
        const achievements = [];

        Object.keys(realBranchNodes).forEach(branchKey => {
            const data = realBranchNodes[branchKey];
            const realXp = realSubSectionXp[branchKey] || 0;

            if (realXp === 0) return;

            data.nodes.forEach(node => {
                if (realXp >= node.threshold) {
                    achievements.push({
                        icon: node.icon,
                        title: node.name,
                        description: node.reward || `Разблокировано в ветке "${data.branchName}"`,
                        course: `${data.sectionTitle} → ${data.branchName}`,
                        xp: node.threshold,
                        date: "Недавно",
                        source: 'focus'
                    });
                }
            });
        });

        return achievements.sort((a, b) => b.xp - a.xp);
    }

    async function openAchievementsDetail(type) {
        const modal = document.getElementById("achievementsModal");
        const title = document.getElementById("achievementsModalTitle");
        const list = document.getElementById("achievementsDetailList");

        let titleText = "";
        if (type === "courses") titleText = '<i class="fa-solid fa-certificate" style="color:#10b981;"></i> Сертификаты за курсы';
        else if (type === "edugame") titleText = '<i class="fa-solid fa-medal" style="color:#f59e0b;"></i> Достижения от EduGame';
        else titleText = '<i class="fa-solid fa-trophy" style="color:#10b981;"></i> Все достижения';

        title.innerHTML = titleText;
        list.innerHTML = `<div style="padding:20px; text-align:center; color:var(--text-muted);">Загрузка достижений...</div>`;
        modal.classList.add("open");

        const renderCard = (ach, idx) => `
            <div class="achievement-detail-card animate__animated animate__fadeInUp" style="animation-delay: ${idx * 0.05}s">
                <div class="achievement-icon-box"><i class="fa-solid ${ach.icon}"></i></div>
                <div class="achievement-info">
                    <h4>${ach.title}</h4>
                    <p class="achievement-desc">${ach.description}</p>
                    ${ach.course ? `<p class="achievement-course"><i class="fa-solid fa-book"></i> ${ach.course}</p>` : ''}
                    <div class="achievement-meta">
                        <span><i class="fa-solid fa-coins"></i> +${ach.xp} XP</span>
                        <span><i class="fa-regular fa-calendar"></i> ${ach.date}</span>
                    </div>
                </div>
            </div>`;

        let html = "";
        let globalIndex = 0;

        if (type === "courses" || type === "all") {
            const courseAchievements = await fetchCourseAchievements();
            html += `<div class="achieve-section-divider"><i class="fa-solid fa-certificate" style="color:#10b981;"></i><span>Из курсов</span><span class="achieve-section-count">${courseAchievements.length}</span></div>`;
            if (courseAchievements.length === 0) {
                html += `<p class="achieve-empty">Сертификатов пока нет. Пройди свой первый курс!</p>`;
            } else {
                courseAchievements.forEach(ach => { html += renderCard(ach, globalIndex++); });
            }
        }

        if (type === "edugame" || type === "all") {
            const focusAchievements = generateEduGameAchievements();
            html += `<div class="achieve-section-divider" style="margin-top: ${type === 'all' ? '22px' : '0'};"><i class="fa-solid fa-medal" style="color:#f59e0b;"></i><span>От EduGame (Разблокировано в ветках)</span><span class="achieve-section-count">${focusAchievements.length}</span></div>`;
            if (focusAchievements.length === 0) {
                html += `<p class="achieve-empty">Системных достижений пока нет. Набирай XP в ветках фокусов!</p>`;
            } else {
                focusAchievements.forEach(ach => { html += renderCard(ach, globalIndex++); });
            }
        }

        list.innerHTML = html;
    }

    function closeAchievementsModal() {
        const modal = document.getElementById("achievementsModal");
        if (modal) modal.classList.remove("open");
    }

    Object.assign(Stats, { fetchCourseAchievements, generateEduGameAchievements, openAchievementsDetail, closeAchievementsModal });
})(window.EduGameStats);