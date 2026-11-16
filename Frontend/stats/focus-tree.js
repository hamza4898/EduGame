(function (Stats) {
    'use strict';
    const { PATH_META, realSubSectionXp, realBranchNodes, escapeHtml } = Stats;

    async function renderFocusTree(secKey) {
        const overlay = document.getElementById(secKey + "-specific-tree");
        if (!overlay) return;

        const canvas = overlay.querySelector(".focus-canvas-content");
        if (!canvas) return;

        canvas.innerHTML = `<div style="padding:40px; color:var(--text-muted); text-align:center;">Синхронизация карты фокусов...</div>`;

        try {
            const response = await apiFetch(`/api/ratings/GetFocusTree?pathCode=${encodeURIComponent(secKey)}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const nodes = await response.json();
            canvas.innerHTML = "";

            if (!nodes || nodes.length === 0) {
                canvas.innerHTML = `<div style="padding:40px; color:var(--text-muted);">Фокусы не найдены в базе данных.</div>`;
                return;
            }

            const grouped = {};
            nodes.forEach(node => {
                const subName = node.subSectionName || node.SubSectionName;
                const userXp = node.currentUserXp !== undefined ? node.currentUserXp : node.CurrentUserXp;

                if (!grouped[subName]) {
                    grouped[subName] = { xp: userXp || 0, items: [] };
                }
                grouped[subName].items.push(node);
            });

            const pathMeta = PATH_META[secKey] || { title: secKey, color: "#06b6d4", icon: "fa-laptop-code" };
            let totalXpSum = 0;
            Object.keys(grouped).forEach(k => totalXpSum += grouped[k].xp);

            const sectionXpChip = document.getElementById(`secXp-${secKey}`);
            if (sectionXpChip) sectionXpChip.innerText = totalXpSum;

            let html = `
                <div class="focus-root-center">
                    <div class="focus-node completed root-node" style="border-color: ${pathMeta.color} !important; width: 190px;">
                        <i class="fa-solid ${pathMeta.icon}" style="color: ${pathMeta.color}; font-size: 22px;"></i>
                        <h5>${pathMeta.title}</h5>
                        <span class="condition">Центральный штаб управления</span>
                    </div>
                </div>
                <div class="canvas-columns-wrapper" style="gap: 50px; display: flex; justify-content: center; margin-top: 30px; align-items: flex-start;">`;

            Object.keys(grouped).forEach(subName => {
                const branch = grouped[subName];
                const branchKey = `${secKey}_${subName}`;

                realSubSectionXp[branchKey] = branch.xp;
                realBranchNodes[branchKey] = {
                    sectionTitle: pathMeta.title,
                    branchName: subName,
                    nodes: branch.items.map(node => ({
                        name: node.name || node.Name,
                        icon: node.icon || node.Icon,
                        threshold: node.xpThreshold !== undefined ? node.xpThreshold : node.XpThreshold,
                        reward: node.rewardText || node.RewardText
                    }))
                };

                const activeGoal = branch.items.find(n => {
                    const thresh = n.xpThreshold !== undefined ? n.xpThreshold : n.XpThreshold;
                    return branch.xp < thresh;
                });

                let headerProgressHtml = "";
                if (activeGoal) {
                    const goalThresh = activeGoal.xpThreshold !== undefined ? activeGoal.xpThreshold : activeGoal.XpThreshold;
                    const goalName = activeGoal.name || activeGoal.Name;
                    const remainingXp = goalThresh - branch.xp;
                    const currentIndex = branch.items.indexOf(activeGoal);
                    const prevNode = branch.items[currentIndex - 1];
                    const prevThreshold = prevNode ? (prevNode.xpThreshold !== undefined ? prevNode.xpThreshold : prevNode.XpThreshold) : 0;
                    const pct = Math.min(100, Math.max(4, ((branch.xp - prevThreshold) / (goalThresh - prevThreshold)) * 100));

                    headerProgressHtml = `
                        <div class="branch-xp-row" style="display:flex; justify-content:space-between; font-size:12px; margin-bottom:8px; font-family: monospace;">
                            <span style="color:#f59e0b; font-weight:700;"><i class="fa-solid fa-fire"></i> ${branch.xp} XP</span>
                            <span style="color:var(--text-muted);">До «${escapeHtml(goalName)}»: <strong style="color:#3b82f6;">ещё ${remainingXp} XP</strong></span>
                        </div>
                        <div class="branch-xp-track" style="width:100%; height:6px; background:rgba(255,255,255,0.03); border-radius:3px; margin-bottom:24px; overflow:hidden; border: 1px solid rgba(255,255,255,0.02);">
                            <div class="branch-xp-fill" style="width:${pct}%; height:100%; background:linear-gradient(90deg, #10b981, #059669); box-shadow:0 0 8px rgba(16,185,129,0.5); border-radius:3px; transition: width 0.3s ease;"></div>
                        </div>`;
                } else {
                    headerProgressHtml = `
                        <div class="branch-xp-row" style="display:flex; justify-content:space-between; font-size:12px; margin-bottom:8px; color:#10b981; font-weight:bold;">
                            <span><i class="fa-solid fa-fire"></i> ${branch.xp} XP</span>
                            <span>Ветка закрыта на 100%! 🏆</span>
                        </div>
                        <div class="branch-xp-track" style="width:100%; height:6px; background:rgba(255,255,255,0.03); border-radius:3px; margin-bottom:24px;"><div style="width:100%; height:100%; background:#10b981; box-shadow:0 0 8px #10b981;"></div></div>`;
                }

                html += `
                <div class="canvas-column focus-branch-column" style="background: rgba(255,255,255,0.005); border: 1px solid rgba(255,255,255,0.02); padding: 24px; border-radius: 12px; min-width: 300px;">
                    <div class="column-title" style="color: ${pathMeta.color}; font-weight: 700; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; margin-bottom:12px; display:flex; align-items:center; gap:8px;">
                        <i class="fa-solid fa-terminal" style="font-size:12px;"></i> ${escapeHtml(subName)}
                    </div>
                    ${headerProgressHtml}`;

                let isPreviousCompleted = true;
                branch.items.forEach((node, idx) => {
                    const nThreshold = node.xpThreshold !== undefined ? node.xpThreshold : node.XpThreshold;
                    const nName = node.name || node.Name;
                    const nIcon = node.icon || node.Icon;
                    const nReward = node.rewardText || node.RewardText;

                    let state = "locked";
                    let conditionText = `🎁 ${escapeHtml(nReward)}`;

                    if (branch.xp >= nThreshold) {
                        state = "completed";
                        conditionText = `✓ ${escapeHtml(nReward)}`;
                    } else if (isPreviousCompleted) {
                        state = "active";
                        conditionText = `⚡ ${branch.xp} / ${nThreshold} XP`;
                        isPreviousCompleted = false;
                    }

                    if (idx > 0) {
                        html += `<div class="focus-connector ${state === 'completed' ? 'done' : ''}" style="width: 2px; height: 35px; background: ${state === 'completed' ? '#10b981' : 'rgba(255,255,255,0.03)'}; margin: 0 auto;"></div>`;
                    }

                    let borderStroke = "rgba(255,255,255,0.03)";
                    if (state === "completed") borderStroke = "#10b981";
                    else if (state === "active") borderStroke = "#3b82f6";

                    html += `
                        <div class="focus-node ${state}" style="border: 1px solid ${borderStroke}; box-shadow: ${state === 'active' ? '0 0 12px rgba(59,130,246,0.15)' : 'none'}; background: rgba(255,255,255,0.01); padding: 16px; margin: 0; border-radius: 8px; text-align:center; transition:0.3s;">
                            <i class="fa-solid ${nIcon}" style="font-size:16px; color: ${state === 'completed' ? '#10b981' : state === 'active' ? '#3b82f6' : 'var(--text-muted)'};"></i>
                            <h5 style="margin: 8px 0 4px 0; font-size: 14px; font-weight:600; color: white;">${escapeHtml(nName)}</h5>
                            <span class="node-xp-pill" style="font-size: 11px; background:rgba(255,255,255,0.03); padding:2px 8px; border-radius:10px; color: var(--text-muted);">порог ${nThreshold} XP</span>
                            <span class="condition" style="font-size: 12px; display: block; margin-top: 8px; font-weight:500; color:${state === 'completed' ? '#10b981' : state === 'active' ? '#3b82f6' : 'var(--text-muted)'};">${conditionText}</span>
                        </div>`;
                });
                html += `</div>`;
            });

            html += `</div>`;
            canvas.innerHTML = html;

        } catch (e) {
            console.error("Крах рендеринга дерева:", e);
            canvas.innerHTML = `<div style="padding:40px; color:var(--text-red); text-align:center;">Не удалось прогрузить панораму дерева фокусов. Проверьте API.</div>`;
        }
    }

    function renderAllFocusTrees() {
        Object.keys(PATH_META).forEach(renderFocusTree);
    }

    function initFocusMapEngine() {
        document.querySelectorAll(".specific-tree-overlay").forEach(overlay => {
            const scrollContainer = overlay.querySelector(".focus-canvas-scroll");
            const canvas = overlay.querySelector(".focus-canvas-content");
            if (!scrollContainer || !canvas) return;

            let currentScale = 1;
            scrollContainer.addEventListener("wheel", (e) => {
                e.preventDefault();
                const delta = e.deltaY < 0 ? 0.05 : -0.05;
                currentScale = Math.min(Math.max(0.5, currentScale + delta), 2);
                canvas.style.transform = `scale(${currentScale})`;
            }, { passive: false });

            let isDown = false, startX = 0, startY = 0, startScrollLeft = 0, startScrollTop = 0;
            canvas.addEventListener("mousedown", (e) => {
                isDown = true;
                startX = e.pageX - scrollContainer.offsetLeft;
                startY = e.pageY - scrollContainer.offsetTop;
                startScrollLeft = scrollContainer.scrollLeft;
                startScrollTop = scrollContainer.scrollTop;
            });
            canvas.addEventListener("mouseleave", () => { isDown = false; });
            canvas.addEventListener("mouseup", () => { isDown = false; });
            canvas.addEventListener("mousemove", (e) => {
                if (!isDown) return;
                e.preventDefault();
                const x = e.pageX - scrollContainer.offsetLeft;
                const y = e.pageY - scrollContainer.offsetTop;
                scrollContainer.scrollLeft = startScrollLeft - (x - startX) * 1.5;
                scrollContainer.scrollTop = startScrollTop - (y - startY) * 1.5;
            });
        });
    }

    Object.assign(Stats, { renderFocusTree, renderAllFocusTrees, initFocusMapEngine });
})(window.EduGameStats);