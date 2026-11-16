import { state } from "./core.js";

export function appendChatMessage(text, isBot) {
    const windowEl = document.getElementById("aiSupportChatWindow");
    if (!windowEl) return;
    const row = document.createElement("div");
    row.style.cssText = `display: flex; gap: 10px; max-width: 85%; margin-bottom: 8px; animation: fadeIn 0.2s ease; ${isBot ? 'align-self: flex-start; margin-right: auto;' : 'align-self: flex-end; margin-left: auto;'}`;

    const bubble = document.createElement("div");
    bubble.style.cssText = isBot
        ? "background: rgba(255,255,255,0.03); border: 1px solid var(--border); padding: 10px 14px; border-radius: 2px 12px 12px 12px; color: #94a3b8; font-size: 12.5px; line-height: 1.5; white-space: pre-wrap;"
        : "background: linear-gradient(135deg, #6c5ce7, #a855f7); padding: 10px 14px; border-radius: 12px 12px 2px 12px; color: white; font-size: 12.5px; line-height: 1.5; box-shadow: 0 4px 12px rgba(168,85,247,0.15); white-space: pre-wrap;";
    bubble.textContent = text;
    row.appendChild(bubble);

    windowEl.appendChild(row);
    windowEl.scrollTop = windowEl.scrollHeight;
}

export function loadAiSupportHistory() {
    const chatWindow = document.getElementById("aiSupportChatWindow");
    if (!chatWindow || state.aiSupportHistoryLoaded) return Promise.resolve();
    if (state.aiSupportHistoryLoadPromise) return state.aiSupportHistoryLoadPromise;

    state.aiSupportHistoryLoadPromise = (async () => {
        chatWindow.replaceChildren();
        try {
            const response = await window.apiFetch("/api/chat/GetAiSupportHistory");
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const messages = await response.json();
            messages.forEach(message => appendChatMessage(message.content, message.role === "assistant"));
            if (messages.length === 0) {
                appendChatMessage("Привет! Я ИИ-помощник EduGame. Напиши свой вопрос, и я постараюсь помочь.", true);
            }
            state.aiSupportHistoryLoaded = true;
        } catch (error) {
            console.error("[EduGame AI] Не удалось загрузить историю чата:", error);
            appendChatMessage("Не удалось загрузить историю чата. Попробуйте открыть раздел ещё раз.", true);
        } finally {
            state.aiSupportHistoryLoadPromise = null;
        }
    })();

    return state.aiSupportHistoryLoadPromise;
}

export async function handleSendMessageToAi() {
    const inputEl = document.getElementById("aiSupportInput");
    const chatWindow = document.getElementById("aiSupportChatWindow");
    if (!inputEl || !chatWindow) return;

    await loadAiSupportHistory();
    const text = inputEl.value.trim();
    if (!text) return;

    appendChatMessage(text, false);
    inputEl.value = "";

    appendChatMessage("ИИ формирует ответ...", true);

    try {
        const response = await window.apiFetch("/api/chat/AskAi", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ UserPrompt: text })
        });

        if (chatWindow.lastChild) chatWindow.removeChild(chatWindow.lastChild);

        if (!response.ok) {
            appendChatMessage("❌ Ошибка локальной ИИ-модели: " + response.status + ". Проверьте, что Ollama запущена и модель загружена.", true);
            return;
        }

        const result = await response.json();
        appendChatMessage(result.content, true);
    } catch (err) {
        if (chatWindow.lastChild) chatWindow.removeChild(chatWindow.lastChild);
        appendChatMessage("⚠️ Не удалось связаться с локальной ИИ-моделью. Проверьте запуск Ollama и API.", true);
    }
}