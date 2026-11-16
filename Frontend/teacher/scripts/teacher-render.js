function getPathForSubSection(subSectionId) {
    return TEACHER_PATHS.find(path =>
        path.subs.some(sub => String(sub.id) === String(subSectionId))
    );
}

function renderSubSectionOptions(pathId, selectedSubSectionId) {
    const path = TEACHER_PATHS.find(item => String(item.id) === String(pathId));
    if (!path) return '<option value="">Сначала выберите категорию</option>';
    return `<option value="">Выберите подкатегорию...</option>${path.subs.map(sub =>
        `<option value="${sub.id}" ${String(sub.id) === String(selectedSubSectionId) ? "selected" : ""}>${escapeHtml(sub.name)}</option>`
    ).join("")}`;
}

function bindDescriptionControls() {
    const pathSelect = document.getElementById("pCategory");
    const subSectionSelect = document.getElementById("pSubCategory");
    if (!pathSelect || !subSectionSelect) return;
    pathSelect.addEventListener("change", () => {
        subSectionSelect.innerHTML = renderSubSectionOptions(pathSelect.value, "");
        subSectionSelect.disabled = !pathSelect.value;
    });
}

function renderTeacherHero() {
    const hero = document.getElementById("teacherHeroZone");
    if (!hero) return;
    const course = teacherCourses.find(item => item.status === "draft" && !item.isArchived)
        ?? teacherCourses.find(item => !item.isArchived)
        ?? teacherCourses[0];
    if (!course) {
        hero.innerHTML = `
            <div class="hero-card-inner">
                <span class="hero-badge">ТЕКУЩИЙ КУРС</span>
                <h2>У тебя пока нет курсов</h2>
                <p>Создайте первый курс — он появится в списке ниже.</p>
            </div>`;
        return;
    }
    const readiness = courseChecks(course);
    hero.innerHTML = `
        <div class="hero-card-inner">
            <span class="hero-badge">ТЕКУЩИЙ КУРС</span>
            <h2>${escapeHtml(course.title)}</h2>
            <p>${course.status === "draft"
                ? `Готовность к публикации: <strong>${readiness.pct}%</strong>. Выполнено ${readiness.done} из ${readiness.total} пунктов.`
                : "Откройте редактор курса, чтобы продолжить работу с его содержанием и настройками."}</p>
            ${course.status === "draft" ? `
                <div class="hero-progress-container">
                    <div class="hero-progress-bar" style="width: ${readiness.pct}%;"></div>
                    <span class="hero-progress-text">${readiness.pct}% готовности</span>
                </div>` : ""}
            <button class="btn-resume-raid" data-action="edit" data-id="${course.id}">
                <i class="fa-solid fa-pen-to-square"></i> Продолжить редактирование
            </button>
        </div>`;
    hero.querySelector("[data-action='edit']").addEventListener("click", () => openEditor(course.id));
}

function renderTeacherGrid() {
    const grid = document.getElementById("teacherCoursesGrid");
    if (!grid) return;
    grid.innerHTML = "";
    const allCourses = window.teacherCourses || [];
    if (teacherCollectionLoading) {
        grid.innerHTML = `<p class="no-results-text">Загружаем курсы…</p>`;
        updateTeacherRailArrows(false);
        return;
    }
    const courses = teacherCollection === "liked"
        ? teacherCollectionCourses.filter(course => course.isLiked && !course.isArchived)
        : teacherCollection === "archived"
            ? teacherCollectionCourses.filter(course => course.isArchived)
        : allCourses.filter(course => {
            if (teacherCollection === "draft") return course.status === "draft" && !course.isArchived;
            if (teacherCollection === "published") return course.status === "published" && !course.isArchived;
            return false;
        });

    if (courses.length === 0) {
        const emptyMessages = {
            draft: "Курсов в разработке пока нет. Создайте первый курс кнопкой ниже.",
            published: "Опубликованных курсов пока нет.",
            liked: "В избранном пока нет курсов.",
            archived: "В архиве пока нет курсов."
        };
        grid.innerHTML = `<p class="no-results-text">${emptyMessages[teacherCollection]}</p>`;
        updateTeacherRailArrows(false);
        return;
    }

    courses.forEach(course => {
        const card = document.createElement("div");
        card.className = "rail-card teacher-course-card animate__animated animate__fadeIn";
        const statusLabel = course.isArchived ? "Архив" : (course.status === "draft" ? "В разработке" : "Опубликовано");
        const statusClass = course.isArchived ? "archived" : course.status;
        const categoryLabel = course.sub || course.category || "Курс";
        const heartClass = course.isLiked ? "fa-solid fa-heart" : "fa-regular fa-heart";
        const priceHtml = course.isFree
            ? `<span class="course-price free">Бесплатно</span>` 
            : `<span class="course-price gold"><i class="fa-solid fa-coins"></i> ${Number(course.price) || 0} 🪙</span>`;

        card.innerHTML = `
            <div class="rail-card-top">
                <span class="card-app-badge">${escapeHtml(categoryLabel)}</span>
                <div class="teacher-card-tools">
                    <span class="teacher-course-status ${statusClass}">${statusLabel}</span>
                    <button class="btn-like-course" data-action="like" data-id="${course.id}" aria-label="Избранное">
                        <i class="${heartClass}"></i>
                    </button>
                </div>
            </div>
            <h3>${escapeHtml(course.title)}</h3>
            <p class="rail-desc">${escapeHtml(course.desc || course.description || "Описание курса пока не заполнено.")}</p>
            <div class="rail-meta-row">
                <span><i class="fa-solid fa-users"></i> ${Number(course.students) || 0} учеников</span>
                ${course.rating > 0 ? `<span class="rail-rating"><i class="fa-solid fa-star"></i> ${escapeHtml(course.rating)}</span>` : ""}
                <span><i class="fa-solid fa-layer-group"></i> ${Number(course.lessonCount) || 0} уроков</span>
            </div>
            <div class="rail-progress"><div style="width:${Math.min(100, Math.max(0, Number(course.avgProgress) || 0))}%"></div></div>
            <div class="rail-card-footer">
                ${priceHtml}
                <span class="teacher-course-progress" title="Средний прогресс учеников">${Math.round(Number(course.avgProgress) || 0)}% · средний прогресс</span>
            </div>
            <div class="rail-card-actions">
                <button class="rail-btn details" data-action="edit" data-id="${course.id}">
                    <i class="fa-solid fa-pen-to-square"></i> Редактор
                </button>
                <button class="rail-btn ${course.isArchived ? "details" : "enter"}" data-action="${course.isArchived ? "restore" : "archive"}" data-id="${course.id}">
                    ${course.isArchived ? `<i class="fa-solid fa-rotate-left"></i> Вернуть` : `<i class="fa-solid fa-box-archive"></i> В архив`}
                </button>
            </div>`;
        grid.appendChild(card);
    });
    updateTeacherRailArrows(courses.length > 1);
}

function updateTeacherRailArrows(show) {
    ["teacherRailPrev", "teacherRailNext"].forEach(id => {
        const arrow = document.getElementById(id);
        if (arrow) arrow.style.display = show ? "flex" : "none";
    });
}

function renderSidebar() {
    const nav = document.getElementById("instructorNav");
    if (!nav) return;
    const course = currentCourse();
    nav.innerHTML = NAV_GROUPS.map(group => `
        <div>
            <div class="instr-group-title"><i class="fa-solid ${group.icon}"></i> ${group.t}</div>
            ${group.items.map(item => {
                const badge = item[3] && course.solutions.length > 0 ? `<span class="count-badge">${course.solutions.length}</span>` : "";
                return `<button class="instr-item ${currentPanel === item[0] ? "active" : ""}" onclick="openPanel('${item[0]}')">
                    <i class="fa-solid ${item[2]}"></i> ${item[1]} ${badge}
                </button>`;
            }).join("")}
        </div>`).join("");
}

const NAV_GROUPS = [
    { t: "Курс", icon: "fa-book", items: [ ["desc", "Описание", "fa-file-pen"], ["content", "Содержание", "fa-list-check"], ["checklist", "Чек-лист", "fa-clipboard-check"] ]},
    { t: "Общение с учащимися", icon: "fa-comments", items: [ ["solutions", "Проверка решений", "fa-user-check", true], ["news", "Новости", "fa-bullhorn"], ["reviews", "Отзывы", "fa-star"] ]},
    { t: "Настройки", icon: "fa-gear", items: [ ["publish", "Публикация", "fa-globe"], ["price", "Цена и промокоды", "fa-tags"], ["access", "Права доступа", "fa-user-shield"], ["certs", "Сертификаты", "fa-certificate"] ]}
];

const PANELS = {
    desc: (c) => {
        const selectedPath = getPathForSubSection(c.skillSubSectionId) || TEACHER_PATHS.find(path => path.name === c.category);
        const selectedPathId = selectedPath?.id ?? "";
        return `
        <div class="panel-head"><h2><i class="fa-solid fa-file-pen text-violet"></i> Описание</h2><span class="panel-sub">Как курс видят студенты в каталоге</span></div>
        <label class="form-col">Название курса<input class="instr-input" id="pTitle" maxlength="200" value="${escapeHtml(c.title)}"></label>
        <label class="form-col">Описание<textarea class="instr-textarea" id="pDesc" rows="4" maxlength="2000">${escapeHtml(c.desc)}</textarea></label>
        <label class="form-col">Категория
            <select class="instr-input" id="pCategory">
                <option value="">Выберите категорию...</option>
                ${TEACHER_PATHS.map(path => `<option value="${path.id}" ${String(path.id) === String(selectedPathId) ? "selected" : ""}>${escapeHtml(path.name)}</option>`).join("")}
            </select>
        </label>
        <label class="form-col">Подкатегория
            <select class="instr-input" id="pSubCategory" ${selectedPath ? "" : "disabled"}>
                ${renderSubSectionOptions(selectedPathId, c.skillSubSectionId)}
            </select>
        </label>
        <div><button class="btn-sm violet" onclick="saveDesc()"><i class="fa-solid fa-floppy-disk"></i> Сохранить описание</button></div>`;
    },
    content: (c) => currentLessonSelection
        ? renderLessonEditor(c, currentLessonSelection.moduleIndex, currentLessonSelection.lessonIndex)
        : `
            <div class="panel-head"><h2><i class="fa-solid fa-list-check text-violet"></i> Содержание</h2>
            <button class="btn-sm green" onclick="addModule()"><i class="fa-solid fa-plus"></i> Добавить модуль</button></div>
            ${c.modules.length === 0 ? `<p class="empty-note">Пока нет модулей — добавь первый.</p>` : c.modules.map((m, mi) => `
                <div class="module-block">
                    <div class="module-title">
                        <input class="instr-input" aria-label="Название модуля" value="${escapeHtml(m.title)}" oninput="updateModuleTitle(${mi}, this.value)">
                        <button class="btn-sm green" onclick="addLesson(${mi})"><i class="fa-solid fa-plus"></i> Добавить урок</button>
                    </div>
                    ${m.lessons.map((l, li) => `
                        <div class="lesson-row lesson-row-editor">
                            <input class="lesson-title-input" aria-label="Название урока" value="${escapeHtml(l.title)}" oninput="updateLessonTitle(${mi}, ${li}, this.value)">
                            <span class="lesson-xp-badge">${Number(l.xpReward) || 0} XP</span>
                            <button class="btn-sm violet" onclick="openLessonEditor(${mi}, ${li})"><i class="fa-solid fa-file-pen"></i> Открыть урок</button>
                        </div>`).join("")}
                </div>`).join("")}
            <button class="btn-sm violet save-course-content" onclick="saveCourseContent()"><i class="fa-solid fa-floppy-disk"></i> Сохранить содержание курса</button>`,
    checklist: (c) => {
        const r = courseChecks(c);
        let lastGroup = "";
        return `
        <div class="panel-head"><h2><i class="fa-solid fa-clipboard-check text-violet"></i> Чек-лист</h2><span class="panel-sub">Готовность курса к публикации</span></div>
        <div class="readiness-hero">
            <h3>Курс готов на ${r.pct}%</h3>
            <div class="hero-progress-container" style="margin-bottom:0;">
                <div class="hero-progress-bar" style="width:${r.pct}%;"></div>
                <span class="hero-progress-text">${r.done} из ${r.total} пунктов выполнено</span>
            </div>
        </div>
        ${r.checks.map(ch => {
            const head = ch.group !== lastGroup ? `<div class="check-group-title">${ch.group}</div>` : "";
            lastGroup = ch.group;
            return `${head}<div class="check-row"><i class="fa-solid ${ch.done ? "fa-circle-check ok" : "fa-circle no"}"></i> ${ch.label}</div>`;
        }).join("")}`;
    },
    solutions: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-user-check text-violet"></i> Проверка решений</h2><span class="panel-sub">В очереди: ${c.solutions.length}</span></div>
        ${c.solutions.length === 0 ? `<p class="empty-note">Очередь пуста — студенты пока ничего не сдали. 🎉</p>` : c.solutions.map((s, i) => `
            <div class="solution-row">
                <div class="solution-meta"><strong>${s.student}</strong><span>${s.lesson} · ${s.ago}</span></div>
                <div class="code-block-mock"><pre><code>${s.code}</code></pre></div>
                <div style="display:flex; gap:10px;">
                    <button class="btn-sm green" onclick="acceptSolution(${i})"><i class="fa-solid fa-check"></i> Принять</button>
                    <button class="btn-sm red" onclick="returnSolution(${i})"><i class="fa-solid fa-rotate-left"></i> Вернуть на доработку</button>
                </div>
            </div>`).join("")}`,
    news: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-bullhorn text-violet"></i> Новости</h2></div>
        <div class="form-col">Текст новости
            <textarea class="instr-textarea" id="newsText" rows="3" placeholder="Напр.: Добавил новый урок по DI-контейнерам!"></textarea>
        </div>
        <div><button class="btn-sm violet" onclick="publishNews()"><i class="fa-solid fa-paper-plane"></i> Опубликовать</button></div>
        ${c.news.map(n => `<div class="list-item"><div class="li-head"><strong>${escapeHtml(n.author)}</strong><span>${new Date(n.createdAt).toLocaleString("ru-RU")}</span></div>${escapeHtml(n.text)}</div>`).join("") || `<p class="empty-note">Новостей пока нет.</p>`}`,
    reviews: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-star text-gold"></i> Отзывы</h2><span class="panel-sub">Средняя: ${c.rating > 0 ? "⭐ " + c.rating : "—"}</span></div>
        ${c.reviews.length === 0 ? `<p class="empty-note">Отзывов пока нет — они появятся после первых студентов.</p>` : c.reviews.map(r => `
            <div class="list-item">
                <div class="li-head"><strong>${escapeHtml(r.student)}</strong><span>${"⭐".repeat(r.stars)}</span></div>
                ${escapeHtml(r.text)}
            </div>`).join("")}`,
    publish: (c) => {
        const r = courseChecks(c);
        const canPublish = c.readiness?.isReady === true;
        let action;
        if (c.status === "published") action = `<button class="btn-sm red" onclick="unpublishCourse()"><i class="fa-solid fa-eye-slash"></i> Снять с публикации</button>`;
        else if (c.status === "archived") action = `<button class="btn-sm green" onclick="restoreCourse()"><i class="fa-solid fa-rotate-left"></i> Вернуть из архива</button>`;
        else action = `<button class="btn-sm green" ${canPublish ? "" : "disabled"} onclick="publishCourse()"><i class="fa-solid fa-globe"></i> Опубликовать курс</button>`;
        return `
        <div class="panel-head"><h2><i class="fa-solid fa-globe text-violet"></i> Публикация</h2></div>
        <div class="readiness-hero">
            <h3>Статус: ${c.status === "draft" ? "Черновик" : (c.status === "published" ? "Опубликован" : "Архив")}</h3>
            <div class="hero-progress-container" style="margin:0;">
                <div class="hero-progress-bar" style="width:${r.pct}%;"></div>
                <span class="hero-progress-text">Готовность ${r.pct}% · публикация требует выполнения всех условий</span>
            </div>
        </div>
        <div>${action}</div>
        ${!canPublish && c.status === "draft" ? `<div class="empty-note">${(c.readiness?.missingRequirements || []).map(escapeHtml).join("<br>") || "Заполни чек-лист, чтобы опубликовать курс."}</div>` : ""}`;
    },
    price: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-tags text-gold"></i> Цена и промокоды</h2></div>
        <label class="switch-row"><input type="checkbox" id="pFree" ${c.isFree ? "checked" : ""}> Курс бесплатный (монеты за учеников)</label>
        <label class="form-col">Цена в монетах (если платный)<input class="instr-input" id="pPrice" type="number" min="0" value="${c.price}"></label>
        <div><button class="btn-sm violet" onclick="savePrice()"><i class="fa-solid fa-floppy-disk"></i> Сохранить цену</button></div>
        <div class="check-group-title">Промокоды</div>
        ${c.promo.map((p, i) => `<div class="list-item"><div class="li-head"><strong>${escapeHtml(p.code)}</strong><span>скидка ${p.discountPercent}%</span></div><button class="btn-sm red" onclick="removePromo(${i})">Удалить</button></div>`).join("") || `<p class="empty-note">Промокодов нет.</p>`}
        <div class="inline-row">
            <input class="instr-input" id="promoCode" placeholder="КОД">
            <input class="instr-input" id="promoDiscount" type="number" min="1" max="100" placeholder="%">
            <button class="btn-sm green" onclick="addPromo()"><i class="fa-solid fa-plus"></i></button>
        </div>`,
    access: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-user-shield text-violet"></i> Права доступа</h2></div>
        <div class="list-item"><div class="li-head"><strong>Вы</strong><span>Владелец курса</span></div></div>
        ${c.coTeachers.map((t, i) => `
            <div class="list-item"><div class="li-head"><strong>${escapeHtml(t.userName)}</strong><span>${escapeHtml(t.role)}</span></div>
                <div><button class="btn-sm red" onclick="removeCoTeacher(${i})"><i class="fa-solid fa-user-minus"></i> Убрать доступ</button></div>
            </div>`).join("")}
        <div class="inline-row">
            <input class="instr-input" id="coName" placeholder="Ник нового со-автора">
            <button class="btn-sm green" onclick="addCoTeacher()"><i class="fa-solid fa-user-plus"></i> Выдать доступ</button>
        </div>`,
    certs: (c) => `
        <div class="panel-head"><h2><i class="fa-solid fa-certificate text-gold"></i> Сертификаты</h2></div>
        <label class="switch-row"><input type="checkbox" id="certEnabled" ${c.cert.enabled ? "checked" : ""}> Выдавать сертификат за прохождение курса</label>
        <label class="form-col">Подпись на сертификате<input class="instr-input" id="certSigner" value="${escapeHtml(c.cert.signer)}"></label>
        <div><button class="btn-sm violet" onclick="saveCert()"><i class="fa-solid fa-floppy-disk"></i> Сохранить</button></div>
        ${c.cert.enabled ? `<div class="list-item"><div class="li-head"><strong>🏆 Сертификат EduGame</strong><span>подпись: ${c.cert.signer}</span></div>Студент получает сертификат после 100% прохождения курса.</div>` : ""}`
};

function openPanel(key) {
    currentPanel = key;
    currentLessonSelection = key === "content" ? currentLessonSelection : null;
    renderSidebar();
    const panel = document.getElementById("instructorPanel");
    if (!panel) return;
    const course = currentCourse();
    if (key === "desc") {
        panel.innerHTML = '<p class="empty-note">Загружаем данные курса…</p>';
        try { loadCourseDescription(course); } 
        catch (error) {
            console.error("Не удалось загрузить описание курса:", error);
            panel.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить курс.")}</p>`;
            showToast("Не удалось загрузить курс", error.message || "Проверьте подключение и попробуйте снова.");
            return;
        }
    }
    if (["news", "reviews", "price", "access", "publish"].includes(key)) {
        panel.innerHTML = '<p class="empty-note">Загружаем данные с сервера…</p>';
        try { loadEditorPanelData(key, course); } 
        catch (error) {
            console.error(`Не удалось загрузить панель ${key}:`, error);
            panel.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить данные.")}</p>`;
            showToast("Ошибка загрузки", error.message || "Проверьте подключение и попробуйте снова.");
            return;
        }
    }
    if (currentCourseId !== course.id || currentPanel !== key) return;
    panel.innerHTML = PANELS[key](course);
    if (key === "desc") bindDescriptionControls();
}

function renderLessonEditor(course, moduleIndex, lessonIndex) {
    const module = course.modules[moduleIndex];
    const lesson = module?.lessons[lessonIndex];
    if (!lesson) return `<p class="empty-note">Урок не найден. Вернитесь к содержанию курса.</p>`;
    const maximumXp = ({ 1: 300, 2: 700, 3: 2000 })[course.difficulty] ?? 150;
    const blockEditor = (block, blockIndex) => {
        const blockType = Number(block.type);
        if (blockType === 0) {
            const safeHtml = sanitizeLessonHtml(block.content);
            return `<div class="lesson-block-editor">
                <div class="lesson-block-heading"><strong><i class="fa-solid fa-align-left"></i> Текст лекции</strong>
                    <button class="btn-sm red" onclick="removeLessonBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">Удалить блок</button></div>
                <p class="lesson-editor-hint">Введите или вставьте текст в белое поле ниже. Форматирование можно выбрать кнопками над редактором.</p>
                <div class="lesson-rich-editor" id="lessonText-${moduleIndex}-${lessonIndex}-${blockIndex}" contenteditable="true"
                    data-placeholder="Нажмите здесь и начните писать лекцию…"
                    oninput="updateTextBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex}, this)">${safeHtml}</div>
            </div>`;
        }
        if (blockType === 2 || blockType === 1) {
            const isImage = blockType === 2;
            return `<div class="lesson-block-editor">
                <div class="lesson-block-heading"><strong><i class="fa-solid ${isImage ? "fa-image" : "fa-video"}"></i> ${isImage ? "Изображение" : "Видео"}</strong>
                    <button class="btn-sm red" onclick="removeLessonBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">Удалить блок</button></div>
                <label class="form-col">HTTPS-ссылка на ${isImage ? "изображение" : "YouTube или Vimeo"}
                    <input class="instr-input" id="media-${moduleIndex}-${lessonIndex}-${blockIndex}" value="${escapeHtml(block.content)}"
                        placeholder="${isImage ? "https://example.com/image.jpg" : "https://youtu.be/..."}"
                        oninput="updateMediaBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex}, this.value)">
                </label>
            </div>`;
        }
        let task = {};
        try { task = JSON.parse(block.content || "{}"); } catch { task = {}; }
        if (blockType === 3) {
            const options = Array.isArray(task.options) && task.options.length >= 2 ? task.options : ["", ""];
            const answerIndex = Number.isInteger(Number(task.answer)) && Number(task.answer) >= 0 && Number(task.answer) < options.length
                ? Number(task.answer)
                : 0;
            return `<div class="lesson-block-editor">
                <div class="lesson-block-heading"><strong><i class="fa-solid fa-circle-question"></i> Квиз</strong>
                    <button class="btn-sm red" onclick="removeLessonBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">Удалить блок</button></div>
                <p class="lesson-editor-hint">Напишите вопрос, добавьте варианты и отметьте правильный ответ.</p>
                <label class="form-col">Вопрос
                    <input class="instr-input" id="quiz-question-${moduleIndex}-${lessonIndex}-${blockIndex}" value="${escapeHtml(task.question ?? "")}"
                        placeholder="Например: Что выведет программа?"
                        oninput="updateQuizBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">
                </label>
                <fieldset class="quiz-options-editor">
                    <legend>Варианты ответа</legend>
                    ${options.map((option, optionIndex) => `
                        <div class="quiz-option-row">
                            <label class="quiz-correct-choice" title="Отметить правильный ответ">
                                <input type="radio" name="quiz-correct-${moduleIndex}-${lessonIndex}-${blockIndex}" value="${optionIndex}"
                                    ${optionIndex === answerIndex ? "checked" : ""}
                                    onchange="updateQuizBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">
                                <span>Правильный</span>
                            </label>
                            <input class="instr-input quiz-option-input" data-quiz-block="${moduleIndex}-${lessonIndex}-${blockIndex}" data-option-index="${optionIndex}" value="${escapeHtml(option ?? "")}"
                                aria-label="Вариант ответа ${optionIndex + 1}" placeholder="Вариант ${optionIndex + 1}"
                                oninput="updateQuizBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">
                            <button class="btn-sm gray quiz-remove-option" type="button"
                                onclick="removeQuizOption(${moduleIndex}, ${lessonIndex}, ${blockIndex}, ${optionIndex})"
                                aria-label="Удалить вариант ${optionIndex + 1}" title="Удалить вариант"
                                ${options.length <= 2 ? "disabled" : ""}>
                                <i class="fa-solid fa-xmark"></i>
                            </button>
                        </div>`).join("")}
                    <button class="btn-sm gray quiz-add-option" type="button"
                        onclick="addQuizOption(${moduleIndex}, ${lessonIndex}, ${blockIndex})"
                        ${options.length >= 6 ? "disabled" : ""}>
                        <i class="fa-solid fa-plus"></i> Добавить вариант
                    </button>
                </fieldset>
            </div>`;
        }
        return `<div class="lesson-block-editor">
            <div class="lesson-block-heading"><strong><i class="fa-solid fa-paper-plane"></i> Задание с проверкой преподавателем</strong>
                <button class="btn-sm red" onclick="removeLessonBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">Удалить блок</button></div>
            <label class="form-col">Условие задания
                <textarea class="instr-textarea" id="assignment-prompt-${blockIndex}" rows="4" oninput="updateAssignmentBlock(${moduleIndex}, ${lessonIndex}, ${blockIndex})">${escapeHtml(task.prompt ?? "")}</textarea>
            </label>
        </div>`;
    };
    return `<div class="panel-head"><h2><i class="fa-solid fa-file-pen text-violet"></i> Редактор урока</h2>
        <button class="btn-sm gray" onclick="backToCourseContent()"><i class="fa-solid fa-arrow-left"></i> К содержанию</button></div>
        <label class="form-col">Модуль<input class="instr-input" value="${escapeHtml(module.title)}" oninput="updateModuleTitle(${moduleIndex}, this.value)"></label>
        <label class="form-col">Название урока<input class="instr-input" value="${escapeHtml(lesson.title)}" oninput="updateLessonTitle(${moduleIndex}, ${lessonIndex}, this.value)"></label>
        <label class="form-col">Опыт за прохождение урока (максимум ${maximumXp} XP)
            <input class="instr-input" id="lessonXp-${moduleIndex}-${lessonIndex}" type="number" min="0" max="${maximumXp}" value="${Number(lesson.xpReward) || 0}"
                oninput="updateLessonXp(${moduleIndex}, ${lessonIndex}, this.value)">
        </label>
        <div class="lesson-editor-toolbar" aria-label="Форматирование текста">
            <button class="btn-sm gray" onclick="formatLessonText('bold')" title="Жирный"><i class="fa-solid fa-bold"></i></button>
            <button class="btn-sm gray" onclick="formatLessonText('italic')" title="Курсив"><i class="fa-solid fa-italic"></i></button>
            <button class="btn-sm gray" onclick="formatLessonText('underline')" title="Подчеркнутый"><i class="fa-solid fa-underline"></i></button>
            <button class="btn-sm gray" onclick="formatLessonText('insertUnorderedList')" title="Список"><i class="fa-solid fa-list-ul"></i></button>
        </div>
        ${lesson.blocks.map(blockEditor).join("")}
        <div class="lesson-block-actions">
            <button class="btn-sm violet" onclick="addLessonBlock(${moduleIndex}, ${lessonIndex}, 0)"><i class="fa-solid fa-align-left"></i> Текст</button>
            <button class="btn-sm violet" onclick="addLessonBlock(${moduleIndex}, ${lessonIndex}, 2)"><i class="fa-solid fa-image"></i> Фото</button>
            <button class="btn-sm violet" onclick="addLessonBlock(${moduleIndex}, ${lessonIndex}, 1)"><i class="fa-solid fa-video"></i> Видео</button>
            <button class="btn-sm violet" onclick="addLessonBlock(${moduleIndex}, ${lessonIndex}, 3)"><i class="fa-solid fa-circle-question"></i> Квиз</button>
            <button class="btn-sm violet" onclick="addLessonBlock(${moduleIndex}, ${lessonIndex}, 4)"><i class="fa-solid fa-paper-plane"></i> Задание</button>
        </div>
        <button class="btn-sm green save-course-content" onclick="saveCourseContent()"><i class="fa-solid fa-floppy-disk"></i> Сохранить содержание курса</button>`;
}