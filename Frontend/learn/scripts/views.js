(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { dashboardView, lessonView, catalogView } = Learn.el;

    function showView(view) {
        [dashboardView, lessonView, catalogView].forEach(v => {
            if (!v) return;
            v.style.display = "none";
            v.classList.add("hidden");
            v.classList.remove("animate__animated", "animate__fadeIn");
        });
        view.style.display = (view === dashboardView) ? "flex" : "block";
        view.classList.remove("hidden");
        void view.offsetWidth;
        view.classList.add("animate__animated", "animate__fadeIn");
        window.scrollTo(0, 0);
    }

    function initViews() {
        const btnResume = document.getElementById("btnResumeActiveRaid");
        const btnLeave = document.getElementById("btnLeaveLesson");
        const btnCompletionDashboard = document.getElementById("btnCompletionDashboard");
        const btnReviewLessons = document.getElementById("btnReviewLessons");
        if (btnResume) btnResume.addEventListener("click", () => {
            if (state.currentCourse) Learn.startCourse(state.currentCourse).catch(error => alert(error.message || "Не удалось открыть курс."));
        });
        if (btnLeave) btnLeave.addEventListener("click", () => showView(dashboardView));
        if (btnCompletionDashboard) btnCompletionDashboard.addEventListener("click", () => showView(dashboardView));
        if (btnReviewLessons) btnReviewLessons.addEventListener("click", Learn.showCourseLessons);
    }

    Object.assign(Learn, { showView, initViews });
})(window.EduGameLearn);