(function (Learn) {
    'use strict';
    const { state, API, SUB_TO_PATH, BLOCK_KIND } = Learn;

    function normalizeCourse(dto) {
        const byOrder = (a, b) => (a.order ?? 0) - (b.order ?? 0);
        const subName = dto.skillSubSection?.name || "Курс";
        const pathId = SUB_TO_PATH[subName] || "it";

        const course = {
            id: dto.id,
            title: dto.title,
            description: dto.description || "",
            longDescription: dto.longDescription || dto.description || "",
            whatYouLearn: dto.whatYouLearn || [],
            pathId: pathId,
            sub: subName,
            price: dto.priceCoins ?? 0,
            isFree: !dto.isPaid,
            owned: dto.owned ?? false,
            progress: dto.progress ?? 0,
            rating: dto.rating ?? 0,
            studentsCount: dto.studentsCount ?? 0,
            authorType: "community-type",
            authorIcon: "fa-users",
            authorName: dto.community?.communityName
                ? `Сообщество: ${dto.community.communityName}`
                : "Сообщество EduGame",
            communityChat: dto.community?.communityName || "general",
            isLiked: dto.isLiked ?? false,
            isArchived: dto.isArchived ?? false,
            reviews: dto.reviews || [],
            modules: (dto.sections || []).slice().sort(byOrder).map(s => ({
                id: s.id,
                title: s.title,
                lessons: (s.lessons || []).slice().sort(byOrder).map(l => ({
                    id: String(l.id),
                    title: l.title,
                    status: l.status || "locked",
                    blocks: (l.blocks || []).slice().sort(byOrder).map(b => ({
                        kind: BLOCK_KIND[b.type] ?? "text",
                        content: b.content
                    }))
                }))
            }))
        };
        return course;
    }

    async function loadMyCourses() {
        try {
            const r = await apiFetch(API.myCourses);
            if (!r.ok) throw new Error("Ошибка загрузки");
            const dto = await r.json();
            const list = (Array.isArray(dto) ? dto : []).map(normalizeCourse);
            state.ALL_COURSES = list;
            window.ALL_COURSES = list;
            return list;
        } catch (e) {
            console.error("[Learn] Не удалось загрузить мои курсы:", e);
            state.ALL_COURSES = [];
            window.ALL_COURSES = [];
            return [];
        }
    }

    async function loadCatalogCourses(query = "", priceFilter = "all") {
        try {
            const params = new URLSearchParams();
            if (query) params.set("query", query);
            if (priceFilter !== "all") params.set("priceFilter", priceFilter);
            const r = await apiFetch(`${API.catalog}?${params.toString()}`);
            if (!r.ok) throw new Error("Ошибка каталога");
            const dto = await r.json();
            state.CATALOG_COURSES = (Array.isArray(dto) ? dto : []).map(normalizeCourse);
            return state.CATALOG_COURSES;
        } catch (e) {
            console.error("[Learn] Не удалось загрузить каталог:", e);
            state.CATALOG_COURSES = [];
            return [];
        }
    }

    async function loadCourseDetails(courseId) {
        const response = await apiFetch(API.course(courseId));
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            throw new Error(payload?.error || payload?.title || `Не удалось загрузить уроки курса (${response.status}).`);
        }

        const dto = payload?.value ?? payload?.Value ?? payload;
        const sections = dto?.sections ?? dto?.Sections;
        if (!Array.isArray(sections)) {
            throw new Error("Сервер не вернул структуру курса. Попробуйте обновить страницу или обратитесь к преподавателю.");
        }

        return { modules: normalizeCourse({ ...dto, sections }).modules };
    }

    function normalizeReview(review) {
        return {
            id: review.id ?? review.Id,
            studentId: review.studentId ?? review.StudentId ?? "",
            student: review.student ?? review.Student ?? "Ученик",
            avatarUrl: review.avatarUrl ?? review.AvatarUrl ?? "",
            stars: Number(review.stars ?? review.Stars) || 0,
            text: review.text ?? review.Text ?? "",
            createdAt: review.createdAt ?? review.CreatedAt
        };
    }

    async function loadCourseReviews(courseId) {
        const response = await apiFetch(`/api/courses/GetCourseReviews?courseId=${encodeURIComponent(courseId)}`);
        const payload = await response.json().catch(() => null);
        if (!response.ok) {
            throw new Error(payload?.error || payload?.title || `Не удалось загрузить отзывы (${response.status}).`);
        }
        const reviews = payload?.value ?? payload?.Value ?? payload;
        if (!Array.isArray(reviews)) throw new Error("Сервер вернул некорректный список отзывов.");
        return reviews.map(normalizeReview);
    }

    Object.assign(Learn, { normalizeCourse, loadMyCourses, loadCatalogCourses, loadCourseDetails, normalizeReview, loadCourseReviews });
})(window.EduGameLearn);