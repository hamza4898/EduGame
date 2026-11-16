import { state } from "./core.js";
import { syncUserBusinessPassport } from "./passport.js";
import { updateSliderStack, switchMainScreen } from "./slider.js";
import { showWalletForCurrentRole } from "./roles.js";
import { initializeDeveloperTools } from "./devtools.js";
import { loadNotifications, connectNotifications } from "./notifications.js";
import { handleSendMessageToAi, loadAiSupportHistory } from "./ai-chat.js";
import { loadAllFriends } from "./friends.js";

document.addEventListener("DOMContentLoaded", async () => {
    console.log("[EduGame Core] Сборка интерфейса главной стены...");
    connectNotifications();
    loadNotifications().catch(error => console.error("[Notifications] Failed to load:", error));

    state.cards = Array.from(document.querySelectorAll(".hub-app-card"));
    if (state.cards.length >= 3) {
        state.cards[state.currentIndex].classList.add("active");
        state.cards[(state.currentIndex + 1) % state.cards.length].classList.add("next");
        state.cards[(state.currentIndex + 2) % state.cards.length].classList.add("prev");
    }

    state.cards.forEach((card, index) => {
        card.addEventListener("mouseenter", () => {
            if (!card.classList.contains("active")) updateSliderStack(index);
        });

        card.addEventListener("click", (e) => {
            if (!card.classList.contains("active")) {
                e.preventDefault();
                updateSliderStack(index);
            } else {
                const appName = card.getAttribute("data-app");
                const userRole = localStorage.getItem("UserRole");

                switch (appName) {
                    case "section-chat":
                        window.location.href = "Chat.html";
                        break;
                    case "section-stats":
                        window.location.href = userRole === "2" ? "TeacherStats.html" : "Stats.html";
                        break;
                    case "section-learn":
                        window.location.href = userRole === "2" ? "LearnTeacher.html" : "Learn.html";
                        break;
                    default:
                        alert(`Фишка ${appName} пока находится в разработке!`);
                }
            }
        });
    });

    const triggerHub = document.getElementById("btnMainHubTrigger");
    if (triggerHub) triggerHub.addEventListener("click", () => switchMainScreen("slider-wrapper", triggerHub));

    const triggerNotif = document.getElementById("notificationBtn");
    if (triggerNotif) {
        triggerNotif.addEventListener("click", () => {
            loadNotifications().catch(error => console.error("[Notifications] Failed to load:", error));
            switchMainScreen("hubNotificationsView", triggerNotif);
        });
    }

    const triggerWallet = document.getElementById("hudWalletBtn");
    if (triggerWallet) triggerWallet.addEventListener("click", () => {
        showWalletForCurrentRole();
        switchMainScreen("hubWalletView", triggerWallet);
    });

    const teacherWalletWithdrawBtn = document.getElementById("teacherWalletWithdrawBtn");
    if (teacherWalletWithdrawBtn) {
        teacherWalletWithdrawBtn.addEventListener("click", () => {
            alert("Платёжный шлюз пока не подключён. Вывод средств временно недоступен.");
        });
    }

    const triggerFriends = document.getElementById("hudFriendsBtn");
    if (triggerFriends) {
        triggerFriends.addEventListener("click", () => {
            switchMainScreen("hubFriendsView", triggerFriends);
            loadAllFriends();
        });
    }

    document.querySelectorAll(".tab-friend").forEach(tab => {
        tab.addEventListener("click", () => {
            document.querySelectorAll(".tab-friend").forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            state.currentFriendsTab = tab.dataset.tab;
            import("./friends.js").then(({ renderFriendsFeed }) => renderFriendsFeed());
        });
    });

    const triggerSettings = document.getElementById("hudSettingsBtn");
    if (triggerSettings) {
        triggerSettings.addEventListener("click", () => {
            console.log("[Navigation] Переход в настройки безопасности...");
            window.location.href = "Settings.html";
        });
    }

    const btnLogo = document.getElementById("btnMainLogoTrigger");
    const subMenu = document.getElementById("hubOptionsSubMenu");
    const arrowIcon = document.getElementById("logoArrowIcon");
    if (btnLogo && subMenu) {
        btnLogo.addEventListener("click", (e) => {
            e.stopPropagation();
            const isOpen = subMenu.style.display === "block";
            subMenu.style.display = isOpen ? "none" : "block";
            if (arrowIcon) arrowIcon.style.transform = isOpen ? "rotate(0deg)" : "rotate(180deg)";
        });

        document.addEventListener("click", (e) => {
            if (!subMenu.contains(e.target) && e.target !== btnLogo) {
                subMenu.style.display = "none";
                if (arrowIcon) arrowIcon.style.transform = "rotate(0deg)";
            }
        });
    }

    const btnSupportForm = document.getElementById("btnOpenSupportForm");
    if (btnSupportForm) btnSupportForm.addEventListener("click", () => {
        switchMainScreen("hubSupportView");
        void loadAiSupportHistory();
    });

    const btnSendAi = document.getElementById("btnSendToAiSupport");
    const inputAi = document.getElementById("aiSupportInput");
    if (btnSendAi) btnSendAi.addEventListener("click", handleSendMessageToAi);
    if (inputAi) {
        inputAi.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                handleSendMessageToAi();
            }
        });
    }

    const modal2fa = document.getElementById("security-modal-overlay");
    const btnSkip = document.getElementById("btn-skip-security");
    if (btnSkip && modal2fa) {
        btnSkip.addEventListener("click", () => {
            modal2fa.classList.add("animate__animated", "animate__fadeOut");
            setTimeout(() => {
                modal2fa.style.display = "none";
                modal2fa.classList.remove("animate__animated", "animate__fadeOut");
            }, 400);
        });
    }

    document.getElementById("btnCloseNotifDetail")?.addEventListener("click", () => {
        document.getElementById("modalNotifDetail")?.classList.remove("open");
    });

    await syncUserBusinessPassport();
    await initializeDeveloperTools();

    try {
        const response = await window.apiFetch("/api/user/GetMySecurityPassport");
        if (response.ok) {
            const data = await response.json();
            state.currentUserRole = String(data.userRole ?? "");
            if (state.currentUserRole) localStorage.setItem("UserRole", state.currentUserRole);
            localStorage.setItem("isTwoFactorEnabled", data.twoFactorEnabled);

            if (data.twoFactorEnabled === false && modal2fa) {

                const lastShowTime = localStorage.getItem("last_2fa_warning_time");
                const now = Date.now();

                const oneWeekMs = 7 * 24 * 60 * 60 * 1000;

                if (!lastShowTime || (now - lastShowTime > oneWeekMs)) {

                    console.log("[2FA Warning] Протокол времени истек. Выводим окно...");
                    modal2fa.style.display = "flex";

                    localStorage.setItem("last_2fa_warning_time", now);
                } else {
                    const daysLeft = ((oneWeekMs - (now - lastShowTime)) / (1000 * 60 * 60 * 24)).toFixed(1);
                    console.log(`[2FA Warning] Окно скрыто фильтром времени. Следующий показ через ${daysLeft} дн.`);
                    modal2fa.style.display = "none";
                }
            } else if (modal2fa) {
                modal2fa.style.display = "none";
            }
        }
    } catch (err) {
        console.log("[2FA Platform] Бэкенд оффлайн, запущен автономный демо-режим.");
        if (modal2fa) modal2fa.style.display = "flex";
    }
});