(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { escapeHtml, findLesson } = Learn.utils;

    function updateHero() {
        const t = document.getElementById("heroCourseTitle");
        const h = document.getElementById("heroCourseHint");
        const bar = document.getElementById("heroProgressBar");
        const txt = document.getElementById("heroProgressText");

        const btnResume = document.getElementById("btnResumeActiveRaid");

        if (!state.currentCourse) {
            if (t) t.textContent = "У тебя пока нет активных рейдов";
            if (h) h.innerHTML = `Открой каталог снизу и начни свой первый квест по знаниям.`;
            if (bar) bar.style.width = "0%";

            if (txt) txt.style.display = "none";
            if (btnResume) btnResume.style.display = "none";
            return;
        }

        if (t) t.textContent = state.currentCourse.title;
        const found = findLesson(state.currentCourse, state.currentLessonId);
        if (h && found) h.innerHTML = `Ты остановился на этапе: <strong>${escapeHtml(found.module.title)} · ${escapeHtml(found.lesson.title)}</strong>. Пройди и забери награду.`;
        if (bar) bar.style.width = (state.currentCourse.progress || 0) + "%";

        if (txt) {
            txt.style.display = "block";
            txt.textContent = `${state.currentCourse.progress || 0}% рейда пройдено`;
        }
        if (btnResume) btnResume.style.display = "inline-flex";
    }

    Learn.updateHero = updateHero;
})(window.EduGameLearn);