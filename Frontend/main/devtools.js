export async function initializeDeveloperTools() {
    const form = document.getElementById("developerNotificationForm");
    if (!form) return;

    try {
        const refreshResponse = await window.refreshEduGameSession();
        if (!refreshResponse.ok) {
            throw new Error(`Не удалось обновить сессию для проверки роли (${refreshResponse.status}).`);
        }
        const accessResponse = await window.apiFetch("/api/developer/GetAdminAccess");
        if (!accessResponse.ok) {
            throw new Error(`Не удалось проверить роль разработчика (${accessResponse.status}).`);
        }
        const accessPayload = await accessResponse.json();
        const isAdmin = typeof accessPayload === "boolean"
            ? accessPayload
            : accessPayload?.value ?? accessPayload?.Value;
        if (typeof isAdmin !== "boolean") {
            throw new Error("Сервер вернул некорректный результат проверки роли admin.");
        }
        if (!isAdmin) return;

        form.style.display = "flex";
        form.addEventListener("submit", async event => {
            event.preventDefault();
            const input = document.getElementById("developerNotificationMessage");
            const status = document.getElementById("developerNotificationStatus");
            const button = form.querySelector("button[type='submit']");
            const message = input?.value.trim();
            if (!message) return;

            if (button) button.disabled = true;
            if (status) status.textContent = "Отправляем уведомление...";
            try {
                const response = await window.apiFetch("/api/developer/BroadcastNotification", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ message })
                });
                const body = await response.json().catch(() => null);
                if (!response.ok) {
                    throw new Error(body?.error || `Ошибка отправки (${response.status}).`);
                }

                if (input) input.value = "";
                if (status) status.textContent = `Уведомление сохранено и отправлено всем пользователям (${body} получателей).`;
            } catch (error) {
                console.error("[Developer broadcast] Не удалось отправить уведомление:", error);
                if (status) status.textContent = error instanceof Error ? error.message : "Не удалось отправить уведомление.";
            } finally {
                if (button) button.disabled = false;
            }
        });
    } catch (error) {
        console.error("[Developer tools] Не удалось проверить права администратора:", error);
    }
}