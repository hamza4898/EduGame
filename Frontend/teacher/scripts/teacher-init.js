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