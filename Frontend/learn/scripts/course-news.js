(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { escapeHtml } = Learn.utils;

    const courseNewsModal = document.getElementById("modalCourseNews");
    const courseNewsList = document.getElementById("courseNewsList");
    const btnCourseNews = document.getElementById("btnCourseNews");
    const btnCloseCourseNews = document.getElementById("btnCloseCourseNews");

    async function openCourseNews() {
        if (!state.currentCourse || !courseNewsModal || !courseNewsList) return;
        const newsCourseId = String(state.currentCourse.id);
        courseNewsModal.dataset.courseId = newsCourseId;
        document.getElementById("courseNewsCourseName").textContent = state.currentCourse.title;
        courseNewsList.innerHTML = '<p class="course-news-state"><i class="fa-solid fa-spinner fa-spin"></i> Загружаем новости…</p>';
        courseNewsModal.classList.add("open");

        try {
            const response = await apiFetch(`/api/courses/GetCourseNews?courseId=${encodeURIComponent(newsCourseId)}`);
            const payload = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(payload?.error || payload?.title || `Не удалось загрузить новости (${response.status}).`);
            }
            const news = payload?.value ?? payload?.Value ?? payload;
            if (!Array.isArray(news)) throw new Error("Сервер вернул некорректный список новостей.");
            if (courseNewsModal.dataset.courseId !== newsCourseId) return;
            if (!news.length) {
                courseNewsList.innerHTML = '<div class="course-news-empty"><i class="fa-regular fa-bell-slash"></i><strong>Новостей пока нет</strong><span>Здесь появятся обновления от создателя курса.</span></div>';
                return;
            }

            courseNewsList.replaceChildren(...news.map(item => {
                const author = item.author ?? item.Author ?? "Создатель курса";
                const text = item.text ?? item.Text ?? "";
                const createdAt = item.createdAt ?? item.CreatedAt;
                const article = document.createElement("article");
                article.className = "course-news-item";
                const header = document.createElement("div");
                header.className = "course-news-item-meta";
                const authorEl = document.createElement("strong");
                authorEl.textContent = author;
                const time = document.createElement("time");
                if (createdAt) {
                    const date = new Date(createdAt);
                    time.dateTime = date.toISOString();
                    time.textContent = new Intl.DateTimeFormat("ru-RU", {
                        dateStyle: "medium",
                        timeStyle: "short"
                    }).format(date);
                }
                const content = document.createElement("p");
                content.textContent = text;
                header.append(authorEl, time);
                article.append(header, content);
                return article;
            }));
        } catch (error) {
            console.error("[Learn] Не удалось загрузить новости курса:", error);
            if (courseNewsModal.dataset.courseId === newsCourseId) {
                courseNewsList.innerHTML = `<p class="course-news-error">${escapeHtml(error.message || "Не удалось загрузить новости курса.")}</p>`;
            }
        }
    }

    function initCourseNews() {
        btnCourseNews?.addEventListener("click", openCourseNews);
        btnCloseCourseNews?.addEventListener("click", () => courseNewsModal?.classList.remove("open"));
        courseNewsModal?.addEventListener("click", event => {
            if (event.target === courseNewsModal) courseNewsModal.classList.remove("open");
        });
    }

    Learn.initCourseNews = initCourseNews;
})(window.EduGameLearn);