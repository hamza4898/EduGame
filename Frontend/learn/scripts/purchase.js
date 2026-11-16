(function (Learn) {
    'use strict';
    const { API } = Learn;
    const { renderRail } = Learn;

    async function buyCourse(course) {
        const promoCode = prompt("Введите промокод или оставьте поле пустым:", "")?.trim() || null;
        if (!confirm(`Купить «${course.title}» за ${course.price} монет${promoCode ? ` с промокодом ${promoCode}` : ""}?`)) return;

        let purchaseResult;
        try {
            const r = await apiFetch(API.buyCourse, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ courseId: course.id, promoCode })
            });
            const payload = await r.json().catch(() => null);
            if (!r.ok) {
                throw new Error(payload?.error || payload?.title || payload?.message || "Покупка не выполнена.");
            }
            purchaseResult = payload?.value ?? payload?.Value ?? payload;
        } catch (error) {
            console.error("[Learn] Не удалось купить курс:", error);
            alert(error.message || "Не удалось связаться с сервером.");
            return;
        }

        course.owned = true;
        const coinsEl = document.getElementById("hudUserCoinsDisplay");
        const newBalance = purchaseResult?.coinsBalance ?? purchaseResult?.CoinsBalance;
        if (coinsEl && Number.isFinite(Number(newBalance))) {
            coinsEl.textContent = Number(newBalance).toLocaleString("ru-RU");
        }
        renderRail();

        try {
            await Learn.startCourse(course);
        } catch (error) {
            console.error("[Learn] Покупка прошла, но курс не загрузился:", error);
            alert(`Курс куплен, монеты списаны. Не удалось открыть курс: ${error.message || "ошибка загрузки"}. Обновите страницу, чтобы продолжить.`);
        }
    }

    Learn.buyCourse = buyCourse;
})(window.EduGameLearn);