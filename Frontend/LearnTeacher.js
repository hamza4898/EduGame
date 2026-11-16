const teacherCourses = [];
const TEACHER_PATHS = [];
let teacherCollectionCourses = [];
let currentCourseId = null;
let currentPanel = "dashboard";
let currentLessonSelection = null;
let teacherCollection = "draft";
let teacherCollectionRequestVersion = 0;
let teacherCollectionLoading = false;
let teacherLikedCourseIds = new Set();
let teacherStudentCounts = new Map();
let teacherRatings = new Map();
let teacherProgress = new Map();

function getCourse(id) {
    return teacherCourses.find(c => c.id === id)
        ?? teacherCollectionCourses.find(c => c.id === id);
}
function currentCourse() {
    return getCourse(currentCourseId);
}

function escapeHtml(s) {
    return String(s ?? "").replace(/[&<>"']/g, c => ({ 
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' 
    }[c]));
}

function sanitizeLessonHtml(html) {
    const allowedTags = new Set(["P", "BR", "STRONG", "B", "EM", "I", "U", "UL", "OL", "LI", "H2", "H3", "BLOCKQUOTE", "PRE", "CODE", "A"]);
    const document = new DOMParser().parseFromString(String(html ?? ""), "text/html");
    const sanitizeNode = node => {
        if (node.nodeType === Node.TEXT_NODE) return document.createTextNode(node.textContent ?? "");
        if (node.nodeType !== Node.ELEMENT_NODE) return document.createDocumentFragment();

        const fragment = document.createDocumentFragment();
        const element = node;
        if (!allowedTags.has(element.tagName)) {
            [...element.childNodes].forEach(child => fragment.append(sanitizeNode(child)));
            return fragment;
        }

        const clean = document.createElement(element.tagName.toLowerCase());
        if (element.tagName === "A") {
            const href = element.getAttribute("href");
            try {
                const url = new URL(href, window.location.origin);
                if (["https:", "http:"].includes(url.protocol)) {
                    clean.setAttribute("href", url.href);
                    clean.setAttribute("target", "_blank");
                    clean.setAttribute("rel", "noopener noreferrer");
                }
            } catch {
                // Invalid links are rendered as plain text.
            }
        }
        [...element.childNodes].forEach(child => clean.append(sanitizeNode(child)));
        fragment.append(clean);
        return fragment;
    };

    const result = document.createElement("div");
    [...document.body.childNodes].forEach(child => result.append(sanitizeNode(child)));
    return result.innerHTML;
}

function normalizeCourseSections(sections) {
    return (sections ?? []).map(section => ({
        id: section.id ?? section.Id ?? 0,
        title: section.title ?? section.Title ?? "",
        lessons: (section.lessons ?? section.Lessons ?? []).map(lesson => ({
            id: lesson.id ?? lesson.Id ?? 0,
            title: lesson.title ?? lesson.Title ?? "",
            order: lesson.order ?? lesson.Order ?? 0,
            xpReward: lesson.xpReward ?? lesson.XpReward ?? 0,
            blocks: (lesson.blocks ?? lesson.Blocks ?? []).map(block => ({
                id: block.id ?? block.Id ?? 0,
                type: block.type ?? block.Type ?? 0,
                content: block.content ?? block.Content ?? "",
                order: block.order ?? block.Order ?? 0
            }))
        }))
    }));
}

function unwrapApiResult(payload, message) {
    if (payload?.isSuccess === false || payload?.IsSuccess === false) {
        const rawErrors = payload.errors ?? payload.Errors;
        const errors = Array.isArray(rawErrors) ? rawErrors : [];
        const errorMessages = errors.map(error =>
            typeof error === "string" ? error : (error.errorMessage ?? error.ErrorMessage ?? error.identifier ?? error.Identifier)
        ).filter(Boolean);
        throw new Error(errorMessages.join("\n") || message);
    }
    return payload?.value ?? payload?.Value ?? payload;
}

async function loadTeacherCourses() {
    const [rows, likedRows, studentCounts, ratings, progress, completionCounts] = await Promise.all([
        fetchTeacherCourseRows("/api/courses/GetMyTeacherCourses"),
        fetchTeacherCourseRows("/api/courses/GetMyLikedTeacherCourses"),
        fetchTeacherCourseRows("/api/courses/GetMyTeacherCourseStudentCounts"),
        fetchTeacherCourseRows("/api/courses/GetMyTeacherCourseRatings"),
        fetchTeacherCourseRows("/api/courses/GetMyTeacherCourseProgress"),
        fetchTeacherCourseRows("/api/courses/GetMyTeacherCourseCompletionCounts")
    ]);

    teacherLikedCourseIds = new Set(likedRows.map(row => Number(row.id ?? row.Id)));
    teacherStudentCounts = new Map(studentCounts.map(row => [
        Number(row.courseId ?? row.CourseId),
        Number(row.studentCount ?? row.StudentCount) || 0
    ]));
    teacherRatings = new Map(ratings.map(row => [
        Number(row.courseId ?? row.CourseId),
        Number(row.rating ?? row.Rating) || 0
    ]));
    teacherProgress = new Map(progress.map(row => [
        Number(row.courseId ?? row.CourseId),
        {
            average: Number(row.averageProgress ?? row.AverageProgress) || 0
        }
    ]));
    const completionCountByCourse = new Map(completionCounts.map(row => [
        Number(row.courseId ?? row.CourseId),
        Number(row.completedCount ?? row.CompletedCount) || 0
    ]));

    teacherCourses.splice(0, teacherCourses.length, ...rows.map(row => {
        const course = normalizeTeacherCourse(row);
        course.isLiked = teacherLikedCourseIds.has(course.id);
        course.completedCount = completionCountByCourse.get(course.id) || 0;
        return course;
    }));
    window.teacherCourses = teacherCourses;
    currentCourseId = teacherCourses[0]?.id ?? null;
    renderTeacherHero();
    renderTeacherGrid();
}

async function fetchTeacherCourseRows(url) {
    const response = await apiFetch(url);
    const payload = await response.json().catch(() => null);
    if (!response.ok) {
        throw new Error(payload?.error || payload?.title || `Не удалось загрузить данные курсов (${response.status}).`);
    }

    const rows = unwrapApiResult(payload, "Не удалось загрузить данные курсов преподавателя.");
    if (!Array.isArray(rows)) throw new Error("Сервер вернул некорректный список.");
    return rows;
}

function normalizeTeacherCourse(row) {
    const id = Number(row.id ?? row.Id);
    const title = row.title ?? row.Title ?? "";
    const description = row.description ?? row.Description ?? "";
    const isPaid = row.isPaid ?? row.IsPaid ?? false;
    const course = {
        id,
        title,
        desc: description,
        description,
        difficulty: row.difficulty ?? row.Difficulty ?? 2,
        category: row.skillPathName ?? row.SkillPathName ?? "",
        sub: row.skillSubSectionName ?? row.SkillSubSectionName ?? "",
        skillSubSectionId: row.skillSubSectionId ?? row.SkillSubSectionId,
        communityName: row.communityName ?? row.CommunityName ?? "",
        status: (row.isPublished ?? row.IsPublished) ? "published" : "draft",
        price: row.priceCoins ?? row.PriceCoins ?? 0,
        isPaid,
        isFree: !isPaid,
        students: 0,
        lessonCount: row.lessonCount ?? row.LessonCount ?? 0,
        rating: 0,
        avgProgress: 0,
        isLiked: teacherLikedCourseIds.has(id),
        isArchived: false,
        newsCount: row.newsCount ?? row.NewsCount ?? 0,
        completedCount: 0,
        readinessPercent: row.readinessPercent ?? row.ReadinessPercent ?? 0,
        isReadyToPublish: row.isReadyToPublish ?? row.IsReadyToPublish ?? false,
        modules: [],
        solutions: [],
        news: [],
        reviews: [],
        studentsNames: [],
        grades: [],
        promo: [],
        coTeachers: [],
        cert: { enabled: false, signer: "Преподаватель" },
        activity: [0, 0, 0, 0, 0, 0, 0]
    };
    course.students = teacherStudentCounts.get(id) || 0;
    course.rating = teacherRatings.get(id) || 0;
    const courseProgress = teacherProgress.get(id);
    course.avgProgress = courseProgress?.average || 0;
    course.completedCount = courseProgress?.completed || 0;
    return course;
}

async function loadTeacherCollection(collection, requestVersion) {
    const endpoint = collection === "liked"
        ? "/api/courses/GetMyLikedTeacherCourses"
        : "/api/courses/GetMyArchivedTeacherCourses";
    const rows = await fetchTeacherCourseRows(endpoint);
    if (requestVersion !== teacherCollectionRequestVersion || collection !== teacherCollection) return;
    const loadedCourses = rows.map(row => {
        const course = normalizeTeacherCourse(row);
        course.isLiked = collection === "liked" || teacherLikedCourseIds.has(course.id);
        course.isArchived = collection === "archived";
        const existing = getCourse(course.id);
        if (existing) {
            Object.assign(existing, course, {
                modules: existing.modules,
                solutions: existing.solutions,
                news: existing.news,
                reviews: existing.reviews,
                studentsNames: existing.studentsNames,
                grades: existing.grades,
                promo: existing.promo,
                coTeachers: existing.coTeachers,
                cert: existing.cert,
                activity: existing.activity,
                students: existing.students,
                rating: existing.rating,
                avgProgress: existing.avgProgress,
                completedCount: existing.completedCount
            });
            return existing;
        }
        return course;
    });

    teacherCollectionCourses = loadedCourses;
    renderTeacherGrid();
}

async function initCreateCourseModal() {
    const pathSelect = document.getElementById("createPath");
    const subPathSelect = document.getElementById("createSubPath");
    const isFreeCheckbox = document.getElementById("createIsFree");
    const priceInput = document.getElementById("createPrice");
    const communityNameInput = document.getElementById("createCommunityName");
    const titleInput = document.getElementById("createTitle");

    const response = await apiFetch("/api/courses/GetCourseCreationOptions");
    const payload = await response.json().catch(() => null);
    if (!response.ok) {
        throw new Error(payload?.error || payload?.title || `Не удалось загрузить направления обучения (${response.status}).`);
    }
    const paths = unwrapApiResult(payload, "Не удалось загрузить направления обучения.");
    if (!Array.isArray(paths)) {
        throw new Error("Сервер вернул некорректный список направлений обучения.");
    }

    TEACHER_PATHS.splice(0, TEACHER_PATHS.length, ...paths.map(path => ({
        id: path.id ?? path.Id,
        name: path.name ?? path.Name,
        subs: (path.subSections ?? path.SubSections ?? []).map(section => ({
            id: section.id ?? section.Id,
            name: section.name ?? section.Name
        }))
    })));

    paths.forEach(p => {
        const opt = document.createElement("option");
        opt.value = p.id ?? p.Id;
        opt.textContent = p.name ?? p.Name;
        pathSelect.appendChild(opt);
    });

    pathSelect.addEventListener("change", () => {
        const selectedPath = TEACHER_PATHS.find(p => String(p.id) === pathSelect.value);
        subPathSelect.innerHTML = '<option value="">Выберите подкатегорию...</option>';
        
        if (selectedPath) {
            subPathSelect.disabled = false;
            selectedPath.subs.forEach(sub => {
                const opt = document.createElement("option");
                opt.value = sub.id;
                opt.textContent = sub.name;
                subPathSelect.appendChild(opt);
            });
        } else {
            subPathSelect.disabled = true;
            subPathSelect.innerHTML = '<option value="">Сначала выберите путь</option>';
        }
    });

    isFreeCheckbox.addEventListener("change", () => {
        if (isFreeCheckbox.checked) {
            priceInput.value = "0";
            priceInput.disabled = true;
        } else {
            priceInput.disabled = false;
            priceInput.focus();
        }
    });

    titleInput.addEventListener("input", () => {
        if (!communityNameInput.dataset.edited) {
            communityNameInput.value = titleInput.value.trim().slice(0, 30);
        }
    });
    communityNameInput.addEventListener("input", () => {
        communityNameInput.dataset.edited = "true";
    });
}

function courseChecks(course) {
    const modules = course.modules || [];
    const lessons = modules.flatMap(m => m.lessons || []);
    const hasTask = lessons.some(lesson => lesson.blocks?.some(block => block.type === 3));
    const checks = [
        { group: "Структура и содержание", label: "Название не короче 5 символов", done: (course.title || "").trim().length >= 5 },
        { group: "Структура и содержание", label: "Заполнено описание (от 20 символов)", done: (course.desc || "").trim().length >= 20 },
        { group: "Структура и содержание", label: "Создано минимум 3 урока", done: lessons.length >= 3 },
        { group: "Структура и содержание", label: "Уроки заполнены контентом", done: lessons.filter(l => l.blocks?.some(block => (block.content || "").trim())).length >= 2 },
        { group: "Подача", label: "Добавлен квиз с проверкой ответа", done: hasTask },
        { group: "Подача", label: "Опубликована первая новость", done: (course.news || []).length > 0 || course.newsCount > 0 },
        { group: "Подача", label: "Настроена цена или курс бесплатный", done: course.isFree ? course.price === 0 : course.price > 0 }
    ];
    const done = checks.filter(c => c.done).length;
    const pct = modules.length ? Math.round((done / checks.length) * 100) : course.readinessPercent || 0;
    return { checks, done: modules.length ? done : Math.round((pct * checks.length) / 100), total: checks.length, pct };
}

function courseIncome(course) {
    if (course.isFree) return course.students * 2;
    return course.students * course.price;
}

function showToast(title, message) {
    const toast = document.getElementById("gameToast");
    if (!toast) return;
    document.getElementById("toastTitle").innerText = title;
    document.getElementById("toastMessage").innerText = message;
    toast.classList.add("show");
    setTimeout(() => toast.classList.remove("show"), 3500);
}

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

async function toggleTeacherCourseArchive(courseId) {
    const course = getCourse(courseId);
    if (!course) return;
    try {
        const response = await apiFetch(`/api/courses/ToggleArchived?courseId=${encodeURIComponent(courseId)}`, { method: "POST" });
        if (!response.ok) throw new Error(`Не удалось обновить архив (${response.status}).`);
        course.isArchived = unwrapApiResult(await response.json(), "Не удалось обновить архив курса.");
        if (!course.isArchived && !teacherCourses.some(item => item.id === course.id)) {
            teacherCourses.push(course);
            window.teacherCourses = teacherCourses;
        }
        renderTeacherGrid();
        renderTeacherHero();
    } catch (error) {
        console.error("Не удалось обновить архив курса:", error);
        showToast("Не удалось обновить архив", error.message || "Проверьте соединение и повторите попытку.");
    }
}

async function toggleTeacherCourseLike(courseId) {
    const course = getCourse(courseId);
    if (!course) return;
    try {
        const response = await apiFetch(`/api/courses/ToggleLiked?courseId=${encodeURIComponent(courseId)}`, { method: "POST" });
        if (!response.ok) throw new Error(`Не удалось обновить избранное (${response.status}).`);
        course.isLiked = unwrapApiResult(await response.json(), "Не удалось обновить избранное курса.");
        if (course.isLiked) teacherLikedCourseIds.add(courseId);
        else teacherLikedCourseIds.delete(courseId);
        renderTeacherGrid();
    } catch (error) {
        console.error("Не удалось обновить избранное курса:", error);
        showToast("Не удалось обновить избранное", error.message || "Проверьте соединение и повторите попытку.");
    }
}

function openCreateModal() {
    const modal = document.getElementById("modalCreateCourse");
    if (modal) modal.classList.add("open");
}

function closeCreateModal() {
    const modal = document.getElementById("modalCreateCourse");
    if (modal) modal.classList.remove("open");
}

async function createCourse() {
    const skillSubSectionId = Number(document.getElementById("createSubPath").value);
    const title = document.getElementById("createTitle").value.trim();
    const desc = document.getElementById("createDesc").value.trim();
    const isFree = document.getElementById("createIsFree").checked;
    const price = isFree ? 0 : (parseInt(document.getElementById("createPrice").value, 10) || 0);
    const communityName = document.getElementById("createCommunityName").value.trim();
    const communityDescription = document.getElementById("createCommunityDesc").value.trim();
    const button = document.getElementById("btnCreateCourse");

    if (!skillSubSectionId) { showToast("Ошибка", "Выберите путь обучения и подкатегорию!"); return; }
    if (title.length < 5) { showToast("Ошибка", "Название курса должно быть не менее 5 символов!"); return; }
    if (desc.length < 10) { showToast("Ошибка", "Дайте более подробное описание курса (мин. 10 символов)."); return; }
    if (communityName.length < 3 || communityName.length > 30) { showToast("Ошибка", "Название сообщества должно содержать от 3 до 30 символов."); return; }
    if (!isFree && price <= 0) { showToast("Ошибка", "Цена платного курса должна быть больше нуля."); return; }

    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Создаём курс и сообщество...';
    try {
        const response = await apiFetch("/api/courses/CreateCourse", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                title, description: desc, isPaid: !isFree, priceCoins: price,
                difficulty: Number(document.getElementById("createDifficulty").value),
                skillSubSectionId, communityName, communityDescription,
                privacyType: Number(document.getElementById("createCommunityPrivacy").value),
                membersLimit: Number(document.getElementById("createCommunityLimit").value)
            })
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            const rawErrors = payload?.errors ?? payload?.Errors;
            const errors = Array.isArray(rawErrors) ? rawErrors : [];
            const errorMessages = errors.map(error => typeof error === "string" ? error : (error.errorMessage ?? error.ErrorMessage ?? error.identifier ?? error.Identifier)).filter(Boolean);
            throw new Error(payload?.error ?? payload?.title ?? (errorMessages.join("\n") || `Не удалось создать курс (${response.status}).`));
        }

        const result = unwrapApiResult(payload, "Сервер не подтвердил создание курса.");
        await loadTeacherCourses();
        const createdCourseId = result.courseId ?? result.CourseId;
        const createdCourse = teacherCourses.find(course => course.id === createdCourseId)
            ?? teacherCourses.find(course => course.communityName === (result.communityName ?? result.CommunityName) && course.title === title);
        if (createdCourse) currentCourseId = createdCourse.id;

        document.getElementById("createTitle").value = "";
        document.getElementById("createDesc").value = "";
        document.getElementById("createCommunityName").value = "";
        document.getElementById("createCommunityName").dataset.edited = "";
        document.getElementById("createCommunityDesc").value = "";
        document.getElementById("createPath").value = "";
        document.getElementById("createSubPath").innerHTML = '<option value="">Сначала выберите путь</option>';
        document.getElementById("createSubPath").disabled = true;
        document.getElementById("createIsFree").checked = true;
        document.getElementById("createPrice").value = "0";
        document.getElementById("createPrice").disabled = true;

        closeCreateModal();
        showToast("Курс создан! 🎓", `Курс «${title}» и связанное сообщество «${communityName}» готовы.`);
    } catch (error) {
        console.error("Ошибка создания курса и сообщества:", error);
        showToast("Не удалось создать курс", error.message || "Проверьте соединение и повторите попытку.");
    } finally {
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

const NAV_GROUPS = [
    { t: "Курс", icon: "fa-book", items: [ ["desc", "Описание", "fa-file-pen"], ["content", "Содержание", "fa-list-check"], ["checklist", "Чек-лист", "fa-clipboard-check"] ]},
    { t: "Общение с учащимися", icon: "fa-comments", items: [ ["solutions", "Проверка решений", "fa-user-check", true], ["news", "Новости", "fa-bullhorn"], ["reviews", "Отзывы", "fa-star"] ]},
    { t: "Настройки", icon: "fa-gear", items: [ ["publish", "Публикация", "fa-globe"], ["price", "Цена и промокоды", "fa-tags"], ["access", "Права доступа", "fa-user-shield"], ["certs", "Сертификаты", "fa-certificate"] ]}
];

function openEditor(courseId) {
    currentCourseId = courseId;
    currentPanel = "desc";
    document.getElementById("teacherDashView").classList.add("hidden");
    document.getElementById("teacherDashView").style.display = "none";
    document.getElementById("editorView").classList.remove("hidden");
    document.getElementById("editorView").style.display = "block";
    document.getElementById("editorCourseTitle").innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(currentCourse().title)}`;
    openPanel("desc");
}

function closeEditor() {
    document.getElementById("editorView").classList.add("hidden");
    document.getElementById("editorView").style.display = "none";
    document.getElementById("teacherDashView").classList.remove("hidden");
    document.getElementById("teacherDashView").style.display = "flex";
    renderTeacherGrid();
    renderTeacherHero();
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

function applyCourseDescription(course, data) {
    if (!data || typeof data !== "object") throw new Error("Сервер вернул некорректные данные курса.");
    course.title = data.courseName ?? data.CourseName ?? course.title;
    course.desc = data.courseDescription ?? data.CourseDescription ?? course.desc;
    course.description = course.desc;
    course.category = data.skillPath ?? data.SkillPath ?? course.category;
    course.sub = data.skillSubSection ?? data.SkillSubSection ?? course.sub;
    course.skillSubSectionId = data.skillSubSectionId ?? data.SkillSubSectionId ?? course.skillSubSectionId;
    course.difficulty = data.difficulty ?? data.Difficulty ?? course.difficulty;
    course.isPaid = data.isPaid ?? data.IsPaid ?? !course.isFree;
    course.isFree = !course.isPaid;
    course.price = data.priceCoins ?? data.PriceCoins ?? course.price;
    course.status = (data.isPublished ?? data.IsPublished) ? "published" : "draft";
    course.cert = { enabled: data.certificateEnabled ?? data.CertificateEnabled ?? false, signer: data.certificateSigner ?? data.CertificateSigner ?? "" };
    course.modules = normalizeCourseSections(data.sections ?? data.Sections);
    const title = document.getElementById("editorCourseTitle");
    if (title && currentCourseId === course.id) title.innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(course.title)}`;
}

async function loadCourseDescription(course) {
    const response = await apiFetch(`/api/courses/GetCourseDescription?courseId=${encodeURIComponent(course.id)}`);
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw new Error(payload?.error || payload?.title || `Не удалось загрузить курс (${response.status}).`);
    applyCourseDescription(course, unwrapApiResult(payload, "Не удалось загрузить описание курса."));
}

async function courseApiRequest(path, options) {
    const response = await apiFetch(path, options);
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw new Error(payload?.error || payload?.title || `Запрос не выполнен (${response.status}).`);
    return unwrapApiResult(payload, "Сервер вернул ошибку.");
}

async function loadEditorPanelData(key, course) {
    const query = `courseId=${encodeURIComponent(course.id)}`;
    if (key === "news") {
        course.news = await courseApiRequest(`/api/courses/GetCourseNews?${query}`);
        course.newsCount = course.news.length;
    }
    else if (key === "reviews") {
        course.reviews = await courseApiRequest(`/api/courses/GetCourseReviews?${query}`);
        course.rating = course.reviews.length ? course.reviews.reduce((total, review) => total + review.stars, 0) / course.reviews.length : 0;
    } else if (key === "price") course.promo = await courseApiRequest(`/api/courses/GetCoursePromos?${query}`);
    else if (key === "access") course.coTeachers = await courseApiRequest(`/api/courses/GetCourseTeachers?${query}`);
    else if (key === "publish") course.readiness = await courseApiRequest(`/api/courses/GetCourseReadiness?${query}`);
}

async function openPanel(key) {
    currentPanel = key;
    currentLessonSelection = key === "content" ? currentLessonSelection : null;
    renderSidebar();
    const panel = document.getElementById("instructorPanel");
    if (!panel) return;
    const course = currentCourse();
    if (key === "desc") {
        panel.innerHTML = '<p class="empty-note">Загружаем данные курса…</p>';
        try { await loadCourseDescription(course); } 
        catch (error) {
            console.error("Не удалось загрузить описание курса:", error);
            panel.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить курс.")}</p>`;
            showToast("Не удалось загрузить курс", error.message || "Проверьте подключение и попробуйте снова.");
            return;
        }
    }
    if (["news", "reviews", "price", "access", "publish"].includes(key)) {
        panel.innerHTML = '<p class="empty-note">Загружаем данные с сервера…</p>';
        try { await loadEditorPanelData(key, course); } 
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

async function saveDesc() {
    const c = currentCourse();
    const title = document.getElementById("pTitle").value.trim();
    const description = document.getElementById("pDesc").value.trim();
    const subSectionId = Number(document.getElementById("pSubCategory").value);
    const selectedPath = TEACHER_PATHS.find(path => String(path.id) === document.getElementById("pCategory").value);
    const selectedSubSection = selectedPath?.subs.find(sub => sub.id === subSectionId);
    const button = document.querySelector("#instructorPanel button[onclick='saveDesc()']");

    if (title.length < 5 || title.length > 200) { showToast("Ошибка", "Название курса должно содержать от 5 до 200 символов."); return; }
    if (description.length < 10 || description.length > 2000) { showToast("Ошибка", "Описание курса должно содержать от 10 до 2000 символов."); return; }
    if (!selectedSubSection) { showToast("Ошибка", "Выберите категорию и одну из её подкатегорий."); return; }

    const originalText = button?.innerHTML;
    if (button) { button.disabled = true; button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Сохраняем...'; }

    try {
        const response = await apiFetch(`/api/courses/ChangeCourseDescription?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ courseName: title, courseDescription: description, skillSubSectionId: subSectionId })
        });
        const payload = await response.json().catch(() => null);
        unwrapApiResult(payload, "Не удалось сохранить изменения курса.");
        if (!response.ok) throw new Error(payload?.error || payload?.title || `Не удалось сохранить курс (${response.status}).`);

        c.title = title; c.desc = description; c.description = description;
        c.skillSubSectionId = subSectionId; c.category = selectedPath.name; c.sub = selectedSubSection.name;
        document.getElementById("editorCourseTitle").innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(c.title)}`;
        showToast("Сохранено! 💾", "Название, описание и подкатегория курса обновлены.");
    } catch (error) {
        console.error("Не удалось сохранить изменения курса:", error);
        showToast("Не удалось сохранить", error.message || "Проверьте соединение и повторите попытку.");
    } finally {
        if (button) { button.disabled = false; button.innerHTML = originalText; }
    }
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

function addModule() {
    const course = currentCourse();
    course.modules.push({ id: 0, title: `Новый модуль ${course.modules.length + 1}`, lessons: [] });
    currentLessonSelection = null;
    openPanel("content");
}

function addLesson(moduleIndex) {
    const course = currentCourse();
    const xpByDifficulty = { 1: 15, 2: 30, 3: 50 };
    course.modules[moduleIndex].lessons.push({
        id: 0, title: "Новый урок", xpReward: xpByDifficulty[course.difficulty] ?? 15,
        blocks: [{ id: 0, type: 0, content: "<p></p>", order: 0 }]
    });
    currentLessonSelection = { moduleIndex, lessonIndex: course.modules[moduleIndex].lessons.length - 1 };
    openPanel("content");
}

function openLessonEditor(moduleIndex, lessonIndex) {
    currentLessonSelection = { moduleIndex, lessonIndex };
    openPanel("content");
}

function backToCourseContent() {
    currentLessonSelection = null;
    openPanel("content");
}

function updateModuleTitle(moduleIndex, value) { currentCourse().modules[moduleIndex].title = value; }
function updateLessonTitle(moduleIndex, lessonIndex, value) { currentCourse().modules[moduleIndex].lessons[lessonIndex].title = value; }

function updateLessonXp(moduleIndex, lessonIndex, value) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    lesson.xpReward = Number(value) || 0;
    lesson.blocks.filter(block => block.type === 3).forEach(block => {
        try {
            const quiz = JSON.parse(block.content);
            quiz.xp = lesson.xpReward;
            block.content = JSON.stringify(quiz);
        } catch {
            block.content = JSON.stringify({ question: "", options: ["", ""], answer: 0, xp: lesson.xpReward, coins: 0 });
        }
    });
}

function updateTextBlock(moduleIndex, lessonIndex, blockIndex, editor) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = sanitizeLessonHtml(editor.innerHTML);
}

function updateMediaBlock(moduleIndex, lessonIndex, blockIndex, value) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = value.trim();
}

function updateQuizBlock(moduleIndex, lessonIndex, blockIndex) {
    const block = currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex];
    const quizKey = `${moduleIndex}-${lessonIndex}-${blockIndex}`;
    const options = [...document.querySelectorAll(`#instructorPanel .quiz-option-input[data-quiz-block="${quizKey}"]`)]
        .sort((left, right) => Number(left.dataset.optionIndex) - Number(right.dataset.optionIndex))
        .map(input => input.value);
    const selectedAnswer = document.querySelector(`#instructorPanel input[name="quiz-correct-${moduleIndex}-${lessonIndex}-${blockIndex}"]:checked`);
    block.content = JSON.stringify({
        question: document.getElementById(`quiz-question-${moduleIndex}-${lessonIndex}-${blockIndex}`).value.trim(),
        options, answer: selectedAnswer ? Number(selectedAnswer.value) : 0,
        xp: currentCourse().modules[moduleIndex].lessons[lessonIndex].xpReward, coins: 0
    });
}

function addQuizOption(moduleIndex, lessonIndex, blockIndex) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    const block = lesson.blocks[blockIndex];
    const quiz = JSON.parse(block.content || "{}");
    const options = Array.isArray(quiz.options) ? quiz.options : ["", ""];
    if (options.length >= 6) return;

    updateQuizBlock(moduleIndex, lessonIndex, blockIndex);
    const updatedQuiz = JSON.parse(block.content);
    updatedQuiz.options.push("");
    block.content = JSON.stringify(updatedQuiz);
    document.getElementById("instructorPanel").innerHTML = renderLessonEditor(currentCourse(), moduleIndex, lessonIndex);
    document.querySelector(`#instructorPanel .quiz-option-input[data-option-index="${updatedQuiz.options.length - 1}"]`)?.focus();
}

function removeQuizOption(moduleIndex, lessonIndex, blockIndex, optionIndex) {
    const block = currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex];
    updateQuizBlock(moduleIndex, lessonIndex, blockIndex);
    const quiz = JSON.parse(block.content);
    if (quiz.options.length <= 2) return;

    quiz.options.splice(optionIndex, 1);
    if (quiz.answer === optionIndex) quiz.answer = Math.max(0, optionIndex - 1);
    else if (quiz.answer > optionIndex) quiz.answer -= 1;
    block.content = JSON.stringify(quiz);
    document.getElementById("instructorPanel").innerHTML = renderLessonEditor(currentCourse(), moduleIndex, lessonIndex);
}

function updateAssignmentBlock(moduleIndex, lessonIndex, blockIndex) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = JSON.stringify({
        prompt: document.getElementById(`assignment-prompt-${blockIndex}`).value.trim()
    });
}

function addLessonBlock(moduleIndex, lessonIndex, type) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    const defaults = {
        0: "", 1: "", 2: "",
        3: JSON.stringify({ question: "", options: ["", ""], answer: 0, xp: lesson.xpReward, coins: 0 }),
        4: JSON.stringify({ prompt: "" })
    };
    const blockIndex = lesson.blocks.length;
    lesson.blocks.push({ id: 0, type, content: defaults[type], order: blockIndex });
    openPanel("content").then(() => {
        if (type === 0) {
            const editor = document.getElementById(`lessonText-${moduleIndex}-${lessonIndex}-${blockIndex}`);
            editor?.focus();
        }
    });
}

function removeLessonBlock(moduleIndex, lessonIndex, blockIndex) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks.splice(blockIndex, 1);
    openPanel("content");
}

function formatLessonText(command) {
    document.execCommand(command, false);
    const selection = window.getSelection();
    const editor = selection?.anchorNode?.parentElement?.closest(".lesson-rich-editor");
    if (editor) {
        const [moduleIndex, lessonIndex, blockIndex] = editor.id.replace("lessonText-", "").split("-").map(Number);
        updateTextBlock(moduleIndex, lessonIndex, blockIndex, editor);
    }
}

function normalizeVideoUrl(value) {
    try {
        const url = new URL(value);
        if (url.protocol !== "https:") return null;
        if (url.hostname === "youtu.be") {
            const videoId = url.pathname.slice(1);
            return videoId ? `https://www.youtube-nocookie.com/embed/${encodeURIComponent(videoId)}` : null;
        }
        if (["youtube.com", "www.youtube.com", "m.youtube.com"].includes(url.hostname)) {
            const videoId = url.searchParams.get("v") ?? url.pathname.match(/^\/embed\/([^/]+)/)?.[1];
            return videoId ? `https://www.youtube-nocookie.com/embed/${encodeURIComponent(videoId)}` : null;
        }
        if (url.hostname === "vimeo.com") {
            const videoId = url.pathname.split("/").filter(Boolean).at(-1);
            return videoId && /^\d+$/.test(videoId) ? `https://player.vimeo.com/video/${videoId}` : null;
        }
        return ["www.youtube-nocookie.com", "player.vimeo.com"].includes(url.hostname) ? url.href : null;
    } catch { return null; }
}

async function saveCourseContent() {
    const course = currentCourse();
    const button = document.querySelector("#instructorPanel .save-course-content");
    const originalText = button?.innerHTML;

    try {
        for (const module of course.modules) {
            if (!module.title.trim()) throw new Error("Укажите название каждого модуля.");
            for (const lesson of module.lessons) {
                if (!lesson.title.trim()) throw new Error("Укажите название каждого урока.");
                if (lesson.xpReward < 0 || !Number.isInteger(Number(lesson.xpReward))) throw new Error("Награда за урок должна быть целым числом XP от 0.");
                for (const block of lesson.blocks) {
                    if (block.type === 1) {
                        const videoUrl = normalizeVideoUrl(block.content);
                        if (!videoUrl) throw new Error("Для видео укажите HTTPS-ссылку на YouTube или Vimeo.");
                        block.content = videoUrl;
                    }
                    if (block.type === 2) {
                        let imageUrl;
                        try { imageUrl = new URL(block.content); } catch { throw new Error("Для изображения укажите корректную HTTPS-ссылку."); }
                        if (imageUrl.protocol !== "https:") throw new Error("Ссылка на изображение должна использовать HTTPS.");
                        block.content = imageUrl.href;
                    }
                    if (block.type === 3) {
                        let quiz;
                        try {
                            quiz = JSON.parse(block.content);
                        } catch {
                            throw new Error("Проверьте данные квиза: не удалось прочитать вопрос и варианты ответа.");
                        }
                        quiz.question = String(quiz.question ?? "").trim();
                        quiz.options = Array.isArray(quiz.options) ? quiz.options.map(option => String(option).trim()) : [];
                        if (!quiz.question) throw new Error("Заполните вопрос в каждом квизе.");
                        if (quiz.options.length < 2 || quiz.options.length > 6 || quiz.options.some(option => !option)) {
                            throw new Error("В каждом квизе должно быть от 2 до 6 непустых вариантов ответа.");
                        }
                        if (!Number.isInteger(quiz.answer) || quiz.answer < 0 || quiz.answer >= quiz.options.length) {
                            throw new Error("Отметьте правильный вариант ответа в каждом квизе.");
                        }
                        quiz.xp = Number(lesson.xpReward) || 0;
                        quiz.coins = 0;
                        block.content = JSON.stringify(quiz);
                    }
                }
            }
        }

        if (button) { button.disabled = true; button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Сохраняем…'; }

        const response = await apiFetch(`/api/courses/UpdateCourseContent?courseId=${encodeURIComponent(course.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                sections: course.modules.map(module => ({
                    id: module.id, title: module.title,
                    lessons: module.lessons.map(lesson => ({
                        id: lesson.id, title: lesson.title, xpReward: Number(lesson.xpReward) || 0,
                        blocks: lesson.blocks.map((block, order) => ({ type: block.type, content: block.content, order }))
                    }))
                }))
            })
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            const result = payload?.value ?? payload?.Value ?? payload;
            const errors = result?.errors ?? result?.Errors ?? payload?.errors ?? payload?.Errors ?? [];
            const messages = errors.map(error => typeof error === "string" ? error : error.errorMessage ?? error.ErrorMessage).filter(Boolean);
            throw new Error(messages.join("\n") || payload?.error || payload?.title || `Не удалось сохранить содержание (${response.status}).`);
        }
        applyCourseDescription(course, unwrapApiResult(payload, "Не удалось сохранить содержание курса."));

        currentLessonSelection = null;
        await openPanel("content");
        showToast("Содержание сохранено", "Модули, уроки, материалы и награды XP обновлены.");
    } catch (error) {
        console.error("Не удалось сохранить содержание курса:", error);
        showToast("Не удалось сохранить", error.message || "Проверьте данные и повторите попытку.");
    } finally {
        if (button) { button.disabled = false; button.innerHTML = originalText; }
    }
}

function acceptSolution(idx) {
    const c = currentCourse(); const s = c.solutions[idx];
    c.solutions.splice(idx, 1); openPanel("solutions");
    showToast("Решение принято! ✅", `${s.student} получит XP и монеты за задание.`);
}

function returnSolution(idx) {
    const c = currentCourse(); const s = c.solutions[idx];
    c.solutions.splice(idx, 1); openPanel("solutions");
    showToast("Возвращено на доработку", `${s.student} получит комментарий с замечаниями.`);
}

async function publishNews() {
    const c = currentCourse();
    const text = document.getElementById("newsText").value.trim();
    if (text.length < 3) { showToast("Пусто!", "Напиши текст новости."); return; }
    try {
        const publishedNews = await courseApiRequest(`/api/courses/PublishCourseNews?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ text })
        });
        c.news = [publishedNews, ...(c.news || []).filter(news => news.id !== publishedNews.id)];
        c.newsCount = Math.max(Number(c.newsCount) || 0, c.news.length);
        document.getElementById("newsText").value = "";
        await openPanel("news");
        if (c.status !== "published") {
            showToast("Новость сохранена", "Курс ещё в разработке, поэтому уведомление учащимся не отправлено. Для уведомления опубликуйте курс и новость.");
        } else if ((publishedNews.notifiedStudents ?? publishedNews.NotifiedStudents ?? 0) > 0) {
            const notifiedStudents = publishedNews.notifiedStudents ?? publishedNews.NotifiedStudents;
            showToast("Новость опубликована! 📢", `Уведомление отправлено учащимся курса: ${notifiedStudents}.`);
        } else {
            showToast("Новость опубликована! 📢", "Новость сохранена. На курс пока никто не записан, поэтому уведомлять некого.");
        }
    } catch (error) { showToast("Не удалось опубликовать новость", error.message); }
}

async function publishCourse() {
    const c = currentCourse();
    try {
        const readiness = await courseApiRequest(`/api/courses/PublishCourse?courseId=${encodeURIComponent(c.id)}`, { method: "POST" });
        c.status = "published"; c.readiness = readiness;
        await openPanel("publish"); await loadTeacherCourses();
        showToast("Курс опубликован! 🌍", `«${c.title}» теперь виден студентам в каталоге.`);
    } catch (error) { showToast("Не удалось опубликовать курс", error.message); }
}

async function unpublishCourse() {
    const c = currentCourse();
    try {
        await courseApiRequest(`/api/courses/UnpublishCourse?courseId=${encodeURIComponent(c.id)}`, { method: "POST" });
        c.status = "draft";
        await openPanel("publish"); await loadTeacherCourses();
        showToast("Курс скрыт", "Курс больше не отображается в каталоге студентов.");
    } catch (error) { showToast("Не удалось снять курс с публикации", error.message); }
}

function archiveCourse(id) {
    const c = id ? getCourse(id) : currentCourse();
    c.status = "archived";
    if (id) renderTeacherGrid(); else openPanel("publish");
    renderTeacherHero();
    showToast("В архиве 📦", `«${c.title}» перемещён в архив.`);
}

function restoreCourse(id) {
    const c = id ? getCourse(id) : currentCourse();
    c.status = "draft";
    if (id) renderTeacherGrid(); else openPanel("publish");
    renderTeacherHero();
    showToast("Возвращён 🔄", `«${c.title}» снова в разработке.`);
}

async function savePrice() {
    const c = currentCourse();
    const isFree = document.getElementById("pFree").checked;
    const price = parseInt(document.getElementById("pPrice").value, 10) || 0;
    if (isFree ? price !== 0 : price <= 0) { showToast("Некорректная цена", "Для бесплатного курса цена равна 0, для платного — больше 0."); return; }
    try {
        await courseApiRequest(`/api/courses/SaveCoursePrice?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ isPaid: !isFree, priceCoins: price })
        });
        c.isFree = isFree; c.isPaid = !isFree; c.price = price;
        showToast("Цена сохранена! 🏷️", c.isFree ? "Курс бесплатный." : `Цена: ${c.price} монет.`);
    } catch (error) { showToast("Не удалось сохранить цену", error.message); }
}

async function addPromo() {
    const c = currentCourse();
    const code = document.getElementById("promoCode").value.trim().toUpperCase();
    const discount = parseInt(document.getElementById("promoDiscount").value, 10) || 0;
    if (code.length < 3 || discount < 1 || discount > 100) { showToast("Ошибка", "Нужен код (3–32 символа) и скидка 1–100%."); return; }
    try {
        await courseApiRequest(`/api/courses/AddCoursePromo?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ code, discountPercent: discount })
        });
        await openPanel("price");
        showToast("Промокод создан! 🎟️", `${code} даёт скидку ${discount}%.`);
    } catch (error) { showToast("Не удалось создать промокод", error.message); }
}

async function removePromo(idx) {
    const c = currentCourse(); const promo = c.promo[idx];
    if (!promo) return;
    try {
        await courseApiRequest(`/api/courses/RemoveCoursePromo?courseId=${encodeURIComponent(c.id)}&promoId=${promo.id}`, { method: "DELETE" });
        await openPanel("price");
    } catch (error) { showToast("Не удалось удалить промокод", error.message); }
}

async function addCoTeacher() {
    const c = currentCourse();
    const name = document.getElementById("coName").value.trim();
    if (name.length < 2) { showToast("Ошибка", "Введи ник со-автора."); return; }
    try {
        await courseApiRequest(`/api/courses/InviteCourseTeacher?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ userName: name })
        });
        await openPanel("access");
        showToast("Доступ выдан! 🛡️", `${name} получит уведомление и сможет редактировать курс.`);
    } catch (error) { showToast("Не удалось выдать доступ", error.message); }
}

async function removeCoTeacher(idx) {
    const c = currentCourse(); const teacher = c.coTeachers[idx];
    if (!teacher) return;
    try {
        await courseApiRequest(`/api/courses/RemoveCourseTeacher?courseId=${encodeURIComponent(c.id)}&userName=${encodeURIComponent(teacher.userName)}`, { method: "DELETE" });
        await openPanel("access");
        showToast("Доступ снят", `${teacher.userName} больше не со-автор.`);
    } catch (error) { showToast("Не удалось снять доступ", error.message); }
}

async function saveCert() {
    const c = currentCourse();
    const enabled = document.getElementById("certEnabled").checked;
    const signer = document.getElementById("certSigner").value.trim();
    try {
        await courseApiRequest(`/api/courses/SaveCourseCertificate?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ enabled, signer })
        });
        c.cert = { enabled, signer };
        showToast("Сертификаты настроены! 🏆", enabled ? "Студенты смогут скачать сертификат после завершения." : "Выдача сертификатов выключена.");
    } catch (error) { showToast("Не удалось сохранить настройки сертификата", error.message); }
}

// ============================ ИНИЦИАЛИЗАЦИЯ ============================
document.addEventListener("DOMContentLoaded", async () => {
    window.teacherCourses = teacherCourses;
    
    document.getElementById("btnOpenCreateCourse").addEventListener("click", openCreateModal);
    document.getElementById("btnCloseCreateCourse").addEventListener("click", closeCreateModal);
    document.getElementById("btnCreateCourse").addEventListener("click", createCourse);
    document.getElementById("btnLeaveEditor").addEventListener("click", closeEditor);

    document.getElementById("teacherCoursesGrid").addEventListener("click", (e) => {
        const btn = e.target.closest("button[data-action]");
        if (!btn) return;
        const courseId = Number(btn.dataset.id);
        if (btn.dataset.action === "edit") openEditor(courseId);
        else if (btn.dataset.action === "archive" || btn.dataset.action === "restore") toggleTeacherCourseArchive(courseId);
        else if (btn.dataset.action === "like") toggleTeacherCourseLike(courseId);
    });

    document.querySelectorAll(".dashboard-filters-zone .tab-filter").forEach(chip => {
        chip.addEventListener("click", async () => {
            document.querySelectorAll(".dashboard-filters-zone .tab-filter").forEach(tab => tab.classList.remove("active"));
            chip.classList.add("active");
            teacherCollection = chip.dataset.collection || "draft";
            const requestVersion = ++teacherCollectionRequestVersion;
            if (teacherCollection === "liked" || teacherCollection === "archived") {
                teacherCollectionLoading = true;
                teacherCollectionCourses = [];
                renderTeacherGrid();
                try {
                    await loadTeacherCollection(teacherCollection, requestVersion);
                } catch (error) {
                    console.error(`Не удалось загрузить коллекцию ${teacherCollection}:`, error);
                    showToast("Не удалось загрузить курсы", error.message || "Проверьте соединение и повторите попытку.");
                } finally {
                    if (requestVersion === teacherCollectionRequestVersion) {
                        teacherCollectionLoading = false;
                        renderTeacherGrid();
                    }
                }
            } else {
                teacherCollectionLoading = false;
            }
            renderTeacherGrid();
        });
    });
    document.getElementById("teacherRailPrev").addEventListener("click", () => {
        document.getElementById("teacherCoursesGrid").scrollBy({ left: -320, behavior: "smooth" });
    });
    document.getElementById("teacherRailNext").addEventListener("click", () => {
        document.getElementById("teacherCoursesGrid").scrollBy({ left: 320, behavior: "smooth" });
    });


    try { await initCreateCourseModal(); } 
    catch (error) {
        console.error("Не удалось загрузить направления для создания курса:", error);
        showToast("Не удалось загрузить форму", error.message || "Попробуйте обновить страницу.");
    }

    try {
        const response = await apiFetch("/api/user/GetMyBusinessPassport");
        if (!response.ok) throw new Error(`Не удалось загрузить профиль (${response.status}).`);
        const data = await response.json().catch(() => null);
        const passport = unwrapApiResult(data, "Не удалось загрузить профиль преподавателя.");
        const user = passport.user ?? passport.User ?? {};
        const avatarUrl = user.avatarUrl ?? user.AvatarUrl;
        if (avatarUrl) {
            document.querySelectorAll(".saas-avatar-mini").forEach(avatar => {
                avatar.style.backgroundImage = `url("${avatarUrl.replaceAll('"', '%22')}")`;
            });
        }
        const nick = document.getElementById("saas-username-display");
        const lvl = document.getElementById("hud-level-badge");
        if (nick) nick.textContent = passport.userName ?? passport.UserName ?? "";
        if (lvl && (passport.levelingProgress ?? passport.LevelingProgress)) {
            const progress = passport.levelingProgress ?? passport.LevelingProgress;
            lvl.textContent = `Lvl ${progress.level ?? progress.Level}`;
        }
        const coins = user.coinsBalance ?? user.CoinsBalance;
        const coinsElement = document.getElementById("hudUserCoinsDisplay");
        if (coins !== undefined && coinsElement) {
            coinsElement.textContent = Number(coins).toLocaleString("ru-RU");
        }
    } catch (error) {
        console.error("[Learn] Не удалось загрузить профиль преподавателя:", error);
        showToast("Не удалось загрузить профиль", error.message || "Проверьте подключение и войдите в аккаунт.");
    }

    try { await loadTeacherCourses(); } 
    catch (error) {
        console.error("[Learn] Не удалось загрузить курсы преподавателя:", error);
        showToast("Не удалось открыть курсы", error.message || "Проверьте подключение и профиль преподавателя.");
    }

    renderTeacherGrid();
    renderTeacherHero();
});

window.openEditor = openEditor;
window.openPanel = openPanel;
window.openCreateModal = openCreateModal;
window.renderTeacherGrid = renderTeacherGrid;
window.saveDesc = saveDesc;
window.addModule = addModule;
window.addLesson = addLesson;
window.openLessonEditor = openLessonEditor;
window.backToCourseContent = backToCourseContent;
window.updateModuleTitle = updateModuleTitle;
window.updateLessonTitle = updateLessonTitle;
window.updateLessonXp = updateLessonXp;
window.updateTextBlock = updateTextBlock;
window.updateMediaBlock = updateMediaBlock;
window.updateQuizBlock = updateQuizBlock;
window.addQuizOption = addQuizOption;
window.removeQuizOption = removeQuizOption;
window.updateAssignmentBlock = updateAssignmentBlock;
window.addLessonBlock = addLessonBlock;
window.removeLessonBlock = removeLessonBlock;
window.formatLessonText = formatLessonText;
window.saveCourseContent = saveCourseContent;
window.acceptSolution = acceptSolution;
window.returnSolution = returnSolution;
window.publishNews = publishNews;
window.publishCourse = publishCourse;
window.unpublishCourse = unpublishCourse;
window.archiveCourse = archiveCourse;
window.restoreCourse = restoreCourse;
window.savePrice = savePrice;
window.addPromo = addPromo;
window.removePromo = removePromo;
window.addCoTeacher = addCoTeacher;
window.removeCoTeacher = removeCoTeacher;
window.saveCert = saveCert;