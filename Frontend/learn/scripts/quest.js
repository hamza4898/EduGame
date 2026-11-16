(function (Learn) {
    'use strict';
    const { state, API } = Learn;
    const { findLesson, isCourseCompleted } = Learn.utils;
    const { updateHero } = Learn;
    const { renderRail } = Learn;
    const { showCourseCompletion } = Learn;
    const { renderSidebar, renderLesson } = Learn;

    function getNextLesson(course, lessonId) {
        const flat = allLessons(course);
        const idx = flat.findIndex(l => String(l.id) === String(lessonId));
        return (idx >= 0 && idx < flat.length - 1) ? flat[idx + 1] : null;
    }

    function initQuest() {
        const btnComplete = document.getElementById("btnCompleteQuest");
        const gameToast = document.getElementById("gameToast");
        if (btnComplete) {
            btnComplete.addEventListener("click", async () => {
                const found = findLesson(state.currentCourse, state.currentLessonId);
                if (!found) return;
                const taskBlock = found.lesson.blocks?.find(b => b.kind === "quiz" || b.kind === "assignment");
                if (taskBlock?.kind === "quiz" && state.selectedQuizAnswer === null) {
                    alert("Сначала выберите ответ на квиз.");
                    return;
                }
                try {
                    const r = await apiFetch(API.completeLesson, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ lessonId: parseInt(found.lesson.id), selectedAnswer: state.selectedQuizAnswer })
                    });
                    const rewardData = await r.json().catch(() => null);
                    if (!r.ok) {
                        const message = rewardData?.error || rewardData?.title || rewardData?.errors?.[0] || "Не удалось завершить урок.";
                        const feedback = document.getElementById("taskFeedback");
                        if (feedback) {
                            feedback.className = "task-feedback err";
                            feedback.textContent = message;
                        } else alert(message);
                        return;
                    }
                    const next = getNextLesson(state.currentCourse, state.currentLessonId);
                    found.lesson.status = "completed";
                    if (next) { next.status = "active"; state.currentLessonId = next.id; }
                    state.currentCourse.progress = Number(rewardData?.newProgressPercent ?? rewardData?.NewProgressPercent ?? 0);
                    state.selectedQuizAnswer = null;
                    const coinsEl = document.getElementById("hudUserCoinsDisplay");
                    const coinsGained = Number(rewardData?.coinsGained ?? rewardData?.CoinsGained ?? 0);
                    const xpGained = Number(rewardData?.xpGained ?? rewardData?.XpGained ?? 0);
                    if (coinsEl) coinsEl.textContent = (parseInt(coinsEl.textContent) || 0) + coinsGained;
                    if (gameToast) {
                        document.getElementById("toastTitle").textContent = "Квест пройден!";
                        document.getElementById("toastMessage").textContent = `+${xpGained} XP и +${coinsGained} монет`;
                        gameToast.classList.add("show");
                        setTimeout(() => gameToast.classList.remove("show"), 3500);
                    }
                    updateHero();
                    renderRail();
                    if (isCourseCompleted(state.currentCourse)) {
                        state.currentCourse.progress = 100;
                        showCourseCompletion(state.currentCourse);
                    } else {
                        renderLesson();
                        renderSidebar();
                    }
                } catch (error) {
                    console.warn("[Learn] Ошибка отметки урока на бэке:", error);
                    alert("Не удалось связаться с сервером. Прогресс не изменён.");
                }
            });
        }
    }

    Learn.initQuest = initQuest;
})(window.EduGameLearn);