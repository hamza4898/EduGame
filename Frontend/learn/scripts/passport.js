(function (Learn) {
    'use strict';
    const { state } = Learn;

    async function loadPassport() {
        try {
            const response = await apiFetch("/api/user/GetMyBusinessPassport");
            if (response.ok) {
                const passport = await response.json();
                const passportUser = passport.user ?? passport.User ?? {};
                state.currentUserId = passportUser.externalId ?? passportUser.ExternalId
                    ?? passportUser.id ?? passportUser.Id ?? "";
                const nick = document.getElementById("userNickname");
                const lvl = document.getElementById("userLevel");
                const levelingProgress = passport.levelingProgress ?? passport.LevelingProgress;
                if (nick) nick.textContent = passport.userName ?? passport.UserName ?? "";
                if (lvl && levelingProgress) lvl.textContent = `Lvl ${levelingProgress.level ?? levelingProgress.Level ?? 0}`;
                const avatarUrl = passportUser.avatarUrl ?? passportUser.AvatarUrl;
                if (avatarUrl) {
                    document.querySelectorAll(".saas-avatar-mini").forEach(avatar => {
                        avatar.style.backgroundImage = `url("${avatarUrl.replaceAll('"', '%22')}")`;
                    });
                }
                const coinsBalance = passportUser.coinsBalance ?? passportUser.CoinsBalance;
                if (coinsBalance !== undefined) {
                    const coinsEl = document.getElementById("hudUserCoinsDisplay");
                    if (coinsEl) coinsEl.textContent = coinsBalance;
                }
            }
        } catch (e) { console.warn("[Learn] паспорт недоступен:", e); }
    }

    Learn.loadPassport = loadPassport;
})(window.EduGameLearn);