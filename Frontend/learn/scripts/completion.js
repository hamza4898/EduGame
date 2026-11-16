(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { lessonLayout, courseCompletionView } = Learn.el;
    const { loadCourseReviews, normalizeReview } = Learn;
    const { renderSidebar, renderLesson } = Learn;

    function renderCourseCompletion(course) {
        const lessons = allLessons(course);
        const completedModules = course.modules.filter(module =>
            module.lessons.length > 0 && module.lessons.every(lesson => lesson.status === "completed")
        ).length;
        document.getElementById("completionCourseTitle").textContent = `${course.title} завершён!`;
        document.getElementById("completionStats").innerHTML = `
            <div class="completion-stat">
                <span class="completion-stat-icon violet"><i class="fa-solid fa-layer-group"></i></span>
                <strong>${completedModules} / ${course.modules.length}</strong>
                <span>модулей пройдено</span>
            </div>
            <div class="completion-stat">
                <span class="completion-stat-icon green"><i class="fa-solid fa-book-open"></i></span>
                <strong>${lessons.length} / ${lessons.length}</strong>
                <span>уроков завершено</span>
            </div>
            <div class="completion-stat">
                <span class="completion-stat-icon gold"><i class="fa-solid fa-chart-line"></i></span>
                <strong>100%</strong>
                <span>прогресс курса</span>
            </div>`;
    }

    function setReviewButtonSaved(isSaved) {
        const button = document.getElementById("btnSubmitCourseReview");
        if (!button) return;
        button.innerHTML = isSaved
            ? '<i class="fa-solid fa-floppy-disk"></i> Обновить отзыв'
            : '<i class="fa-solid fa-paper-plane"></i> Отправить отзыв';
    }

    function showCourseCompletion(course) {
        renderCourseCompletion(course);
        lessonLayout.classList.add("hidden");
        courseCompletionView.classList.remove("hidden");
        const status = document.getElementById("courseReviewStatus");
        if (status) {
            status.textContent = "";
            status.classList.remove("error");
        }
        document.getElementById("courseReviewStars").value = "5";
        document.getElementById("courseReviewText").value = "";
        setReviewButtonSaved(false);
        loadCourseReviews(course.id).then(reviews => {
            if (state.currentCourse?.id !== course.id || courseCompletionView.classList.contains("hidden")) return;
            const ownReview = reviews.find(review => review.studentId === state.currentUserId);
            if (!ownReview) return;
            document.getElementById("courseReviewStars").value = String(ownReview.stars);
            document.getElementById("courseReviewText").value = ownReview.text;
            setReviewButtonSaved(true);
        }).catch(error => {
            console.error("[Learn] Не удалось загрузить сохраненный отзыв:", error);
            if (status) {
                status.textContent = "Не удалось загрузить ваш отзыв. Можно отправить его ещё раз.";
                status.classList.add("error");
            }
        });
        document.querySelector(".learn-main-wrapper").scrollTop = 0;
    }

    function showCourseLessons() {
        courseCompletionView.classList.add("hidden");
        lessonLayout.classList.remove("hidden");
        renderSidebar();
        renderLesson();
    }

    function initCompletion() {
        const btnSubmitReview = document.getElementById("btnSubmitCourseReview");
        const btnDownloadCertificate = document.getElementById("btnDownloadCertificate");
        if (btnSubmitReview) {
            btnSubmitReview.addEventListener("click", async () => {
                if (!state.currentCourse) return;
                const status = document.getElementById("courseReviewStatus");
                btnSubmitReview.disabled = true;
                if (status) {
                    status.textContent = "Сохраняем отзыв…";
                    status.classList.remove("error");
                }
                try {
                    const response = await apiFetch(`/api/courses/SaveCourseReview?courseId=${encodeURIComponent(state.currentCourse.id)}`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                            stars: Number(document.getElementById("courseReviewStars").value),
                            text: document.getElementById("courseReviewText").value.trim()
                        })
                    });
                    const payload = await response.json().catch(() => null);
                    if (!response.ok) throw new Error(payload?.error || payload?.title || "Не удалось сохранить отзыв.");
                    const savedReview = normalizeReview(payload?.value ?? payload?.Value ?? payload);
                    state.currentCourse.reviews = [
                        ...(state.currentCourse.reviews || []).filter(review =>
                            (review.studentId ?? review.StudentId) !== savedReview.studentId),
                        savedReview
                    ];
                    document.getElementById("courseReviewText").value = savedReview.text;
                    setReviewButtonSaved(true);
                    if (status) status.textContent = "Отзыв сохранён. Спасибо, что поделились впечатлениями!";
                } catch (error) {
                    console.error("[Learn] Не удалось сохранить отзыв:", error);
                    if (status) {
                        status.textContent = error.message || "Не удалось сохранить отзыв.";
                        status.classList.add("error");
                    }
                } finally {
                    btnSubmitReview.disabled = false;
                }
            });
        }
        if (btnDownloadCertificate) {
            btnDownloadCertificate.addEventListener("click", async () => {
                if (!state.currentCourse) return;
                try {
                    const response = await apiFetch(`/api/courses/GetCourseCertificate?courseId=${encodeURIComponent(state.currentCourse.id)}`);
                    if (!response.ok) {
                        const payload = await response.json().catch(() => null);
                        throw new Error(payload?.error || payload?.title || "Сертификат недоступен.");
                    }
                    const url = URL.createObjectURL(await response.blob());
                    const link = document.createElement("a");
                    link.href = url;
                    link.download = `EduGame-Certificate-${state.currentCourse.id}.pdf`;
                    link.click();
                    URL.revokeObjectURL(url);
                } catch (error) {
                    alert(error.message || "Не удалось загрузить сертификат.");
                }
            });
        }
    }

    Object.assign(Learn, { showCourseCompletion, showCourseLessons, initCompletion });
})(window.EduGameLearn);