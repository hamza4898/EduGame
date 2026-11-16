import { state } from "./core.js";

export function updateSliderStack(targetIndex) {
    if (state.isSwitching || targetIndex === state.currentIndex) return;
    state.isSwitching = true;
    state.currentIndex = targetIndex;

    state.cards.forEach((card, index) => {
        card.classList.add("switching");
        card.classList.remove("active", "next", "prev");
        if (index === state.currentIndex) {
            card.classList.add("active");
        } else if (index === (state.currentIndex + 1) % state.cards.length) {
            card.classList.add("next");
        } else {
            card.classList.add("prev");
        }
        setTimeout(() => {
            card.classList.remove("switching");
            state.isSwitching = false;
        }, 800);
    });
}

export function switchMainScreen(targetViewId, activeBtnEl = null) {
    const screens = ["slider-wrapper", "hubSupportView", "hubNotificationsView", "hubWalletView", "hubFriendsView"];

    screens.forEach(screenClassOrId => {
        const el = document.getElementById(screenClassOrId) || document.querySelector(`.${screenClassOrId}`);
        if (el) el.style.display = "none";
    });

    const targetEl = document.getElementById(targetViewId) || document.querySelector(`.${targetViewId}`);
    if (targetEl) {
        targetEl.style.display = targetViewId === "slider-wrapper" ? "flex" : "block";
        targetEl.classList.remove("hidden");
    }

    document.querySelectorAll(".saas-dockbar .dock-item").forEach(item => item.classList.remove("active"));
    if (activeBtnEl) activeBtnEl.classList.add("active");
}