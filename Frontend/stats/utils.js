(function (Stats) {
    'use strict';
    const { escapeHtml } = Stats;

    function createLeaderboardRowHtml(index, avatar, name, metric, isCurrentUser, isTopThreeGlobal, userId = null) {
        const medalClass = index === 0 ? "bg-gold" : index === 1 ? "bg-silver" : index === 2 ? "bg-bronze" : "";
        const currentUserClass = isCurrentUser ? "current-user" : "";
        const topThreeClass = (isTopThreeGlobal && index < 3) ? "top-three" : "";
        const profileUrl = userId ? `Profile.html?id=${encodeURIComponent(userId)}` : null;

        return `
            <div class="leader-row-item animate__animated animate__fadeInUp ${currentUserClass} ${topThreeClass}" style="animation-delay: ${index * 0.04}s">
                <span class="leader-place ${medalClass}">${index + 1}</span>
                ${profileUrl ? `<a class="leader-avatar-box" href="${profileUrl}" aria-label="Открыть профиль ${escapeHtml(name)}">` : '<div class="leader-avatar-box">'}
                    <img src="${escapeHtml(avatar || 'images/hamzaBro3.png')}" alt="${escapeHtml(name)}" onerror="this.src='images/hamzaBro3.png'">
                ${profileUrl ? '</a>' : '</div>'}
                ${profileUrl ? `<a class="leader-name" href="${profileUrl}" style="color:inherit;text-decoration:none;">${escapeHtml(name)}</a>` : `<span class="leader-name">${escapeHtml(name)}</span>`}
                <span class="leader-xp" style="color: #10b981; font-weight: bold;">${metric}</span>
            </div>`;
    }

    Stats.createLeaderboardRowHtml = createLeaderboardRowHtml;
})(window.EduGameStats);