function openEditor(courseId) {
    currentCourseId = courseId;
    currentPanel = "desc";
    document.getElementById("teacherDashView").classList.add("hidden");
    document.getElementById("teacherDashView").style.display = "none";
    document.getElementById("editorView").classList.remove("hidden");
    document.getElementById("editorView").style.display = "block";
    document.getElementById("editorCourseTitle").innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(currentCourse().title)}`;
    openPanel("desc");
}

function closeEditor() {
    document.getElementById("editorView").classList.add("hidden");
    document.getElementById("editorView").style.display = "none";
    document.getElementById("teacherDashView").classList.remove("hidden");
    document.getElementById("teacherDashView").style.display = "flex";
    renderTeacherGrid();
    renderTeacherHero();
}

function applyCourseDescription(course, data) {
    if (!data || typeof data !== "object") throw new Error("Сервер вернул некорректные данные курса.");
    course.title = data.courseName ?? data.CourseName ?? course.title;
    course.desc = data.courseDescription ?? data.CourseDescription ?? course.desc;
    course.description = course.desc;
    course.category = data.skillPath ?? data.SkillPath ?? course.category;
    course.sub = data.skillSubSection ?? data.SkillSubSection ?? course.sub;
    course.skillSubSectionId = data.skillSubSectionId ?? data.SkillSubSectionId ?? course.skillSubSectionId;
    course.difficulty = data.difficulty ?? data.Difficulty ?? course.difficulty;
    course.isPaid = data.isPaid ?? data.IsPaid ?? !course.isFree;
    course.isFree = !course.isPaid;
    course.price = data.priceCoins ?? data.PriceCoins ?? course.price;
    course.status = (data.isPublished ?? data.IsPublished) ? "published" : "draft";
    course.cert = { enabled: data.certificateEnabled ?? data.CertificateEnabled ?? false, signer: data.certificateSigner ?? data.CertificateSigner ?? "" };
    course.modules = normalizeCourseSections(data.sections ?? data.Sections);
    const title = document.getElementById("editorCourseTitle");
    if (title && currentCourseId === course.id) title.innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(course.title)}`;
}

function addModule() {
    const course = currentCourse();
    course.modules.push({ id: 0, title: `Новый модуль ${course.modules.length + 1}`, lessons: [] });
    currentLessonSelection = null;
    openPanel("content");
}

function addLesson(moduleIndex) {
    const course = currentCourse();
    const xpByDifficulty = { 1: 15, 2: 30, 3: 50 };
    course.modules[moduleIndex].lessons.push({
        id: 0, title: "Новый урок", xpReward: xpByDifficulty[course.difficulty] ?? 15,
        blocks: [{ id: 0, type: 0, content: "<p></p>", order: 0 }]
    });
    currentLessonSelection = { moduleIndex, lessonIndex: course.modules[moduleIndex].lessons.length - 1 };
    openPanel("content");
}

function openLessonEditor(moduleIndex, lessonIndex) {
    currentLessonSelection = { moduleIndex, lessonIndex };
    openPanel("content");
}

function backToCourseContent() {
    currentLessonSelection = null;
    openPanel("content");
}

function updateModuleTitle(moduleIndex, value) { currentCourse().modules[moduleIndex].title = value; }
function updateLessonTitle(moduleIndex, lessonIndex, value) { currentCourse().modules[moduleIndex].lessons[lessonIndex].title = value; }

function updateLessonXp(moduleIndex, lessonIndex, value) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    lesson.xpReward = Number(value) || 0;
    lesson.blocks.filter(block => block.type === 3).forEach(block => {
        try {
            const quiz = JSON.parse(block.content);
            quiz.xp = lesson.xpReward;
            block.content = JSON.stringify(quiz);
        } catch {
            block.content = JSON.stringify({ question: "", options: ["", ""], answer: 0, xp: lesson.xpReward, coins: 0 });
        }
    });
}

function updateTextBlock(moduleIndex, lessonIndex, blockIndex, editor) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = sanitizeLessonHtml(editor.innerHTML);
}

function updateMediaBlock(moduleIndex, lessonIndex, blockIndex, value) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = value.trim();
}

function updateQuizBlock(moduleIndex, lessonIndex, blockIndex) {
    const block = currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex];
    const quizKey = `${moduleIndex}-${lessonIndex}-${blockIndex}`;
    const options = [...document.querySelectorAll(`#instructorPanel .quiz-option-input[data-quiz-block="${quizKey}"]`)]
        .sort((left, right) => Number(left.dataset.optionIndex) - Number(right.dataset.optionIndex))
        .map(input => input.value);
    const selectedAnswer = document.querySelector(`#instructorPanel input[name="quiz-correct-${moduleIndex}-${lessonIndex}-${blockIndex}"]:checked`);
    block.content = JSON.stringify({
        question: document.getElementById(`quiz-question-${moduleIndex}-${lessonIndex}-${blockIndex}`).value.trim(),
        options, answer: selectedAnswer ? Number(selectedAnswer.value) : 0,
        xp: currentCourse().modules[moduleIndex].lessons[lessonIndex].xpReward, coins: 0
    });
}

function addQuizOption(moduleIndex, lessonIndex, blockIndex) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    const block = lesson.blocks[blockIndex];
    const quiz = JSON.parse(block.content || "{}");
    const options = Array.isArray(quiz.options) ? quiz.options : ["", ""];
    if (options.length >= 6) return;
    updateQuizBlock(moduleIndex, lessonIndex, blockIndex);
    const updatedQuiz = JSON.parse(block.content);
    updatedQuiz.options.push("");
    block.content = JSON.stringify(updatedQuiz);
    document.getElementById("instructorPanel").innerHTML = renderLessonEditor(currentCourse(), moduleIndex, lessonIndex);
    document.querySelector(`#instructorPanel .quiz-option-input[data-option-index="${updatedQuiz.options.length - 1}"]`)?.focus();
}

function removeQuizOption(moduleIndex, lessonIndex, blockIndex, optionIndex) {
    const block = currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex];
    updateQuizBlock(moduleIndex, lessonIndex, blockIndex);
    const quiz = JSON.parse(block.content);
    if (quiz.options.length <= 2) return;
    quiz.options.splice(optionIndex, 1);
    if (quiz.answer === optionIndex) quiz.answer = Math.max(0, optionIndex - 1);
    else if (quiz.answer > optionIndex) quiz.answer -= 1;
    block.content = JSON.stringify(quiz);
    document.getElementById("instructorPanel").innerHTML = renderLessonEditor(currentCourse(), moduleIndex, lessonIndex);
}

function updateAssignmentBlock(moduleIndex, lessonIndex, blockIndex) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks[blockIndex].content = JSON.stringify({
        prompt: document.getElementById(`assignment-prompt-${blockIndex}`).value.trim()
    });
}

function addLessonBlock(moduleIndex, lessonIndex, type) {
    const lesson = currentCourse().modules[moduleIndex].lessons[lessonIndex];
    const defaults = {
        0: "", 1: "", 2: "",
        3: JSON.stringify({ question: "", options: ["", ""], answer: 0, xp: lesson.xpReward, coins: 0 }),
        4: JSON.stringify({ prompt: "" })
    };
    const blockIndex = lesson.blocks.length;
    lesson.blocks.push({ id: 0, type, content: defaults[type], order: blockIndex });
    openPanel("content").then(() => {
        if (type === 0) {
            const editor = document.getElementById(`lessonText-${moduleIndex}-${lessonIndex}-${blockIndex}`);
            editor?.focus();
        }
    });
}

function removeLessonBlock(moduleIndex, lessonIndex, blockIndex) {
    currentCourse().modules[moduleIndex].lessons[lessonIndex].blocks.splice(blockIndex, 1);
    openPanel("content");
}

function formatLessonText(command) {
    document.execCommand(command, false);
    const selection = window.getSelection();
    const editor = selection?.anchorNode?.parentElement?.closest(".lesson-rich-editor");
    if (editor) {
        const [moduleIndex, lessonIndex, blockIndex] = editor.id.replace("lessonText-", "").split("-").map(Number);
        updateTextBlock(moduleIndex, lessonIndex, blockIndex, editor);
    }
}

function normalizeVideoUrl(value) {
    try {
        const url = new URL(value);
        if (url.protocol !== "https:") return null;
        if (url.hostname === "youtu.be") {
            const videoId = url.pathname.slice(1);
            return videoId ? `https://www.youtube-nocookie.com/embed/${encodeURIComponent(videoId)}` : null;
        }
        if (["youtube.com", "www.youtube.com", "m.youtube.com"].includes(url.hostname)) {
            const videoId = url.searchParams.get("v") ?? url.pathname.match(/^\/embed\/([^/]+)/)?.[1];
            return videoId ? `https://www.youtube-nocookie.com/embed/${encodeURIComponent(videoId)}` : null;
        }
        if (url.hostname === "vimeo.com") {
            const videoId = url.pathname.split("/").filter(Boolean).at(-1);
            return videoId && /^\d+$/.test(videoId) ? `https://player.vimeo.com/video/${videoId}` : null;
        }
        return ["www.youtube-nocookie.com", "player.vimeo.com"].includes(url.hostname) ? url.href : null;
    } catch { return null; }
}

async function saveCourseContent() {
    const course = currentCourse();
    const button = document.querySelector("#instructorPanel .save-course-content");
    const originalText = button?.innerHTML;
    try {
        for (const module of course.modules) {
            if (!module.title.trim()) throw new Error("Укажите название каждого модуля.");
            for (const lesson of module.lessons) {
                if (!lesson.title.trim()) throw new Error("Укажите название каждого урока.");
                if (lesson.xpReward < 0 || !Number.isInteger(Number(lesson.xpReward))) throw new Error("Награда за урок должна быть целым числом XP от 0.");
                for (const block of lesson.blocks) {
                    if (block.type === 1) {
                        const videoUrl = normalizeVideoUrl(block.content);
                        if (!videoUrl) throw new Error("Для видео укажите HTTPS-ссылку на YouTube или Vimeo.");
                        block.content = videoUrl;
                    }
                    if (block.type === 2) {
                        let imageUrl;
                        try { imageUrl = new URL(block.content); } catch { throw new Error("Для изображения укажите корректную HTTPS-ссылку."); }
                        if (imageUrl.protocol !== "https:") throw new Error("Ссылка на изображение должна использовать HTTPS.");
                        block.content = imageUrl.href;
                    }
                    if (block.type === 3) {
                        let quiz;
                        try {
                            quiz = JSON.parse(block.content);
                        } catch {
                            throw new Error("Проверьте данные квиза: не удалось прочитать вопрос и варианты ответа.");
                        }
                        quiz.question = String(quiz.question ?? "").trim();
                        quiz.options = Array.isArray(quiz.options) ? quiz.options.map(option => String(option).trim()) : [];
                        if (!quiz.question) throw new Error("Заполните вопрос в каждом квизе.");
                        if (quiz.options.length < 2 || quiz.options.length > 6 || quiz.options.some(option => !option)) {
                            throw new Error("В каждом квизе должно быть от 2 до 6 непустых вариантов ответа.");
                        }
                        if (!Number.isInteger(quiz.answer) || quiz.answer < 0 || quiz.answer >= quiz.options.length) {
                            throw new Error("Отметьте правильный вариант ответа в каждом квизе.");
                        }
                        quiz.xp = Number(lesson.xpReward) || 0;
                        quiz.coins = 0;
                        block.content = JSON.stringify(quiz);
                    }
                }
            }
        }
        if (button) { button.disabled = true; button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Сохраняем…'; }
        const response = await apiFetch(`/api/courses/UpdateCourseContent?courseId=${encodeURIComponent(course.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                sections: course.modules.map(module => ({
                    id: module.id, title: module.title,
                    lessons: module.lessons.map(lesson => ({
                        id: lesson.id, title: lesson.title, xpReward: Number(lesson.xpReward) || 0,
                        blocks: lesson.blocks.map((block, order) => ({ type: block.type, content: block.content, order }))
                    }))
                }))
            })
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            const result = payload?.value ?? payload?.Value ?? payload;
            const errors = result?.errors ?? result?.Errors ?? payload?.errors ?? payload?.Errors ?? [];
            const messages = errors.map(error => typeof error === "string" ? error : error.errorMessage ?? error.ErrorMessage).filter(Boolean);
            throw new Error(messages.join("\n") || payload?.error || payload?.title || `Не удалось сохранить содержание (${response.status}).`);
        }
        applyCourseDescription(course, unwrapApiResult(payload, "Не удалось сохранить содержание курса."));
        currentLessonSelection = null;
        await openPanel("content");
        showToast("Содержание сохранено", "Модули, уроки, материалы и награды XP обновлены.");
    } catch (error) {
        console.error("Не удалось сохранить содержание курса:", error);
        showToast("Не удалось сохранить", error.message || "Проверьте данные и повторите попытку.");
    } finally {
        if (button) { button.disabled = false; button.innerHTML = originalText; }
    }
}

async function saveDesc() {
    const c = currentCourse();
    const title = document.getElementById("pTitle").value.trim();
    const description = document.getElementById("pDesc").value.trim();
    const subSectionId = Number(document.getElementById("pSubCategory").value);
    const selectedPath = TEACHER_PATHS.find(path => String(path.id) === document.getElementById("pCategory").value);
    const selectedSubSection = selectedPath?.subs.find(sub => sub.id === subSectionId);
    const button = document.querySelector("#instructorPanel button[onclick='saveDesc()']");
    if (title.length < 5 || title.length > 200) { showToast("Ошибка", "Название курса должно содержать от 5 до 200 символов."); return; }
    if (description.length < 10 || description.length > 2000) { showToast("Ошибка", "Описание курса должно содержать от 10 до 2000 символов."); return; }
    if (!selectedSubSection) { showToast("Ошибка", "Выберите категорию и одну из её подкатегорий."); return; }
    const originalText = button?.innerHTML;
    if (button) { button.disabled = true; button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Сохраняем...'; }
    try {
        const response = await apiFetch(`/api/courses/ChangeCourseDescription?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ courseName: title, courseDescription: description, skillSubSectionId: subSectionId })
        });
        const payload = await response.json().catch(() => null);
        unwrapApiResult(payload, "Не удалось сохранить изменения курса.");
        if (!response.ok) throw new Error(payload?.error || payload?.title || `Не удалось сохранить курс (${response.status}).`);
        c.title = title; c.desc = description; c.description = description;
        c.skillSubSectionId = subSectionId; c.category = selectedPath.name; c.sub = selectedSubSection.name;
        document.getElementById("editorCourseTitle").innerHTML = `<i class="fa-solid fa-chalkboard-user"></i> ${escapeHtml(c.title)}`;
        showToast("Сохранено! 💾", "Название, описание и подкатегория курса обновлены.");
    } catch (error) {
        console.error("Не удалось сохранить изменения курса:", error);
        showToast("Не удалось сохранить", error.message || "Проверьте соединение и повторите попытку.");
    } finally {
        if (button) { button.disabled = false; button.innerHTML = originalText; }
    }
}