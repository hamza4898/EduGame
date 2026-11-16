document.addEventListener("DOMContentLoaded", async () => {
    const Learn = window.EduGameLearn;
    const { state } = Learn;

    await Learn.loadPassport();

    await Learn.loadMyCourses();
    state.currentCourse = state.ALL_COURSES[0] || null;
    if (state.currentCourse) {
        const flat = allLessons(state.currentCourse);
        const firstActive = flat.find(l => l.status === "active") || flat.find(l => l.status !== "completed") || flat[0];
        state.currentLessonId = firstActive?.id;
    }

    Learn.initViews();
    Learn.initRail();
    Learn.initCourseChat();
    Learn.initCourseNews();
    Learn.initCompletion();
    Learn.initQuest();
    Learn.initDetails();
    Learn.initCatalog();

    Learn.updateHero();
    Learn.renderRail();
});