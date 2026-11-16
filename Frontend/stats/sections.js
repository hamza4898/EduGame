(function (Stats) {
    'use strict';
    const { escapeHtml } = Stats;

    async function renderSectionRows() {
        const container = document.getElementById("sectionsSkillList");
        if (!container) return;

        container.innerHTML = `<div style="padding:15px; color:var(--text-muted); font-size:13px;">Синхронизация матриц...</div>`;

        try {
            const response = await apiFetch('/api/ratings/GetUserSkillPaths');
            if (!response.ok) throw new Error();

            const pathsData = await response.json();
            container.innerHTML = "";

            if (!pathsData || pathsData.length === 0) {
                container.innerHTML = `<div style="padding:15px; color:var(--text-muted); font-size:12px;">Сферы не найдены в БД.</div>`;
                return;
            }

            pathsData.forEach(path => {
                const row = document.createElement("div");
                row.className = "skill-mini-box animate__animated animate__fadeInUpFast";
                row.innerHTML = `
                    <div class="skill-meta" style="display:flex; justify-content:space-between; width:100%; align-items:center; margin-bottom:6px;">
                        <span style="font-weight:500; display:flex; align-items:center; gap:8px;">
                            <i class="fa-solid ${path.icon}" style="color:${path.color}; width:16px; text-align:center;"></i>
                            ${escapeHtml(path.name)}
                        </span>
                        <span class="skill-val" style="font-weight:bold; color:var(--text-muted); font-size:13px;">
                            ${path.totalXp} XP
                        </span>
                    </div>
                `;
                container.appendChild(row);
            });
        } catch (error) {
            console.warn("[Stats API] Сбой загрузки сфер.", error);
            container.innerHTML = `<div style="padding:15px; color:var(--text-red); font-size:12px;">Не удалось обновить сферы обучения.</div>`;
        }
    }

    Stats.renderSectionRows = renderSectionRows;
})(window.EduGameStats);