(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { escapeHtml, findLesson } = Learn.utils;

    const questTree = document.getElementById("questTree");
    const lessonBody = document.getElementById("lessonBody");
    const lessonTaskZone = document.getElementById("lessonTaskZone");

    function renderSidebar() {
        if (!questTree || !state.currentCourse) return;
        questTree.innerHTML = "";
        document.getElementById("sidebarCourseName").textContent = state.currentCourse.title;
        state.currentCourse.modules.forEach(m => {
            const group = document.createElement("div");
            group.className = "module-group";
            group.innerHTML = `<div class="module-group-title"><i class="fa-solid fa-layer-group"></i> ${escapeHtml(m.title)}</div>`;
            m.lessons.forEach(l => {
                const statusIcon = l.status === "completed" ? "fa-circle-check" : l.status === "active" ? "fa-circle-dot" : "fa-lock";
                const taskBlock = l.blocks?.find(b => b.kind === "quiz" || b.kind === "assignment");
                let reward = "";
                if (taskBlock) {
                    try {
                        const td = JSON.parse(taskBlock.content);
                        reward = `+${td.xp} XP / +${td.coins} 🪙`;
                    } catch {}
                }
                const node = document.createElement("div");
                node.className = `quest-node ${l.status}`;
                node.innerHTML = `
                    <div class="node-status-icon"><i class="fa-solid ${statusIcon}"></i></div>
                    <div class="node-meta">
                        <span class="node-title">${escapeHtml(l.title)}</span>
                        <span class="node-reward-tag ${taskBlock ? 'gold' : ''}">${l.status === 'completed' ? 'Пройдено' : (l.status === 'locked' ? 'Заблокировано' : reward)}</span>
                    </div>`;
                if (l.status !== "locked") node.addEventListener("click", () => {
                    state.currentLessonId = l.id;
                    state.selectedQuizAnswer = null;
                    renderLesson();
                    renderSidebar();
                });
                group.appendChild(node);
            });
            questTree.appendChild(group);
        });
    }

    function sanitizeLessonHtml(html) {
        const allowedTags = new Set(["P", "BR", "STRONG", "B", "EM", "I", "U", "UL", "OL", "LI", "H2", "H3", "BLOCKQUOTE", "PRE", "CODE", "A"]);
        const parsed = new DOMParser().parseFromString(String(html ?? ""), "text/html");
        const sanitizeNode = node => {
            if (node.nodeType === Node.TEXT_NODE) return parsed.createTextNode(node.textContent ?? "");
            if (node.nodeType !== Node.ELEMENT_NODE) return parsed.createDocumentFragment();

            const fragment = parsed.createDocumentFragment();
            if (!allowedTags.has(node.tagName)) {
                [...node.childNodes].forEach(child => fragment.append(sanitizeNode(child)));
                return fragment;
            }

            const clean = parsed.createElement(node.tagName.toLowerCase());
            if (node.tagName === "A") {
                try {
                    const url = new URL(node.getAttribute("href"), window.location.origin);
                    if (["http:", "https:"].includes(url.protocol)) {
                        clean.href = url.href;
                        clean.target = "_blank";
                        clean.rel = "noopener noreferrer";
                    }
                } catch {
                }
            }
            [...node.childNodes].forEach(child => clean.append(sanitizeNode(child)));
            fragment.append(clean);
            return fragment;
        };

        const result = parsed.createElement("div");
        [...parsed.body.childNodes].forEach(child => result.append(sanitizeNode(child)));
        return result.innerHTML;
    }

    function getSafeImageUrl(value) {
        try {
            const url = new URL(value);
            return url.protocol === "https:" ? url.href : null;
        } catch {
            return null;
        }
    }

    function getSafeVideoUrl(value) {
        try {
            const url = new URL(value);
            const hosts = ["youtube.com", "www.youtube.com", "m.youtube.com", "youtu.be", "youtube-nocookie.com", "www.youtube-nocookie.com", "vimeo.com", "player.vimeo.com"];
            if (url.protocol !== "https:" || !hosts.includes(url.hostname)) return null;
            return url.href;
        } catch {
            return null;
        }
    }

    function renderLesson() {
        const found = findLesson(state.currentCourse, state.currentLessonId);
        if (!found) return;
        const { module, lesson } = found;
        const completeButton = document.getElementById("btnCompleteQuest");
        if (completeButton) {
            completeButton.disabled = lesson.status === "completed" || lesson.status === "locked";
            completeButton.innerHTML = lesson.status === "completed"
                ? '<i class="fa-solid fa-circle-check"></i> Квест завершён'
                : '<i class="fa-solid fa-bolt"></i> Завершить квест и забрать награду';
        }
        document.getElementById("lessonModuleLabel").textContent = module.title.toUpperCase();
        document.getElementById("lessonMainTitle").textContent = lesson.title;

        const body = document.createDocumentFragment();
        lesson.blocks.forEach(b => {
            if (b.kind === "video") {
                const src = getSafeVideoUrl(b.content);
                if (!src) return;
                const video = document.createElement("iframe");
                video.className = "lesson-video";
                video.src = src;
                video.allowFullscreen = true;
                video.sandbox = "allow-scripts allow-same-origin allow-presentation";
                video.referrerPolicy = "strict-origin-when-cross-origin";
                body.append(video);
            } else if (b.kind === "image") {
                const src = getSafeImageUrl(b.content);
                if (!src) return;
                const image = document.createElement("img");
                image.className = "lesson-video";
                image.src = src;
                image.alt = "";
                image.style.objectFit = "contain";
                body.append(image);
            } else if (b.kind === "text") {
                const text = document.createElement("div");
                text.innerHTML = sanitizeLessonHtml(b.content);
                body.append(text);
            }
        });
        lessonBody.replaceChildren(body);
        renderTask(lesson);
    }

    function renderTask(lesson) {
        const taskBlock = lesson.blocks?.find(b => b.kind === "quiz" || b.kind === "assignment");
        if (!taskBlock) { lessonTaskZone.innerHTML = ""; return; }
        let t; try { t = JSON.parse(taskBlock.content); } catch { lessonTaskZone.innerHTML = ""; return; }

        if (taskBlock.kind === "quiz") {
            lessonTaskZone.innerHTML = `
                <div class="task-label"><i class="fa-solid fa-circle-question"></i> Задание · авто-проверка</div>
                <div class="task-question">${escapeHtml(t.question)}</div>
                <div class="task-options">
                    ${t.options.map((opt, i) => `<label class="task-option" data-i="${i}"><input type="radio" name="taskAnswer" value="${i}"> ${escapeHtml(opt)}</label>`).join("")}
                </div>
                <div class="task-feedback" id="taskFeedback">Выбери вариант и заверши квест для проверки.</div>`;

            lessonTaskZone.querySelectorAll(".task-option").forEach(opt => {
                opt.addEventListener("click", () => {
                    const chosen = parseInt(opt.getAttribute("data-i"));
                    state.selectedQuizAnswer = chosen;
                    lessonTaskZone.querySelectorAll(".task-option").forEach(o => o.classList.remove("selected"));
                    opt.classList.add("selected");
                });
            });
        } else {
            lessonTaskZone.innerHTML = `
                <div class="task-label"><i class="fa-solid fa-paper-plane"></i> Практическое задание</div>
                <div class="task-question">${escapeHtml(t.prompt)}</div>
                <div class="task-feedback">Самостоятельная практика. Отправка решения преподавателю пока не подключена.</div>`;
        }
    }

    Object.assign(Learn, { renderSidebar, renderLesson });
})(window.EduGameLearn);