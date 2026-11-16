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
        { average: Number(row.averageProgress ?? row.AverageProgress) || 0 }
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
    } else if (key === "reviews") {
        course.reviews = await courseApiRequest(`/api/courses/GetCourseReviews?${query}`);
        course.rating = course.reviews.length ? course.reviews.reduce((total, review) => total + review.stars, 0) / course.reviews.length : 0;
    } else if (key === "price") {
        course.promo = await courseApiRequest(`/api/courses/GetCoursePromos?${query}`);
    } else if (key === "access") {
        course.coTeachers = await courseApiRequest(`/api/courses/GetCourseTeachers?${query}`);
    } else if (key === "publish") {
        course.readiness = await courseApiRequest(`/api/courses/GetCourseReadiness?${query}`);
    }
}