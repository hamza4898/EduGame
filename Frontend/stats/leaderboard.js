(function (Stats) {
    'use strict';
    const { escapeHtml, createLeaderboardRowHtml } = Stats;

    function prepareLeaderboardContainer() {
        const tableContainer = document.getElementById("statsLeaderboardTable");
        if (tableContainer) {
            tableContainer.innerHTML = `<div style="padding: 20px; text-align: center; color: var(--text-muted); font-size: 13px;">Синхронизация с сервером EduGame...</div>`;
        }
        return tableContainer;
    }

    async function fetchStudentRatings() {
        const tableContainer = prepareLeaderboardContainer();
        if (!tableContainer) return;
        try {
            const response = await apiFetch('/api/ratings/GetTopStudents');
            if (!response.ok) throw new Error();
            const rows = await response.json();
            tableContainer.innerHTML = "";
            if (!rows || rows.length === 0) {
                tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-muted);">Студенты еще не заходили в хаб.</p>`;
                return;
            }
            const currentUsername = document.getElementById("statsUsernameDisplay")?.innerText;
            tableContainer.innerHTML = rows.map((item, index) => {
                const isMe = item.userName === currentUsername;
                const metric = `${item.globalXp} XP (Lvl ${item.level})`;
                return createLeaderboardRowHtml(index, item.avatarUrl, item.userName, metric, isMe, true, item.userId ?? item.UserId);
            }).join("");
        } catch (error) {
            console.warn("[Stats API] Ошибка загрузки студентов.", error);
            tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-red);">Не удалось загрузить рейтинг.</p>`;
        }
    }

    async function fetchCommunityRatings() {
        const tableContainer = prepareLeaderboardContainer();
        if (!tableContainer) return;
        try {
            const response = await apiFetch('/api/ratings/GetTopCommunities');
            if (!response.ok) throw new Error();
            const rows = await response.json();
            tableContainer.innerHTML = "";
            if (!rows || rows.length === 0) {
                tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-muted);">Сообщества еще не созданы.</p>`;
                return;
            }
            tableContainer.innerHTML = rows.map((item, index) => {
                const metric = `Индекс Силы: ${item.powerIndex}`;
                return createLeaderboardRowHtml(index, item.avatarUrl, item.name, metric, false, false);
            }).join("");
        } catch (error) {
            console.warn("[Stats API] Ошибка загрузки сообществ.", error);
            tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-red);">Не удалось загрузить рейтинг.</p>`;
        }
    }

    async function fetchCourseRatings() {
        const tableContainer = prepareLeaderboardContainer();
        if (!tableContainer) return;
        try {
            const response = await apiFetch('/api/ratings/GetTopCourses');
            if (!response.ok) throw new Error();
            const rows = await response.json();
            tableContainer.innerHTML = "";
            if (!rows || rows.length === 0) {
                tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-muted);">Курсы еще не добавлены на платформу.</p>`;
                return;
            }
            tableContainer.innerHTML = rows.map((item, index) => {
                const metric = `Поп. Индекс: ${item.popIndex.toFixed(1)}`;
                return createLeaderboardRowHtml(index, item.avatarUrl, item.title, metric, false, false);
            }).join("");
        } catch (error) {
            console.warn("[Stats API] Ошибка загрузки курсов.", error);
            tableContainer.innerHTML = `<p class="no-results-text" style="text-align:center; padding:20px; color:var(--text-red);">Не удалось загрузить рейтинг.</p>`;
        }
    }

    function switchLeaderboardTab(tabName) {
        document.querySelectorAll('.leaderboard-tabs .tab-chip').forEach(chip => chip.classList.remove('active'));
        const activeChip = document.getElementById(`chip-${tabName}`);
        if (activeChip) activeChip.classList.add('active');

        if (tabName === 'global') fetchStudentRatings();
        else if (tabName === 'vuz') fetchCommunityRatings();
        else if (tabName === 'weekly') fetchCourseRatings();
    }

    Object.assign(Stats, { prepareLeaderboardContainer, fetchStudentRatings, fetchCommunityRatings, fetchCourseRatings, switchLeaderboardTab });
})(window.EduGameStats);