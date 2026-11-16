window.EduGameLearn = window.EduGameLearn || {};

(function (Learn) {
    'use strict';

    const PATHS = window.LEARN_PATHS;
    const SUB_TO_PATH = window.SUB_TO_PATH;
    const BLOCK_KIND = window.BLOCK_KIND;

    const state = {
        ALL_COURSES: [],
        CATALOG_COURSES: [],
        currentCourse: null,
        currentLessonId: null,
        selectedQuizAnswer: null,
        currentUserId: ""
    };

    window.ALL_COURSES = state.ALL_COURSES;

    const API = {
        myCourses:    "/api/courses/GetMyStudentCourses",
        catalog:      "/api/courses/GetCatalogCourses",
        course:       (id) => `/api/courses/GetCourse?id=${id}`,
        startCourse:  "/api/courses/StartCourse",
        buyCourse:    "/api/courses/BuyCourse",
        completeLesson: "/api/courses/CompleteLesson",
        toggleLiked:  "/api/courses/ToggleLiked",
        toggleArchived: "/api/courses/ToggleArchived"
    };

    Object.assign(Learn, { PATHS, SUB_TO_PATH, BLOCK_KIND, state, API });
})(window.EduGameLearn);