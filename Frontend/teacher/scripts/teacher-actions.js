async function toggleTeacherCourseArchive(courseId) {
    const course = getCourse(courseId);
    if (!course) return;
    try {
        const response = await apiFetch(`/api/courses/ToggleArchived?courseId=${encodeURIComponent(courseId)}`, { method: "POST" });
        if (!response.ok) throw new Error(`Не удалось обновить архив (${response.status}).`);
        course.isArchived = unwrapApiResult(await response.json(), "Не удалось обновить архив курса.");
        if (!course.isArchived && !teacherCourses.some(item => item.id === course.id)) {
            teacherCourses.push(course);
            window.teacherCourses = teacherCourses;
        }
        renderTeacherGrid();
        renderTeacherHero();
    } catch (error) {
        console.error("Не удалось обновить архив курса:", error);
        showToast("Не удалось обновить архив", error.message || "Проверьте соединение и повторите попытку.");
    }
}

async function toggleTeacherCourseLike(courseId) {
    const course = getCourse(courseId);
    if (!course) return;
    try {
        const response = await apiFetch(`/api/courses/ToggleLiked?courseId=${encodeURIComponent(courseId)}`, { method: "POST" });
        if (!response.ok) throw new Error(`Не удалось обновить избранное (${response.status}).`);
        course.isLiked = unwrapApiResult(await response.json(), "Не удалось обновить избранное курса.");
        if (course.isLiked) teacherLikedCourseIds.add(courseId);
        else teacherLikedCourseIds.delete(courseId);
        renderTeacherGrid();
    } catch (error) {
        console.error("Не удалось обновить избранное курса:", error);
        showToast("Не удалось обновить избранное", error.message || "Проверьте соединение и повторите попытку.");
    }
}

function openCreateModal() {
    const modal = document.getElementById("modalCreateCourse");
    if (modal) modal.classList.add("open");
}

function closeCreateModal() {
    const modal = document.getElementById("modalCreateCourse");
    if (modal) modal.classList.remove("open");
}

async function createCourse() {
    const skillSubSectionId = Number(document.getElementById("createSubPath").value);
    const title = document.getElementById("createTitle").value.trim();
    const desc = document.getElementById("createDesc").value.trim();
    const isFree = document.getElementById("createIsFree").checked;
    const price = isFree ? 0 : (parseInt(document.getElementById("createPrice").value, 10) || 0);
    const communityName = document.getElementById("createCommunityName").value.trim();
    const communityDescription = document.getElementById("createCommunityDesc").value.trim();
    const button = document.getElementById("btnCreateCourse");

    if (!skillSubSectionId) { showToast("Ошибка", "Выберите путь обучения и подкатегорию!"); return; }
    if (title.length < 5) { showToast("Ошибка", "Название курса должно быть не менее 5 символов!"); return; }
    if (desc.length < 10) { showToast("Ошибка", "Дайте более подробное описание курса (мин. 10 символов)."); return; }
    if (communityName.length < 3 || communityName.length > 30) { showToast("Ошибка", "Название сообщества должно содержать от 3 до 30 символов."); return; }
    if (!isFree && price <= 0) { showToast("Ошибка", "Цена платного курса должна быть больше нуля."); return; }

    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Создаём курс и сообщество...';
    try {
        const response = await apiFetch("/api/courses/CreateCourse", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                title, description: desc, isPaid: !isFree, priceCoins: price,
                difficulty: Number(document.getElementById("createDifficulty").value),
                skillSubSectionId, communityName, communityDescription,
                privacyType: Number(document.getElementById("createCommunityPrivacy").value),
                membersLimit: Number(document.getElementById("createCommunityLimit").value)
            })
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            const rawErrors = payload?.errors ?? payload?.Errors;
            const errors = Array.isArray(rawErrors) ? rawErrors : [];
            const errorMessages = errors.map(error => typeof error === "string" ? error : (error.errorMessage ?? error.ErrorMessage ?? error.identifier ?? error.Identifier)).filter(Boolean);
            throw new Error(payload?.error ?? payload?.title ?? (errorMessages.join("\n") || `Не удалось создать курс (${response.status}).`));
        }
        const result = unwrapApiResult(payload, "Сервер не подтвердил создание курса.");
        await loadTeacherCourses();
        const createdCourseId = result.courseId ?? result.CourseId;
        const createdCourse = teacherCourses.find(course => course.id === createdCourseId)
            ?? teacherCourses.find(course => course.communityName === (result.communityName ?? result.CommunityName) && course.title === title);
        if (createdCourse) currentCourseId = createdCourse.id;

        document.getElementById("createTitle").value = "";
        document.getElementById("createDesc").value = "";
        document.getElementById("createCommunityName").value = "";
        document.getElementById("createCommunityName").dataset.edited = "";
        document.getElementById("createCommunityDesc").value = "";
        document.getElementById("createPath").value = "";
        document.getElementById("createSubPath").innerHTML = '<option value="">Сначала выберите путь</option>';
        document.getElementById("createSubPath").disabled = true;
        document.getElementById("createIsFree").checked = true;
        document.getElementById("createPrice").value = "0";
        document.getElementById("createPrice").disabled = true;

        closeCreateModal();
        showToast("Курс создан! 🎓", `Курс «${title}» и связанное сообщество «${communityName}» готовы.`);
    } catch (error) {
        console.error("Ошибка создания курса и сообщества:", error);
        showToast("Не удалось создать курс", error.message || "Проверьте соединение и повторите попытку.");
    } finally {
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

function acceptSolution(idx) {
    const c = currentCourse(); const s = c.solutions[idx];
    c.solutions.splice(idx, 1); openPanel("solutions");
    showToast("Решение принято! ✅", `${s.student} получит XP и монеты за задание.`);
}

function returnSolution(idx) {
    const c = currentCourse(); const s = c.solutions[idx];
    c.solutions.splice(idx, 1); openPanel("solutions");
    showToast("Возвращено на доработку", `${s.student} получит комментарий с замечаниями.`);
}

async function publishNews() {
    const c = currentCourse();
    const text = document.getElementById("newsText").value.trim();
    if (text.length < 3) { showToast("Пусто!", "Напиши текст новости."); return; }
    try {
        const publishedNews = await courseApiRequest(`/api/courses/PublishCourseNews?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ text })
        });
        c.news = [publishedNews, ...(c.news || []).filter(news => news.id !== publishedNews.id)];
        c.newsCount = Math.max(Number(c.newsCount) || 0, c.news.length);
        document.getElementById("newsText").value = "";
        await openPanel("news");
        if (c.status !== "published") {
            showToast("Новость сохранена", "Курс ещё в разработке, поэтому уведомление учащимся не отправлено. Для уведомления опубликуйте курс и новость.");
        } else if ((publishedNews.notifiedStudents ?? publishedNews.NotifiedStudents ?? 0) > 0) {
            const notifiedStudents = publishedNews.notifiedStudents ?? publishedNews.NotifiedStudents;
            showToast("Новость опубликована! 📢", `Уведомление отправлено учащимся курса: ${notifiedStudents}.`);
        } else {
            showToast("Новость опубликована! 📢", "Новость сохранена. На курс пока никто не записан, поэтому уведомлять некого.");
        }
    } catch (error) { showToast("Не удалось опубликовать новость", error.message); }
}

async function publishCourse() {
    const c = currentCourse();
    try {
        const readiness = await courseApiRequest(`/api/courses/PublishCourse?courseId=${encodeURIComponent(c.id)}`, { method: "POST" });
        c.status = "published"; c.readiness = readiness;
        await openPanel("publish"); await loadTeacherCourses();
        showToast("Курс опубликован! 🌍", `«${c.title}» теперь виден студентам в каталоге.`);
    } catch (error) { showToast("Не удалось опубликовать курс", error.message); }
}

async function unpublishCourse() {
    const c = currentCourse();
    try {
        await courseApiRequest(`/api/courses/UnpublishCourse?courseId=${encodeURIComponent(c.id)}`, { method: "POST" });
        c.status = "draft";
        await openPanel("publish"); await loadTeacherCourses();
        showToast("Курс скрыт", "Курс больше не отображается в каталоге студентов.");
    } catch (error) { showToast("Не удалось снять курс с публикации", error.message); }
}

function archiveCourse(id) {
    const c = id ? getCourse(id) : currentCourse();
    c.status = "archived";
    if (id) renderTeacherGrid(); else openPanel("publish");
    renderTeacherHero();
    showToast("В архиве 📦", `«${c.title}» перемещён в архив.`);
}

function restoreCourse(id) {
    const c = id ? getCourse(id) : currentCourse();
    c.status = "draft";
    if (id) renderTeacherGrid(); else openPanel("publish");
    renderTeacherHero();
    showToast("Возвращён 🔄", `«${c.title}» снова в разработке.`);
}

async function savePrice() {
    const c = currentCourse();
    const isFree = document.getElementById("pFree").checked;
    const price = parseInt(document.getElementById("pPrice").value, 10) || 0;
    if (isFree ? price !== 0 : price <= 0) { showToast("Некорректная цена", "Для бесплатного курса цена равна 0, для платного — больше 0."); return; }
    try {
        await courseApiRequest(`/api/courses/SaveCoursePrice?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ isPaid: !isFree, priceCoins: price })
        });
        c.isFree = isFree; c.isPaid = !isFree; c.price = price;
        showToast("Цена сохранена! 🏷️", c.isFree ? "Курс бесплатный." : `Цена: ${c.price} монет.`);
    } catch (error) { showToast("Не удалось сохранить цену", error.message); }
}

async function addPromo() {
    const c = currentCourse();
    const code = document.getElementById("promoCode").value.trim().toUpperCase();
    const discount = parseInt(document.getElementById("promoDiscount").value, 10) || 0;
    if (code.length < 3 || discount < 1 || discount > 100) { showToast("Ошибка", "Нужен код (3–32 символа) и скидка 1–100%."); return; }
    try {
        await courseApiRequest(`/api/courses/AddCoursePromo?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ code, discountPercent: discount })
        });
        await openPanel("price");
        showToast("Промокод создан! 🎟️", `${code} даёт скидку ${discount}%.`);
    } catch (error) { showToast("Не удалось создать промокод", error.message); }
}

async function removePromo(idx) {
    const c = currentCourse(); const promo = c.promo[idx];
    if (!promo) return;
    try {
        await courseApiRequest(`/api/courses/RemoveCoursePromo?courseId=${encodeURIComponent(c.id)}&promoId=${promo.id}`, { method: "DELETE" });
        await openPanel("price");
    } catch (error) { showToast("Не удалось удалить промокод", error.message); }
}

async function addCoTeacher() {
    const c = currentCourse();
    const name = document.getElementById("coName").value.trim();
    if (name.length < 2) { showToast("Ошибка", "Введи ник со-автора."); return; }
    try {
        await courseApiRequest(`/api/courses/InviteCourseTeacher?courseId=${encodeURIComponent(c.id)}`, {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ userName: name })
        });
        await openPanel("access");
        showToast("Доступ выдан! 🛡️", `${name} получит уведомление и сможет редактировать курс.`);
    } catch (error) { showToast("Не удалось выдать доступ", error.message); }
}

async function removeCoTeacher(idx) {
    const c = currentCourse(); const teacher = c.coTeachers[idx];
    if (!teacher) return;
    try {
        await courseApiRequest(`/api/courses/RemoveCourseTeacher?courseId=${encodeURIComponent(c.id)}&userName=${encodeURIComponent(teacher.userName)}`, { method: "DELETE" });
        await openPanel("access");
        showToast("Доступ снят", `${teacher.userName} больше не со-автор.`);
    } catch (error) { showToast("Не удалось снять доступ", error.message); }
}

async function saveCert() {
    const c = currentCourse();
    const enabled = document.getElementById("certEnabled").checked;
    const signer = document.getElementById("certSigner").value.trim();
    try {
        await courseApiRequest(`/api/courses/SaveCourseCertificate?courseId=${encodeURIComponent(c.id)}`, {
            method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ enabled, signer })
        });
        c.cert = { enabled, signer };
        showToast("Сертификаты настроены! 🏆", enabled ? "Студенты смогут скачать сертификат после завершения." : "Выдача сертификатов выключена.");
    } catch (error) { showToast("Не удалось сохранить настройки сертификата", error.message); }
}