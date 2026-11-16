const teacherCourses = [];
const TEACHER_PATHS = [];
let teacherCollectionCourses = [];
let currentCourseId = null;
let currentPanel = "dashboard";
let currentLessonSelection = null;
let teacherCollection = "draft";
let teacherCollectionRequestVersion = 0;
let teacherCollectionLoading = false;
let teacherLikedCourseIds = new Set();
let teacherStudentCounts = new Map();
let teacherRatings = new Map();
let teacherProgress = new Map();

function getCourse(id) {
    return teacherCourses.find(c => c.id === id) ?? teacherCollectionCourses.find(c => c.id === id);
}

function currentCourse() {
    return getCourse(currentCourseId);
}

function escapeHtml(s) {
    return String(s ?? "").replace(/[&<>"']/g, c => ({ 
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' 
    }[c]));
}

function sanitizeLessonHtml(html) {
    const allowedTags = new Set(["P", "BR", "STRONG", "B", "EM", "I", "U", "UL", "OL", "LI", "H2", "H3", "BLOCKQUOTE", "PRE", "CODE", "A"]);
    const document = new DOMParser().parseFromString(String(html ?? ""), "text/html");
    const sanitizeNode = node => {
        if (node.nodeType === Node.TEXT_NODE) return document.createTextNode(node.textContent ?? "");
        if (node.nodeType !== Node.ELEMENT_NODE) return document.createDocumentFragment();
        const fragment = document.createDocumentFragment();
        const element = node;
        if (!allowedTags.has(element.tagName)) {
            [...element.childNodes].forEach(child => fragment.append(sanitizeNode(child)));
            return fragment;
        }
        const clean = document.createElement(element.tagName.toLowerCase());
        if (element.tagName === "A") {
            const href = element.getAttribute("href");
            try {
                const url = new URL(href, window.location.origin);
                if (["https:", "http:"].includes(url.protocol)) {
                    clean.setAttribute("href", url.href);
                    clean.setAttribute("target", "_blank");
                    clean.setAttribute("rel", "noopener noreferrer");
                }
            } catch {
            }
        }
        [...element.childNodes].forEach(child => clean.append(sanitizeNode(child)));
        fragment.append(clean);
        return fragment;
    };
    const result = document.createElement("div");
    [...document.body.childNodes].forEach(child => result.append(sanitizeNode(child)));
    return result.innerHTML;
}

function normalizeCourseSections(sections) {
    return (sections ?? []).map(section => ({
        id: section.id ?? section.Id ?? 0,
        title: section.title ?? section.Title ?? "",
        lessons: (section.lessons ?? section.Lessons ?? []).map(lesson => ({
            id: lesson.id ?? lesson.Id ?? 0,
            title: lesson.title ?? lesson.Title ?? "",
            order: lesson.order ?? lesson.Order ?? 0,
            xpReward: lesson.xpReward ?? lesson.XpReward ?? 0,
            blocks: (lesson.blocks ?? lesson.Blocks ?? []).map(block => ({
                id: block.id ?? block.Id ?? 0,
                type: block.type ?? block.Type ?? 0,
                content: block.content ?? block.Content ?? "",
                order: block.order ?? block.Order ?? 0
            }))
        }))
    }));
}

function unwrapApiResult(payload, message) {
    if (payload?.isSuccess === false || payload?.IsSuccess === false) {
        const rawErrors = payload.errors ?? payload.Errors;
        const errors = Array.isArray(rawErrors) ? rawErrors : [];
        const errorMessages = errors.map(error =>
            typeof error === "string" ? error : (error.errorMessage ?? error.ErrorMessage ?? error.identifier ?? error.Identifier)
        ).filter(Boolean);
        throw new Error(errorMessages.join("\n") || message);
    }
    return payload?.value ?? payload?.Value ?? payload;
}

function showToast(title, message) {
    const toast = document.getElementById("gameToast");
    if (!toast) return;
    document.getElementById("toastTitle").innerText = title;
    document.getElementById("toastMessage").innerText = message;
    toast.classList.add("show");
    setTimeout(() => toast.classList.remove("show"), 3500);
}