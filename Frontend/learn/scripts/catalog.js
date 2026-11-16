(function (Learn) {
    'use strict';
    const { state, PATHS } = Learn;
    const { catalogView, dashboardView } = Learn.el;
    const { escapeHtml, pathName, priceHtml, hexA } = Learn.utils;
    const { loadCatalogCourses } = Learn;
    const { showView } = Learn;
    const { openCourseDetails } = Learn;
    const { startCourse } = Learn;
    const { buyCourse } = Learn;

    const catalogPaths = document.getElementById("catalogPaths");
    const catalogGrid = document.getElementById("catalogGrid");
    const catalogSearch = document.getElementById("catalogSearch");
    let activeSubs = new Set();
    let openPaths = new Set(["it"]);
    let priceFilter = "all";

    function renderCatalogFilters() {
        if (!catalogPaths) return;
        let priceBlock = document.getElementById("priceFilterBlock");
        if (!priceBlock) {
            priceBlock = document.createElement("div");
            priceBlock.id = "priceFilterBlock";
            priceBlock.className = "filter-block";
            priceBlock.innerHTML = `
                <div class="filter-block-title">Цена</div>
                <div class="price-toggle">
                    <div class="price-chip active" data-price="all">Все</div>
                    <div class="price-chip" data-price="free">Бесплатно</div>
                    <div class="price-chip" data-price="paid">Платные</div>
                </div>`;
            catalogPaths.parentElement.insertBefore(priceBlock, catalogPaths);
            priceBlock.querySelectorAll(".price-chip").forEach(chip => {
                chip.addEventListener("click", () => {
                    priceBlock.querySelectorAll(".price-chip").forEach(c => c.classList.remove("active"));
                    chip.classList.add("active");
                    priceFilter = chip.getAttribute("data-price");
                    renderCatalogGrid();
                });
            });
        }

        catalogPaths.innerHTML = PATHS.map(p => `
            <div class="path-item ${openPaths.has(p.id) ? 'open' : ''}" data-path="${p.id}">
                <div class="path-head">
                    <span class="path-ico" style="background:${hexA(p.color,0.15)};color:${p.color};"><i class="fa-solid ${p.icon}"></i></span>
                    <span>${p.name}</span>
                    <i class="fa-solid fa-chevron-down path-chevron"></i>
                </div>
                <div class="path-subs">
                    ${p.subs.map(s => {
                        const key = `${p.id}::${s}`;
                        return `<div class="sub-chip ${activeSubs.has(key) ? 'active' : ''}" data-key="${key}">${s}<i class="fa-solid fa-check sub-check"></i></div>`;
                    }).join("")}
                </div>
            </div>`).join("");

        catalogPaths.querySelectorAll(".path-head").forEach(h => {
            h.addEventListener("click", () => {
                const id = h.parentElement.getAttribute("data-path");
                openPaths.has(id) ? openPaths.delete(id) : openPaths.add(id);
                renderCatalogFilters();
            });
        });
        catalogPaths.querySelectorAll(".sub-chip").forEach(chip => {
            chip.addEventListener("click", () => {
                const key = chip.getAttribute("data-key");
                activeSubs.has(key) ? activeSubs.delete(key) : activeSubs.add(key);
                renderCatalogFilters(); renderCatalogGrid();
            });
        });
    }

    function renderCatalogGrid() {
        if (!catalogGrid) return;
        const q = (catalogSearch?.value || "").toLowerCase().trim();
        const list = state.CATALOG_COURSES.filter(c => {
            const matchSub = activeSubs.size === 0 || activeSubs.has(`${c.pathId}::${c.sub}`);
            const matchPrice = priceFilter === "all" || (priceFilter === "free" ? c.isFree : !c.isFree);
            const matchQ = !q || c.title.toLowerCase().includes(q) || (c.description || "").toLowerCase().includes(q);
            return matchSub && matchPrice && matchQ;
        });

        const title = document.getElementById("catalogResultsTitle");
        const count = document.getElementById("catalogResultsCount");
        if (title) title.textContent = (activeSubs.size || priceFilter !== "all") ? "Отфильтрованные рейды" : "Все рейды";
        if (count) count.textContent = `${list.length} курсов`;

        if (list.length === 0) { catalogGrid.innerHTML = `<p class="no-results-text">Ничего не найдено. Попробуй другой путь или сбрось фильтры.</p>`; return; }

        catalogGrid.innerHTML = list.map(c => `
            <div class="catalog-card animate__animated animate__fadeIn" data-id="${c.id}">
                <div class="rail-card-top">
                    <span class="card-app-badge">${pathName(c.pathId)}</span>
                    ${priceHtml(c)}
                </div>
                <h3>${escapeHtml(c.title)}</h3>
                <p class="rail-desc">${escapeHtml(c.description)}</p>
                <div class="rail-meta-row">
                    <span class="rail-rating"><i class="fa-solid fa-star"></i> ${c.rating.toFixed(1)}</span>
                    <span><i class="fa-solid fa-users"></i> ${c.studentsCount.toLocaleString('ru-RU')}</span>
                </div>
                <div class="catalog-author">
                    <div class="author-avatar ${c.authorType}"><i class="fa-solid ${c.authorIcon}"></i></div>
                    <span>${escapeHtml(c.authorName)}</span>
                </div>
                <div class="rail-card-actions">
                    <button class="rail-btn details" data-act="details"><i class="fa-solid fa-circle-info"></i> Описание</button>
                    ${c.isFree || c.owned
                        ? `<button class="rail-btn enter" data-act="enter"><i class="fa-solid fa-play"></i> Войти</button>`
                        : `<button class="rail-btn buy" data-act="buy"><i class="fa-solid fa-cart-shopping"></i> ${c.price}</button>`}
                </div>
            </div>`).join("");

        catalogGrid.querySelectorAll(".catalog-card").forEach(card => {
            const course = state.CATALOG_COURSES.find(c => c.id == card.getAttribute("data-id"));
            card.querySelector('[data-act="details"]').addEventListener("click", (e) => { e.stopPropagation(); openCourseDetails(course); });
            const enter = card.querySelector('[data-act="enter"]');
            if (enter) enter.addEventListener("click", (e) => {
                e.stopPropagation();
                startCourse(course).catch(error => alert(error.message || "Не удалось открыть курс."));
            });
            const buy = card.querySelector('[data-act="buy"]');
            if (buy) buy.addEventListener("click", (e) => { e.stopPropagation(); buyCourse(course); });
            card.addEventListener("click", () => openCourseDetails(course));
        });
    }

    function initCatalog() {
        if (catalogSearch) catalogSearch.addEventListener("input", renderCatalogGrid);
        const catalogReset = document.getElementById("catalogReset");
        if (catalogReset) catalogReset.addEventListener("click", () => {
            activeSubs.clear(); priceFilter = "all";
            if (catalogSearch) catalogSearch.value = "";
            const pb = document.getElementById("priceFilterBlock");
            if (pb) pb.querySelectorAll(".price-chip").forEach(c => c.classList.toggle("active", c.getAttribute("data-price") === "all"));
            renderCatalogFilters(); renderCatalogGrid();
        });

        const btnOpenCatalog = document.getElementById("btnOpenCatalog");
        const btnLeaveCatalog = document.getElementById("btnLeaveCatalog");
        if (btnOpenCatalog) btnOpenCatalog.addEventListener("click", async () => {
            await loadCatalogCourses();
            renderCatalogFilters(); renderCatalogGrid(); showView(catalogView);
        });
        if (btnLeaveCatalog) btnLeaveCatalog.addEventListener("click", () => showView(dashboardView));
    }

    Learn.initCatalog = initCatalog;
})(window.EduGameLearn);