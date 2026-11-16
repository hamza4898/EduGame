const LEARN_PATHS = [
    { id: "it", name: "Путь IT", icon: "fa-laptop-code", color: "#06b6d4", subs: ["Разработка", "Данные и AI", "Инфраструктура и Кибербез"] },
    { id: "creator", name: "Путь Творца", icon: "fa-palette", color: "#ec4899", subs: ["Визуальный дизайн", "Иллюстрация и Арт", "Медиа и Контент"] },
    { id: "polyglot", name: "Путь Полиглота", icon: "fa-language", color: "#a855f7", subs: ["Базовое владение", "Продвинутый язык", "Коммуникация и культура"] },
    { id: "strategist", name: "Путь Стратега", icon: "fa-chess", color: "#f59e0b", subs: ["Предпринимательство", "Менеджмент и Лидерство", "Маркетинг и Продажи"] },
    { id: "architect", name: "Путь Зодчего", icon: "fa-compass-drafting", color: "#3b82f6", subs: ["Архитектура", "Инженерия и Конструкции", "Умная среда и Инновации"] },
    { id: "growth", name: "Путь Восхождения", icon: "fa-arrow-up-right-dots", color: "#10b981", subs: ["Мышление и обучение", "Эмоции и Коммуникация", "Цели и Продуктивность"] }
];

const SUB_TO_PATH = {
    "Разработка": "it", "Данные и AI": "it", "Инфраструктура и Кибербез": "it",
    "Визуальный дизайн": "creator", "Иллюстрация и Арт": "creator", "Медиа и Контент": "creator",
    "Базовое владение": "polyglot", "Продвинутый язык": "polyglot", "Коммуникация и культура": "polyglot",
    "Предпринимательство": "strategist", "Менеджмент и Лидерство": "strategist", "Маркетинг и Продажи": "strategist",
    "Архитектура": "architect", "Инженерия и Конструкции": "architect", "Умная среда и Инновации": "architect",
    "Мышление и обучение": "growth", "Эмоции и Коммуникация": "growth", "Цели и Продуктивность": "growth"
};

const BLOCK_KIND = { 0: "text", 1: "video", 2: "image", 3: "quiz", 4: "assignment" };

function allLessons(course) {
    return (course.modules || []).flatMap(m => m.lessons || []);
}

function calcProgress(course) {
    const lessons = allLessons(course);
    if (!lessons.length) return 0;
    return Math.round(lessons.filter(l => l.status === "completed").length / lessons.length * 100);
}

function recomputeAllProgress() {
    (window.ALL_COURSES || []).forEach(c => c.progress = calcProgress(c));
}

window.LEARN_PATHS = LEARN_PATHS;
window.SUB_TO_PATH = SUB_TO_PATH;
window.BLOCK_KIND = BLOCK_KIND;
window.allLessons = allLessons;
window.calcProgress = calcProgress;
window.recomputeAllProgress = recomputeAllProgress;