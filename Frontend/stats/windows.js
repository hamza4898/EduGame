(function (Stats) {
    'use strict';

    function openFocusTree(treeId) {
        const overlay = document.getElementById(treeId);
        if (overlay) overlay.classList.add('open');
    }
    function closeFocusTree(treeId) {
        const overlay = document.getElementById(treeId);
        if (overlay) overlay.classList.remove('open');
    }
    function openSpecificTree(treeId) {
        const overlay = document.getElementById(treeId);
        if (overlay) {
            overlay.classList.add('open');
            const scrollCanvas = overlay.querySelector('.focus-canvas-scroll');
            if (scrollCanvas) {
                scrollCanvas.scrollLeft = (scrollCanvas.scrollWidth - scrollCanvas.clientWidth) / 2;
            }
        }
    }
    function closeSpecificTree(treeId) {
        const overlay = document.getElementById(treeId);
        if (overlay) overlay.classList.remove('open');
    }

    function openCoursesModal(secKey) {
        alert("Просмотр курсов раздела временно недоступен. Данные загружаются напрямую из API.");
    }

    function closeCoursesModal() {
        const modal = document.getElementById("coursesSectionModal");
        if (modal) modal.classList.remove('open');
    }

    Object.assign(Stats, { openFocusTree, closeFocusTree, openSpecificTree, closeSpecificTree, openCoursesModal, closeCoursesModal });
})(window.EduGameStats);