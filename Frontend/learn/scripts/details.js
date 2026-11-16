(function (Learn) {
    'use strict';
    const { escapeHtml, pathName, pathColor, totalLessons, lessonWord, hexA, starsHtml } = Learn.utils;
    const { loadCourseDetails, loadCourseReviews } = Learn;
    const { startCourse } = Learn;
    const { buyCourse } = Learn;

    const detailsModal = document.getElementById("modalCourseDetails");

    function renderReviewsWall(container, reviews) {
        container.innerHTML = reviews.length
            ? reviews.map(review => {
                const profileLink = review.studentId
                    ? `Profile.html?id=${encodeURIComponent(review.studentId)}`
                    : null;
                const avatar = `<img src="${escapeHtml(review.avatarUrl || "images/hamzaBro3.png")}" alt="" loading="lazy">`;
                const profileAvatar = profileLink
                    ? `<a class="review-user-avatar" href="${escapeHtml(profileLink)}" aria-label="Открыть профиль ${escapeHtml(review.student)}">${avatar}</a>`
                    : `<span class="review-user-avatar">${avatar}</span>`;
                const studentName = profileLink
                    ? `<a href="${escapeHtml(profileLink)}">${escapeHtml(review.student)}</a>`
                    : escapeHtml(review.student);
                const reviewDate = review.createdAt
                    ? new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium" }).format(new Date(review.createdAt))
                    : "";
                return `<article class="review-row">
                    ${profileAvatar}
                    <div class="review-bubble">
                        <div class="review-meta"><strong>${studentName}</strong><span class="review-stars" aria-label="${review.stars} из 5">${starsHtml(review.stars)}</span></div>
                        <p>${escapeHtml(review.text)}</p>
                        ${reviewDate ? `<time class="review-date">${escapeHtml(reviewDate)}</time>` : ""}
                    </div>
                </article>`;
            }).join("")
            : `<p class="reviews-empty">Отзывов пока нет — стань первым!</p>`;
    }

    async function openCourseDetails(course) {
        if (!detailsModal) return;
        const color = pathColor(course.pathId);

        const cover = detailsModal.querySelector(".details-cover");
        if (cover) {
            cover.style.background = `linear-gradient(135deg, ${hexA(color, 0.45)}, ${hexA(color, 0.12)})`;
            cover.querySelector(".category-badge").textContent = pathName(course.pathId);
            cover.querySelector("h2").textContent = course.title;
            const stats = document.getElementById("detailsCoverStats");
            if (stats) stats.innerHTML = `
                <span class="stat-stars"><i class="fa-solid fa-star"></i> ${course.rating.toFixed(1)} рейтинг</span>
                <span><i class="fa-solid fa-users"></i> ${course.studentsCount.toLocaleString('ru-RU')}</span>
                <span class="stat-lessons"><i class="fa-solid fa-layer-group"></i> ${totalLessons(course)} уроков</span>
                <span><i class="fa-solid fa-signal"></i> ${course.progress || 0}% пройдено</span>`;
        }

        const descBox = document.getElementById("detailsCourseDesc");
        const moreBtn = document.getElementById("detailsMoreBtn");
        if (descBox) {
            const short = course.description || "";
            const full = course.longDescription || short;
            descBox.textContent = short;
            descBox.classList.add("details-desc-clamped");
            descBox.classList.remove("expanded");
            if (moreBtn) {
                moreBtn.style.display = (full.length > short.length) ? "" : "none";
                moreBtn.textContent = "Подробнее";
                moreBtn.onclick = () => {
                    const expanded = descBox.classList.toggle("expanded");
                    descBox.textContent = expanded ? full : short;
                    moreBtn.textContent = expanded ? "Свернуть" : "Подробнее";
                };
            }
        }

        const modulesBox = document.getElementById("detailsCourseModules");
        if (modulesBox) {
            modulesBox.innerHTML = `<p class="course-structure-loading">Загружаем структуру курса…</p>`;
        }

        const avatarBox = document.getElementById("detailsAuthorAvatar");
        if (avatarBox) { avatarBox.className = `details-author-avatar ${course.authorType}`; avatarBox.innerHTML = `<i class="fa-solid ${course.authorIcon}"></i>`; }
        const authorName = document.getElementById("detailsAuthorName");
        if (authorName) authorName.textContent = course.authorName;

        const reviewsBox = document.getElementById("detailsReviewsWall");
        if (reviewsBox) {
            reviewsBox.innerHTML = '<p class="reviews-loading">Загружаем отзывы…</p>';
        }

        const footer = document.getElementById("detailsFooter");
        if (footer) {
            const priceVal = course.isFree
                ? `<span class="price-value free">Бесплатно</span>`
                : `<span class="price-value"><i class="fa-solid fa-coins"></i> ${course.price}</span>`;
            const alreadyOwned = course.owned || course.progress > 0;
            footer.innerHTML = `
                <div class="details-footer-price"><span class="price-label">Стоимость доступа</span>${priceVal}</div>
                ${course.isFree || alreadyOwned
                    ? `<button class="hud-btn-premium" id="btnDetailsStartRaid"><i class="fa-solid fa-rocket"></i> ${course.progress > 0 ? 'Продолжить рейд' : 'Войти в рейд'}</button>`
                    : `<button class="hud-btn-premium" id="btnDetailsStartRaid"><i class="fa-solid fa-wallet"></i> Купить за ${course.price} монет</button>`}`;
            document.getElementById("btnDetailsStartRaid").addEventListener("click", async () => {
                detailsModal.classList.remove("open");
                if (course.isFree || alreadyOwned)
                    startCourse(course).catch(error => alert(error.message || "Не удалось открыть курс."));
                else buyCourse(course);
            });
        }

        detailsModal.dataset.courseId = String(course.id);
        detailsModal.classList.add("open");
        if (modulesBox) {
            loadCourseDetails(course.id).then(details => {
                if (detailsModal.dataset.courseId !== String(course.id)) return;
                course.modules = details.modules;
                modulesBox.innerHTML = course.modules.length
                    ? course.modules.map(module =>
                        `<div class="details-module-row">
                            <i class="fa-solid fa-layer-group m-ico"></i>
                            <strong>${escapeHtml(module.title)}</strong>
                            <span>${module.lessons.length} ${lessonWord(module.lessons.length)}</span>
                            <ul class="details-module-lessons">${module.lessons.map(lesson =>
                                `<li><i class="fa-regular fa-circle-play"></i> ${escapeHtml(lesson.title)}</li>`
                            ).join("")}</ul>
                        </div>`
                    ).join("")
                    : `<p class="course-structure-empty">В этом курсе пока нет модулей.</p>`;
                const lessonCount = course.modules.reduce((total, module) => total + module.lessons.length, 0);
                const lessonStat = document.getElementById("detailsCoverStats")?.querySelector(".stat-lessons");
                if (lessonStat) lessonStat.innerHTML = `<i class="fa-solid fa-layer-group"></i> ${lessonCount} уроков`;
            }).catch(error => {
                console.error("[Learn] Не удалось загрузить структуру курса:", error);
                if (detailsModal.dataset.courseId === String(course.id)) {
                    modulesBox.innerHTML = `<p class="course-structure-error">${escapeHtml(error.message || "Не удалось загрузить структуру курса.")}</p>`;
                }
            });
        }
        loadCourseReviews(course.id).then(reviews => {
            if (detailsModal.dataset.courseId !== String(course.id) || !reviewsBox) return;
            course.reviews = reviews;
            renderReviewsWall(reviewsBox, reviews);
            if (reviews.length) {
                course.rating = reviews.reduce((sum, review) => sum + review.stars, 0) / reviews.length;
                const ratingLabel = document.getElementById("detailsCoverStats")?.querySelector(".stat-stars");
                if (ratingLabel) ratingLabel.innerHTML = `<i class="fa-solid fa-star"></i> ${course.rating.toFixed(1)} рейтинг`;
            }
        }).catch(error => {
            console.error("[Learn] Не удалось загрузить отзывы курса:", error);
            if (detailsModal.dataset.courseId === String(course.id) && reviewsBox) {
                reviewsBox.innerHTML = `<p class="reviews-error">${escapeHtml(error.message || "Не удалось загрузить отзывы.")}</p>`;
            }
        });
    }

    function initDetails() {
        const btnCloseDetails = document.getElementById("btnCloseDetailsModal");
        if (btnCloseDetails) btnCloseDetails.addEventListener("click", () => detailsModal.classList.remove("open"));
        if (detailsModal) detailsModal.addEventListener("click", (e) => { if (e.target === detailsModal) detailsModal.classList.remove("open"); });
    }

    Object.assign(Learn, { openCourseDetails, initDetails });
})(window.EduGameLearn);