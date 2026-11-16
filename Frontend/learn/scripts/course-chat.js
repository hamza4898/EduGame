(function (Learn) {
    'use strict';
    const { state } = Learn;
    const { findLesson } = Learn.utils;

    function goToCourseChat(withLessonContext) {
        if (!state.currentCourse) return;
        const ctx = withLessonContext
            ? encodeURIComponent(`📚 Обсуждаем урок «${findLesson(state.currentCourse, state.currentLessonId)?.lesson.title}» курса «${state.currentCourse.title}»`)
            : "";
        window.location.href = `Chat.html?room=${encodeURIComponent(state.currentCourse.communityChat)}${ctx ? "&highlight=" + ctx : ""}`;
    }

    function initCourseChat() {
        const btnDiscuss = document.getElementById("btnDiscussLesson");
        const btnChatTop = document.getElementById("btnCourseChatTop");
        if (btnDiscuss) btnDiscuss.addEventListener("click", () => goToCourseChat(true));
        if (btnChatTop) btnChatTop.addEventListener("click", () => goToCourseChat(false));
    }

    Learn.initCourseChat = initCourseChat;
})(window.EduGameLearn);