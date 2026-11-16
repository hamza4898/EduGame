(function (Learn) {
    'use strict';
    const { state, API } = Learn;
    const { lessonView, lessonLayout, courseCompletionView } = Learn.el;
    const { isCourseCompleted } = Learn.utils;
    const { loadCourseDetails, loadMyCourses } = Learn;
    const { renderRail } = Learn;
    const { renderSidebar, renderLesson } = Learn;
    const { showCourseCompletion } = Learn;
    const { showView } = Learn;

    async function startCourse(course) {
        state.currentCourse = course;
        if (!course.owned) {
            const response = await apiFetch(API.startCourse, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ courseId: course.id })
            });
            const result = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(result?.error || result?.title || "Не удалось записаться на курс.");
            }
            course.owned = true;
        }

        const detailed = await loadCourseDetails(course.id);
        if (!detailed.modules.some(module => module.lessons.length > 0)) {
            throw new Error("В этом курсе пока нет уроков. Попросите преподавателя проверить и сохранить его содержание.");
        }
        Object.assign(course, detailed);
        await loadMyCourses();
        const index = state.ALL_COURSES.findIndex(item => item.id === course.id);
        if (index >= 0) state.ALL_COURSES[index] = course;
        else state.ALL_COURSES.unshift(course);
        renderRail();

        const flat = allLessons(course);
        if (isCourseCompleted(course)) {
            state.currentLessonId = flat[flat.length - 1].id;
            state.selectedQuizAnswer = null;
            renderSidebar();
            showCourseCompletion(course);
            showView(lessonView);
            return;
        }
        const firstActive = flat.find(l => l.status === "active") || flat.find(l => l.status !== "completed") || flat[0];
        state.currentLessonId = firstActive?.id;
        state.selectedQuizAnswer = null;
        courseCompletionView.classList.add("hidden");
        lessonLayout.classList.remove("hidden");
        renderSidebar(); renderLesson(); showView(lessonView);
    }

    Learn.startCourse = startCourse;
})(window.EduGameLearn);