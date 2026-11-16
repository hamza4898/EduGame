(function (Learn) {
    'use strict';
    const { state, API } = Learn;
    const { escapeHtml, pathName, totalLessons, priceHtml } = Learn.utils;
    const { loadMyCourses } = Learn;

    const rail = document.getElementById("mainCoursesRail");
    let railCollection = "all";

    function actionButtonHtml(course, collection) {
        const alreadyOwned = course.owned || collection === "all" || collection === "archived";
        if (alreadyOwned || course.isFree) {
            return `<button class="rail-btn enter" data-act="enter"><i class="fa-solid fa-play"></i> Войти</button>`;
        }
        return `<button class="rail-btn buy" data-act="buy"><i class="fa-solid fa-cart-shopping"></i> ${course.price}</button>`;
    }

    function renderRail() {
        if (!rail) return;
        rail.innerHTML = "";
        const list = state.ALL_COURSES.filter(c => {
            if (railCollection === "liked") return c.isLiked;
            if (railCollection === "archived") return c.isArchived;
            return !c.isArchived;
        });

        const railPrev = document.getElementById("railPrev");
        const railNext = document.getElementById("railNext");

        if (list.length === 0) {
            rail.innerHTML = `<p class="no-results-text" style="width: 100%; text-align: center; margin: 40px auto; color: var(--text-muted);">В этой секции пока нет курсов. Открой каталог снизу!</p>`;

            if (railPrev) railPrev.style.display = "none";
            if (railNext) railNext.style.display = "none";
            return;
        } else {
            if (railPrev) railPrev.style.display = "flex";
            if (railNext) railNext.style.display = "flex";
        }

        list.forEach(course => {
            const card = document.createElement("div");
            card.className = "rail-card animate__animated animate__fadeIn";
            const heartClass = course.isLiked ? "fa-solid fa-heart" : "fa-regular fa-heart";

            card.innerHTML = `
                <div class="rail-card-top">
                    <span class="card-app-badge">${pathName(course.pathId)}</span>
                    <div class="rail-card-tools">
                        <button class="btn-like-course" data-act="like" aria-label="В избранное"><i class="${heartClass}"></i></button>
                        ${course.owned ? `<button class="btn-like-course" data-act="archive" aria-label="В архив"><i class="fa-solid fa-box-archive"></i></button>` : ""}
                    </div>
                </div>
                <h3>${escapeHtml(course.title)}</h3>
                <p class="rail-desc">${escapeHtml(course.description)}</p>
                <div class="rail-meta-row">
                    <span class="rail-rating"><i class="fa-solid fa-star"></i> ${course.rating.toFixed(1)}</span>
                    <span><i class="fa-solid fa-users"></i> ${course.studentsCount.toLocaleString('ru-RU')}</span>
                    <span><i class="fa-solid fa-layer-group"></i> ${totalLessons(course)} уроков</span>
                </div>
                <div class="rail-progress"><div style="width:${course.progress || 0}%"></div></div>
                <div class="rail-card-footer">
                    ${priceHtml(course)}
                    <span style="font-size:11px;color:var(--violet-primary);font-weight:700;">${course.progress || 0}%</span>
                </div>
                <div class="rail-card-actions">
                    <button class="rail-btn details" data-act="details"><i class="fa-solid fa-circle-info"></i> Описание</button>
                    ${actionButtonHtml(course, railCollection)}
                </div>`;

            card.querySelector('[data-act="like"]').addEventListener("click", async (e) => {
                e.stopPropagation();
                try {
                    const response = await apiFetch(`${API.toggleLiked}?courseId=${encodeURIComponent(course.id)}`, { method: "POST" });
                    if (!response.ok) throw new Error("Не удалось обновить избранное.");
                    course.isLiked = await response.json();
                    renderRail();
                } catch (err) {
                    console.error("Не удалось сохранить лайк:", err);
                    alert(err.message || "Не удалось сохранить избранное.");
                }
            });

            const archive = card.querySelector('[data-act="archive"]');
            if (archive) archive.addEventListener("click", async (e) => {
                e.stopPropagation();
                try {
                    const response = await apiFetch(`${API.toggleArchived}?courseId=${encodeURIComponent(course.id)}`, { method: "POST" });
                    if (!response.ok) throw new Error("Не удалось изменить архив.");
                    course.isArchived = await response.json();
                    await loadMyCourses();
                    renderRail();
                } catch (err) {
                    alert(err.message || "Не удалось изменить архив.");
                }
            });

            card.querySelector('[data-act="details"]').addEventListener("click", (e) => { e.stopPropagation(); Learn.openCourseDetails(course); });

            const enter = card.querySelector('[data-act="enter"]');
            if (enter) enter.addEventListener("click", (e) => {
                e.stopPropagation();
                Learn.startCourse(course).catch(error => alert(error.message || "Не удалось открыть курс."));
            });

            const buy = card.querySelector('[data-act="buy"]');
            if (buy) buy.addEventListener("click", (e) => { e.stopPropagation(); Learn.buyCourse(course); });

            rail.appendChild(card);
        });
    }

    function initRail() {
        const railPrev = document.getElementById("railPrev");
        const railNext = document.getElementById("railNext");
        if (railPrev) railPrev.addEventListener("click", () => rail.scrollBy({ left: -320, behavior: "smooth" }));
        if (railNext) railNext.addEventListener("click", () => rail.scrollBy({ left: 320, behavior: "smooth" }));

        document.querySelectorAll(".dashboard-filters-zone .tab-filter").forEach(chip => {
            chip.addEventListener("click", () => {
                document.querySelectorAll(".dashboard-filters-zone .tab-filter").forEach(c => c.classList.remove("active"));
                chip.classList.add("active");
                railCollection = chip.getAttribute("data-collection") || "all";
                renderRail();
            });
        });
    }

    Object.assign(Learn, { renderRail, initRail });
})(window.EduGameLearn);