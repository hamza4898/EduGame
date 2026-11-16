function escapeHtml(s) {
    return String(s ?? "").replace(/[&<>"']/g, c => ({ 
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' 
    }[c]));
}

const sectionTitles = {
    it: "Путь IT", design: "Путь Творца", lang: "Путь Полиглота",
    business: "Путь Стратега", eng: "Путь Зодчего", growth: "Путь Восхождения"
};

function courseChecks(course) {
    return Math.max(0, Math.min(100, course.readinessPercent || 0));
}

function unwrapApiPayload(payload) {
    return payload?.value ?? payload?.Value ?? payload;
}

function getField(value, ...keys) {
    for (const key of keys) {
        if (value?.[key] !== undefined && value[key] !== null) return value[key];
    }
    return undefined;
}

async function fetchApiRows(url, errorMessage) {
    const response = await apiFetch(url);
    const payload = await response.json().catch(() => null);
    if (!response.ok || payload?.isSuccess === false || payload?.IsSuccess === false) {
        throw new Error(errorMessage);
    }
    const rows = unwrapApiPayload(payload);
    if (!Array.isArray(rows)) throw new Error("Сервер вернул некорректный формат данных.");
    return rows;
}

const teacherRankTitles = {
    Assistant: "Ассистент",
    Instructor: "Инструктор",
    Lector: "Лектор",
    Curator: "Куратор",
    Mentor: "Ментор",
    Professor: "Профессор",
    AcademyMagister: "Магистр Академии",
    Dean: "Декан",
    KnowledgeArchitect: "Архитектор знаний",
    Grandmaster: "Великий магистр"
};

let teacherCoursesData = [];
let teacherLeaderboardData = { global: [], vuz: [], weekly: [] };
let teacherStudentsData = null;
let currentTeacherUserId = "";
let leaderboardRequestVersion = 0;

async function loadTeacherDashboardData() {
    const [coursesResponse, studentCounts, ratings, progress, completionCounts] = await Promise.all([
        apiFetch("/api/courses/GetMyTeacherCourses"),
        fetchApiRows("/api/courses/GetMyTeacherCourseStudentCounts", "Не удалось загрузить количество учащихся по курсам."),
        fetchApiRows("/api/courses/GetMyTeacherCourseRatings", "Не удалось загрузить рейтинги курсов."),
        fetchApiRows("/api/courses/GetMyTeacherCourseProgress", "Не удалось загрузить прогресс по курсам."),
        fetchApiRows("/api/courses/GetMyTeacherCourseCompletionCounts", "Не удалось загрузить количество завершений курсов.")
    ]);
    const coursesPayload = await coursesResponse.json().catch(() => null);
    if (!coursesResponse.ok || coursesPayload?.isSuccess === false || coursesPayload?.IsSuccess === false) {
        const errors = coursesPayload?.errors ?? coursesPayload?.Errors;
        const errorMessage = Array.isArray(errors)
            ? errors.map(error => typeof error === "string" ? error : error.errorMessage ?? error.ErrorMessage).filter(Boolean).join("\n")
            : "";
        throw new Error(errorMessage || coursesPayload?.error || coursesPayload?.title || "Не удалось загрузить курсы преподавателя.");
    }

    const rows = unwrapApiPayload(coursesPayload);
    if (!Array.isArray(rows)) throw new Error("Сервер вернул некорректный список курсов.");
    const studentCountByCourse = new Map(studentCounts.map(row => [
        Number(getField(row, "courseId", "CourseId")),
        Number(getField(row, "studentCount", "StudentCount")) || 0
    ]));
    const ratingByCourse = new Map(ratings.map(row => [
        Number(getField(row, "courseId", "CourseId")),
        Number(getField(row, "rating", "Rating")) || 0
    ]));
    const progressByCourse = new Map(progress.map(row => [
        Number(getField(row, "courseId", "CourseId")),
        Number(getField(row, "averageProgress", "AverageProgress")) || 0
    ]));
    const completionCountByCourse = new Map(completionCounts.map(row => [
        Number(getField(row, "courseId", "CourseId")),
        Number(getField(row, "completedCount", "CompletedCount")) || 0
    ]));
    teacherCoursesData = rows.map(row => ({
        id: row.id ?? row.Id,
        title: row.title ?? row.Title ?? "",
        desc: row.description ?? row.Description ?? "",
        isOwnedByTeacher: row.isOwnedByTeacher ?? row.IsOwnedByTeacher ?? false,
        status: (row.isPublished ?? row.IsPublished) ? "published" : "draft",
        price: row.priceCoins ?? row.PriceCoins ?? 0,
        isFree: !(row.isPaid ?? row.IsPaid),
        students: studentCountByCourse.get(Number(row.id ?? row.Id)) || 0,
        rating: ratingByCourse.get(Number(row.id ?? row.Id)) || 0,
        avgProgress: progressByCourse.get(Number(row.id ?? row.Id)) || 0,
        completed: completionCountByCourse.get(Number(row.id ?? row.Id)) || 0,
        readinessPercent: row.readinessPercent ?? row.ReadinessPercent ?? 0,
        isReadyToPublish: row.isReadyToPublish ?? row.IsReadyToPublish ?? false
    }));
    await loadTeacherAchievements();
    renderTeacherCoursesList();
    await loadLeaderboard('global');
}

async function loadTeacherAchievements() {
    const container = document.getElementById("teacherAchievements");
    if (!container) return;

    container.innerHTML = `<p class="empty-note">Загружаем заслуги…</p>`;
    try {
        const awards = await fetchApiRows(
            "/api/courses/GetMyTeacherXpAwards",
            "Не удалось загрузить историю начислений XP."
        );
        const reasonLabels = {
            PublishedCourse: "Опубликован и проверен готовый курс",
            StudentCompletion: "Учащийся полностью завершил курс",
            CompletionMilestone: "Достигнут рубеж завершений курса",
            CourseReview: "Получен развёрнутый отзыв от выпускника"
        };
        const difficultyLabels = { 1: "Лёгкий", 2: "Средний", 3: "Сложный", Easy: "Лёгкий", Medium: "Средний", Hard: "Сложный" };
        const earnedAwards = awards.map(award => {
            const reason = getField(award, "reason", "Reason");
            const xp = Number(getField(award, "xp", "Xp")) || 0;
            const courseTitle = getField(award, "courseTitle", "CourseTitle") || "Курс";
            const difficulty = getField(award, "difficulty", "Difficulty");
            return `
                <div class="achieve-item teacher-achievement unlocked">
                    <i class="fa-solid fa-award item-icon-green"></i>
                    <div>
                        <strong>${escapeHtml(reasonLabels[reason] || "Заслуга наставника")}</strong>
                        <p>${escapeHtml(courseTitle)}${difficulty !== undefined ? ` · ${escapeHtml(difficultyLabels[difficulty] || "сложность не указана")}` : ""}</p>
                        <span class="achievement-progress">+${xp} XP</span>
                    </div>
                </div>`;
        }).join("");

    container.innerHTML = `
        <div class="achieve-section">
            <h4><i class="fa-solid fa-scroll" style="color:#10b981;"></i> Как начисляется опыт</h4>
            <div class="teacher-xp-rules">
                <p><strong>+100 / +200 / +300 XP</strong> — за публикацию готового курса: лёгкий / средний / сложный.</p>
                <p><strong>+75 / +150 / +225 XP</strong> — за каждого подтверждённого выпускника старше 7 дней (максимум 50 начислений на курс).</p>
                <p><strong>+100 / +250 / +500 XP</strong> — за рубежи в 5 / 10 / 25 таких выпускников; сумма умножается на сложность.</p>
                <p><strong>+25 / +50 / +75 XP</strong> — за отзыв выпускника старше 7 дней от 30 символов (максимум 50 начислений на курс).</p>
            </div>
            <h4 class="teacher-earned-heading"><i class="fa-solid fa-medal" style="color:#f59e0b;"></i> Последние начисления</h4>
            ${earnedAwards || `<p class="empty-note">Пока начислений нет. Опубликуйте готовый курс, чтобы начать.</p>`}
        </div>`;
    } catch (error) {
        console.error("Не удалось загрузить достижения преподавателя:", error);
        container.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить достижения.")}</p>`;
    }
}

function renderTeacherCoursesList() {
    const container = document.getElementById("teacherCoursesList");
    if (!container) return;

    if (teacherCoursesData.length === 0) {
        container.innerHTML = `<p class="empty-note">У вас пока нет созданных курсов.</p>`;
        return;
    }

    container.innerHTML = teacherCoursesData.map(course => {
        const readiness = courseChecks(course);
        const barPct = course.status === "draft" ? readiness : course.avgProgress;
        const statusColor = course.status === "draft" ? "#f59e0b" : (course.status === "published" ? "#10b981" : "#64748b");
        const statusIcon = course.status === "draft" ? "fa-hammer" : (course.status === "published" ? "fa-globe" : "fa-box-archive");

        return `
        <div class="skill-mini-box cursor-pointer" onclick="openCourseAnalytics(${course.id})" style="cursor: pointer;">
            <div class="skill-meta">
                <span><i class="fa-solid ${statusIcon}" style="color:${statusColor}; margin-right:6px; font-size:11px;"></i>${escapeHtml(course.title)}</span>
                <span class="skill-val"><i class="fa-solid fa-users" style="font-size:10px;"></i> ${course.students || 0} <i class="fa-solid fa-chevron-right" style="font-size:10px; margin-left:4px;"></i></span>
            </div>
            <div class="skill-progress"><div class="skill-fill-line green" style="width:${barPct}%; background: ${statusColor}; box-shadow: 0 0 8px ${statusColor};"></div></div>
        </div>`;
    }).join("");
}

function openCourseAnalytics(id) {
    const c = teacherCoursesData.find(x => x.id === id);
    if (!c) return;

    const readiness = courseChecks(c);
    const finished = c.completed || 0;

    document.getElementById("analyticsModalTitle").innerHTML = `<i class="fa-solid fa-chart-simple" style="color:#a855f7;"></i> Аналитика: ${escapeHtml(c.title)}`;

    document.getElementById("analyticsModalBody").innerHTML = `
        <div class="teacher-summary-chips">
            <div class="teacher-chip"><i class="fa-solid fa-users"></i><b>${c.students || 0}</b>учеников</div>
            <div class="teacher-chip chip-amber"><i class="fa-solid fa-chart-line"></i><b>${Number(c.avgProgress || 0).toFixed(1)}%</b>средний прогресс</div>
        </div>

        <div class="stat-cards-row">
            <div class="stat-card"><b>${c.rating > 0 ? "⭐ " + c.rating : "—"}</b><span>рейтинг</span></div>
            <div class="stat-card"><b>${c.students || 0}</b><span>записано учащихся</span></div>
            <div class="stat-card"><b>${readiness}%</b><span>готовность курса</span></div>
            <div class="stat-card"><b>${finished}</b><span>завершили</span></div>
        </div>

        <div class="analytics-block">
            <h4><i class="fa-solid fa-chart-line"></i> Прогресс обучения</h4>
            <p>Средний прогресс записанных студентов: ${Number(c.avgProgress || 0).toFixed(1)}%</p>
        </div>

        <button class="btn-sm violet" onclick="window.location.href='LearnTeacher.html'">
            <i class="fa-solid fa-pen-to-square"></i> Открыть в редакторе курса
        </button>`;

    document.getElementById("courseAnalyticsModal").classList.add("open");
}

function closeCourseAnalytics() {
    document.getElementById("courseAnalyticsModal").classList.remove("open");
}

async function loadTeacherStudents() {
    if (teacherStudentsData) return teacherStudentsData;
    const rows = await fetchApiRows(
        "/api/courses/GetMyTeacherCourseStudents",
        "Не удалось загрузить список записавшихся учащихся."
    );
    teacherStudentsData = rows.map(row => ({
        studentId: getField(row, "studentId", "StudentId") ?? "",
        studentName: getField(row, "studentName", "StudentName") ?? "Ученик",
        avatarUrl: getField(row, "avatarUrl", "AvatarUrl"),
        courseId: getField(row, "courseId", "CourseId"),
        courseTitle: getField(row, "courseTitle", "CourseTitle") ?? "Курс",
        progressPercent: Math.max(0, Math.min(100, Number(getField(row, "progressPercent", "ProgressPercent")) || 0)),
        isCompleted: Boolean(getField(row, "isCompleted", "IsCompleted")),
        enrolledAt: getField(row, "enrolledAt", "EnrolledAt")
    }));
    return teacherStudentsData;
}

function renderTeacherStudentRow(student) {
    const avatar = student.avatarUrl || "images/hamzaBro3.png";
    const enrolled = student.enrolledAt
        ? new Date(student.enrolledAt).toLocaleDateString("ru-RU")
        : "дата не указана";
    return `
        <div class="student-row-item">
            <div class="student-row-top">
                <a class="teacher-student-profile" href="Profile.html?id=${encodeURIComponent(student.studentId)}">
                    <img src="${escapeHtml(avatar)}" alt="" onerror="this.src='images/hamzaBro3.png'">
                    <span class="student-row-name">${escapeHtml(student.studentName)}</span>
                </a>
                <span class="student-status-chip ${student.isCompleted ? "st-top" : "st-ok"}">
                    ${student.isCompleted ? "Завершил" : "Обучается"}
                </span>
            </div>
            <div class="student-row-sub">${escapeHtml(student.courseTitle)} · запись ${escapeHtml(enrolled)}</div>
            <div class="teacher-student-progress" role="progressbar" aria-valuenow="${student.progressPercent}" aria-valuemin="0" aria-valuemax="100">
                <span style="width:${student.progressPercent}%"></span>
            </div>
            <div class="student-row-sub">${student.progressPercent}% прогресса</div>
        </div>`;
}

async function openTeacherStudentsModal() {
    const summary = document.getElementById("teacherStudentsSummary");
    const list = document.getElementById("teacherStudentsList");
    summary.innerHTML = `<span class="empty-note">Загружаем список учащихся…</span>`;
    list.innerHTML = "";
    document.getElementById("teacherStudentsModal").classList.add("open");
    try {
        const students = await loadTeacherStudents();
        const uniqueStudents = new Set(students.map(student => student.studentId)).size;
        summary.innerHTML = `<span class="empty-note">Записей на курсы: ${students.length} · уникальных учащихся: ${uniqueStudents}</span>`;
        list.innerHTML = students.length
            ? students.map(renderTeacherStudentRow).join("")
            : `<p class="empty-note">Пока никто не записался на ваши курсы.</p>`;
    } catch (error) {
        console.error("Не удалось загрузить список учащихся:", error);
        summary.innerHTML = `<span class="empty-note">${escapeHtml(error.message || "Не удалось загрузить список.")}</span>`;
    }
}

function closeTeacherStudentsModal() {
    document.getElementById("teacherStudentsModal").classList.remove("open");
}

function switchLeaderboardTab(tabName) {
    document.querySelectorAll('.tab-chip').forEach(chip => chip.classList.remove('active'));
    const activeChip = document.getElementById(`chip-${tabName}`);
    if (activeChip) activeChip.classList.add('active');
    renderLeaderboard(tabName);
}

async function loadLeaderboard(type) {
    const tableContainer = document.getElementById("statsLeaderboardTable");
    if (!tableContainer) return;
    const requestVersion = ++leaderboardRequestVersion;
    tableContainer.innerHTML = `<p class="empty-note">Загружаем рейтинг…</p>`;
    const endpoint = {
        global: "/api/ratings/GetTopTeachers",
        vuz: "/api/ratings/GetTopCommunities",
        weekly: "/api/ratings/GetTopCourses"
    }[type];
    try {
        const rows = await fetchApiRows(endpoint, "Не удалось загрузить рейтинг.");
        if (requestVersion !== leaderboardRequestVersion) return;
        teacherLeaderboardData[type] = rows.map((row, index) => {
            if (type === "global") {
                const userId = getField(row, "userId", "UserId") ?? "";
                const count = Number(getField(row, "studentsCount", "StudentsCount")) || 0;
                const score = Number(getField(row, "score", "Score")) || 0;
                const coursesCount = Number(getField(row, "publishedCoursesCount", "PublishedCoursesCount")) || 0;
                const averageRating = Number(getField(row, "averageRating", "AverageRating")) || 0;
                return {
                    place: index + 1,
                    name: getField(row, "userName", "UserName") ?? "Преподаватель",
                    avatarUrl: getField(row, "avatarUrl", "AvatarUrl"),
                    href: userId ? `Profile.html?id=${encodeURIComponent(userId)}` : "",
                    xp: `${coursesCount} курсов · ${count} уч. · ${averageRating ? `${averageRating.toFixed(1)} ★` : "без отзывов"} · индекс ${score.toFixed(1)}`,
                    isCurrentUser: userId === currentTeacherUserId
                };
            }
            if (type === "vuz") {
                return {
                    place: index + 1,
                    name: getField(row, "name", "Name") ?? "Сообщество",
                    xp: `Индекс силы ${Number(getField(row, "powerIndex", "PowerIndex")) || 0}`
                };
            }
            const popularity = Number(getField(row, "popIndex", "PopIndex")) || 0;
            return {
                place: index + 1,
                name: getField(row, "title", "Title") ?? "Курс",
                xp: `Индекс популярности ${popularity.toFixed(1)}`
            };
        });
        renderLeaderboardRows(type);
    } catch (error) {
        if (requestVersion !== leaderboardRequestVersion) return;
        console.error(`Не удалось загрузить рейтинг (${type}):`, error);
        tableContainer.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить рейтинг.")}</p>`;
    }
}

function renderLeaderboardRows(type) {
    const tableContainer = document.getElementById("statsLeaderboardTable");
    if (!tableContainer) return;
    tableContainer.innerHTML = "";

    const data = teacherLeaderboardData[type] || [];
    if (data.length === 0) {
        tableContainer.innerHTML = `<p class="empty-note">Для этого рейтинга пока нет данных.</p>`;
        return;
    }

    data.forEach((row, index) => {
        const item = document.createElement("div");
        item.className = `leader-row-item animate__animated animate__fadeInUp ${row.isCurrentUser ? 'current-user' : ''}`;
        item.style.animationDelay = `${index * 0.04}s`;
        const avatar = row.avatarUrl
            ? `<img class="teacher-leader-avatar" src="${escapeHtml(row.avatarUrl)}" alt="" onerror="this.remove()">`
            : "";
        const name = row.href
            ? `<a href="${escapeHtml(row.href)}">${escapeHtml(row.name)}</a>`
            : escapeHtml(row.name);
        item.innerHTML = `
            <span class="leader-place">${row.place}</span>
            ${avatar}
            <span class="leader-name">${name}</span>
            <span class="leader-xp">${escapeHtml(row.xp)}</span>`;
        tableContainer.appendChild(item);
    });
}

function renderLeaderboard(type) {
    loadLeaderboard(type);
}

function openFocusTree(treeId) {
    const overlay = document.getElementById(treeId);
    if (overlay) overlay.classList.add('open');
}

function closeFocusTree(treeId) {
    const overlay = document.getElementById(treeId);
    if (overlay) overlay.classList.remove('open');
}

document.addEventListener("DOMContentLoaded", async () => {
    try {
        const response = await apiFetch("/api/user/GetMyBusinessPassport");
        if (!response.ok) throw new Error(`Не удалось загрузить профиль (${response.status}).`);
        const passport = unwrapApiPayload(await response.json());
        const user = getField(passport, "user", "User") ?? {};
        const progress = getField(passport, "levelingProgress", "LevelingProgress") ?? {};
        currentTeacherUserId = getField(user, "externalId", "ExternalId") ?? "";
        const userName = getField(passport, "userName", "UserName");
        const usernameElement = document.getElementById("statsUsernameDisplay");
        if (usernameElement) usernameElement.textContent = userName ? `${userName} · Учитель` : "Учитель";

        const avatarUrl = getField(user, "avatarUrl", "AvatarUrl");
        const avatar = document.getElementById("statsTeacherAvatar");
        if (avatar) {
            const imageUrl = avatarUrl || "images/hamzaBro3.png";
            avatar.style.backgroundImage = `url("${String(imageUrl).replaceAll('"', '%22')}")`;
        }

        const rankKey = getField(progress, "rankKey", "RankKey");
        const level = Number(getField(progress, "level", "Level")) || 0;
        const rankElement = document.getElementById("statsTeacherRank");
        if (rankElement) {
            rankElement.textContent = level >= 100
                ? `${teacherRankTitles[rankKey] || "Преподаватель"} · максимальный уровень`
                : `${teacherRankTitles[rankKey] || "Преподаватель"} · уровень ${level}`;
        }

        const totalXp = Number(getField(user, "globalXp", "GlobalXp")) || 0;
        const currentXp = Number(getField(progress, "currentXpInLevel", "CurrentXpInLevel")) || 0;
        const nextXp = Number(getField(progress, "nextLevelXpRequired", "NextLevelXpRequired")) || 0;
        const xpElement = document.getElementById("statsTeacherXp");
        const levelXpElement = document.getElementById("statsTeacherXpLevel");
        const xpFill = document.getElementById("statsTeacherXpFill");
        if (xpElement) xpElement.textContent = `${totalXp.toLocaleString("ru-RU")} XP`;
        if (levelXpElement) {
            levelXpElement.textContent = level >= 100
                ? "Максимум"
                : nextXp > 0 ? `${currentXp} / ${nextXp} XP` : `${currentXp} XP`;
        }
        if (xpFill) xpFill.style.width = `${level >= 100 ? 100 : nextXp > 0 ? Math.min(100, currentXp / nextXp * 100) : 0}%`;
    } catch (error) {
        console.error("Не удалось загрузить паспорт преподавателя:", error);
        const rank = document.getElementById("statsTeacherRank");
        if (rank) rank.textContent = "Профиль недоступен";
    }

    try {
        await loadTeacherDashboardData();
    } catch (error) {
        console.error("Не удалось загрузить статистику преподавателя:", error);
        const container = document.getElementById("teacherCoursesList");
        if (container) container.innerHTML = `<p class="empty-note">${escapeHtml(error.message || "Не удалось загрузить статистику.")}</p>`;
        const leaderboard = document.getElementById("statsLeaderboardTable");
        if (leaderboard) leaderboard.innerHTML = `<p class="empty-note">Рейтинг не загружен.</p>`;
    }
});

window.switchLeaderboardTab = switchLeaderboardTab;
window.openTeacherStudentsModal = openTeacherStudentsModal;
window.openCourseAnalytics = openCourseAnalytics;
window.closeCourseAnalytics = closeCourseAnalytics;
window.closeTeacherStudentsModal = closeTeacherStudentsModal;
window.openFocusTree = openFocusTree;
window.closeFocusTree = closeFocusTree;