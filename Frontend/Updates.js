document.addEventListener("DOMContentLoaded", async () => {
    const feed = document.getElementById("developerUpdatesFeed");
    const editor = document.getElementById("developerUpdateEditor");
    const form = document.getElementById("developerUpdateForm");
    const status = document.getElementById("developerUpdateStatus");
    if (!feed || !editor || !form) return;

    function unwrap(value) {
        return value?.value ?? value?.Value ?? value;
    }

    function renderUpdate(update) {
        const card = document.createElement("article");
        card.className = "patch-card";

        const category = document.createElement("span");
        category.className = "patch-tag";
        category.textContent = update.category ?? update.Category ?? "Обновление";

        const title = document.createElement("h2");
        title.className = "patch-title";
        title.textContent = update.title ?? update.Title ?? "";

        const date = document.createElement("span");
        date.className = "patch-date";
        const dateIcon = document.createElement("i");
        dateIcon.className = "fa-regular fa-calendar";
        date.append(dateIcon, document.createTextNode(` ${new Date(update.createdAt ?? update.CreatedAt).toLocaleString("ru-RU")}`));

        const content = document.createElement("p");
        content.className = "patch-content";
        content.textContent = update.content ?? update.Content ?? "";

        const signature = document.createElement("div");
        signature.className = "patch-signature";
        const authorIcon = document.createElement("i");
        authorIcon.className = "fa-solid fa-user-gear";
        authorIcon.setAttribute("aria-hidden", "true");
        const author = document.createElement("span");
        author.textContent = update.authorSignature ?? update.AuthorSignature ?? "Хамза — Главный разработчик EduGame";
        signature.append(authorIcon, author);

        card.append(category, title, date, content, signature);
        return card;
    }

    async function loadUpdates() {
        feed.replaceChildren();
        const response = await window.apiFetch("/api/developer/GetUpdates");
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            throw new Error(payload?.error || `Не удалось загрузить журнал (${response.status}).`);
        }

        const updates = unwrap(payload);
        if (!Array.isArray(updates) || updates.length === 0) {
            const empty = document.createElement("p");
            empty.className = "updates-loading";
            empty.textContent = "Публикаций пока нет.";
            feed.append(empty);
            return;
        }

        updates.forEach(update => feed.append(renderUpdate(update)));
    }

    try {
        const refreshResponse = await window.refreshEduGameSession();
        if (!refreshResponse.ok) {
            throw new Error(`Не удалось обновить сессию для проверки роли (${refreshResponse.status}).`);
        }
        await loadUpdates();
        const accessResponse = await window.apiFetch("/api/developer/GetAdminAccess");
        if (!accessResponse.ok) {
            throw new Error(`Не удалось проверить права публикации (${accessResponse.status}).`);
        }
        const accessPayload = await accessResponse.json();
        const isAdmin = typeof accessPayload === "boolean"
            ? accessPayload
            : accessPayload?.value ?? accessPayload?.Value;
        if (typeof isAdmin !== "boolean") {
            throw new Error("Сервер вернул некорректный результат проверки роли admin.");
        }
        if (isAdmin) editor.style.display = "block";
    } catch (error) {
        console.error("[Updates] Не удалось загрузить журнал:", error);
        feed.replaceChildren();
        const failure = document.createElement("p");
        failure.className = "updates-loading";
        failure.textContent = error instanceof Error ? error.message : "Не удалось загрузить журнал обновлений.";
        feed.append(failure);
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();
        const submitButton = form.querySelector("button[type='submit']");
        const request = {
            title: document.getElementById("updateTitle").value.trim(),
            category: document.getElementById("updateCategory").value.trim(),
            content: document.getElementById("updateContent").value.trim()
        };
        if (submitButton) submitButton.disabled = true;
        if (status) status.textContent = "Публикуем статью...";

        try {
            const response = await window.apiFetch("/api/developer/CreateUpdate", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(request)
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(payload?.error || `Не удалось опубликовать статью (${response.status}).`);
            }

            form.reset();
            document.getElementById("updateCategory").value = "Обновление";
            if (status) status.textContent = "Статья опубликована.";
            await loadUpdates();
        } catch (error) {
            console.error("[Updates] Не удалось опубликовать статью:", error);
            if (status) status.textContent = error instanceof Error ? error.message : "Не удалось опубликовать статью.";
        } finally {
            if (submitButton) submitButton.disabled = false;
        }
    });
});