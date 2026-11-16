(function (Learn) {
    'use strict';
    const { PATHS } = Learn;

    function priceHtml(course) {
        return course.isFree
            ? `<span class="course-price free">Бесплатно</span>`
            : `<span class="course-price gold"><i class="fa-solid fa-coins"></i> ${course.price}</span>`;
    }
    function pathName(pathId) { const p = PATHS.find(x => x.id === pathId); return p ? p.name : "Курс"; }
    function pathColor(pathId) { const p = PATHS.find(x => x.id === pathId); return p ? p.color : "#a855f7"; }
    function totalLessons(course) { return allLessons(course).length; }
    function findLesson(course, lessonId) {
        for (const m of (course?.modules || [])) {
            const l = (m.lessons || []).find(x => String(x.id) === String(lessonId));
            if (l) return { module: m, lesson: l };
        }
        return null;
    }
    function isCourseCompleted(course) {
        const lessons = allLessons(course);
        return lessons.length > 0 && lessons.every(lesson => lesson.status === "completed");
    }
    function escapeHtml(s) {
        return String(s ?? "").replace(/[&<>"']/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c]));
    }
    function starsHtml(rating) {
        const full = Math.round(rating);
        return "★".repeat(full) + "☆".repeat(5 - full);
    }
    function lessonWord(count) {
        const lastTwo = count % 100;
        const last = count % 10;
        if (lastTwo >= 11 && lastTwo <= 14) return "уроков";
        if (last === 1) return "урок";
        if (last >= 2 && last <= 4) return "урока";
        return "уроков";
    }
    function hexA(hex, a) { const n = parseInt(hex.slice(1), 16); return `rgba(${(n>>16)&255},${(n>>8)&255},${n&255},${a})`; }

    Learn.utils = { priceHtml, pathName, pathColor, totalLessons, findLesson, isCourseCompleted, escapeHtml, starsHtml, lessonWord, hexA };
})(window.EduGameLearn);