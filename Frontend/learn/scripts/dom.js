(function (Learn) {
    'use strict';

    const el = {
        dashboardView: document.getElementById("dashboardView"),
        lessonView: document.getElementById("lessonView"),
        catalogView: document.getElementById("catalogView"),
        courseCompletionView: document.getElementById("courseCompletionView")
    };
    el.lessonLayout = el.lessonView.querySelector(".learn-layout-container");

    Learn.el = el;
})(window.EduGameLearn);