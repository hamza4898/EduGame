import { state, eduGameRanksConfig } from "./core.js";

export async function syncUserBusinessPassport() {
    try {
        const response = await window.apiFetch("/api/user/GetMyBusinessPassport");
        if (!response.ok) return;

        const passport = await response.json();

        const hudUsernameDisplay = document.getElementById("hudUsernameDisplay");
        const elLevel = document.getElementById("hudUserLevelDisplay");
        const xpText = document.getElementById("hudUserXpText");
        const elRank = document.getElementById("hudUserRankDisplay");
        const xpBar = document.getElementById("hudUserXpBar");
        const coins = document.getElementById("hudUserCoinsDisplay");
        const walletCoins = document.getElementById("teacherWalletCoinsBalance");
        state.currentUserRole = String(passport.userRole ?? passport.UserRole ?? "");
        if (state.currentUserRole) localStorage.setItem("UserRole", state.currentUserRole);

        if (hudUsernameDisplay) hudUsernameDisplay.textContent = passport.userName;
        const avatarUrl = passport.user?.avatarUrl ?? passport.user?.AvatarUrl;
        if (avatarUrl) {
            document.querySelectorAll(".saas-avatar-mini").forEach(avatar => {
                avatar.style.backgroundImage = `url("${avatarUrl.replaceAll('"', '%22')}")`;
                avatar.style.backgroundSize = "cover";
                avatar.style.backgroundPosition = "center";
            });
        }
        if (elLevel) elLevel.textContent = `Lvl ${passport.levelingProgress.level}`;

        if (xpText) {
            xpText.textContent = `${passport.levelingProgress.currentXpInLevel} / ${passport.levelingProgress.nextLevelXpRequired} XP`;
        }

        if (xpBar) {
            const pct = (passport.levelingProgress.currentXpInLevel / passport.levelingProgress.nextLevelXpRequired) * 100;
            xpBar.style.width = `${Math.min(pct, 100)}%`;
        }

        if (elRank) {
            const rank = eduGameRanksConfig[passport.levelingProgress.rankKey] || { title: "Новичок", badge: "🪨", color: "#94a3b8" };
            elRank.textContent = `${rank.badge} ${rank.title}`;
            elRank.style.color = rank.color;
            elRank.style.filter = `drop-shadow(0 0 5px ${rank.color})`;
        }
        if (coins) {
            const balance = passport.user?.coinsBalance ?? passport.user?.CoinsBalance ?? 0;
            coins.textContent = Number(balance).toLocaleString("ru-RU");
            if (walletCoins) walletCoins.textContent = Number(balance).toLocaleString("ru-RU");
        }
    } catch (error) {
        console.error("[Core Sync] Ошибка запроса паспорта:", error);
    }
}