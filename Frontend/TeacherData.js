function courseChecks(course) {
    const lessons = course.modules.flatMap(m => m.lessons);
    const checks = [
        { group: "Структура и содержание", label: "Заполнено описание (от 20 символов)", done: (course.desc || "").trim().length >= 20 },
        { group: "Структура и содержание", label: "Создано минимум 3 урока", done: lessons.length >= 3 },
        { group: "Структура и содержание", label: "Уроки заполнены контентом", done: lessons.length > 0 && lessons.filter(l => l.hasContent).length >= 2 },
        { group: "Подача", label: "Есть практические задания", done: lessons.some(l => l.hasTask) },
        { group: "Подача", label: "Опубликована первая новость", done: (course.news || []).length > 0 },
        { group: "Подача", label: "Настроена цена или курс бесплатный", done: course.isFree || course.price > 0 }
    ];
    const done = checks.filter(c => c.done).length;
    return { checks, done, total: checks.length, pct: Math.round((done / checks.length) * 100) };
}

function courseIncome(course) {
    if (course.isFree) return course.students * 2;
    return course.students * course.price;
}