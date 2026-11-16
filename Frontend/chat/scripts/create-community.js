(function (Chat) {
    'use strict';
    const { apiFetch, el } = Chat;
    const { $ } = Chat.utils;
    const { closeMenu } = Chat;
    const { addRoomToSidebar } = Chat;

    function validateEnterpriseForm() {
        if (!el.commNameInput || !el.commDescInput || !el.submitCreateCommunityBtn) return;
        const nameOk = el.commNameInput.value.trim().length >= 3 && el.commNameInput.value.trim().length <= 30;
        const descOk = el.commDescInput.value.trim().length > 0;
        const ok = nameOk && descOk;
        el.submitCreateCommunityBtn.disabled = !ok;
        if (ok) {
            el.submitCreateCommunityBtn.style.background = 'var(--accent-gradient)';
            el.submitCreateCommunityBtn.style.color = 'white';
            el.submitCreateCommunityBtn.style.cursor = 'pointer';
        }
    }

    function resetCreateForm() {
        if (el.commNameInput) el.commNameInput.value = "";
        if (el.commDescInput) el.commDescInput.value = "";
        if (el.descCharCount) el.descCharCount.innerText = "0";
        if (el.sidebarPreviewName) el.sidebarPreviewName.innerText = "Новая Гильдия";
        if (el.communityPreviewImg) el.communityPreviewImg.src = "images/hamzaBro3.png";
        if (el.sidebarPreviewAvatar) el.sidebarPreviewAvatar.src = "images/hamzaBro3.png";
        if (el.commNameHint) {
            el.commNameHint.classList.remove('invalid');
            el.commNameHint.innerText = "Имя должно быть от 3 до 30 символов";
        }
        if (el.submitCreateCommunityBtn) el.submitCreateCommunityBtn.disabled = true;
    }

    function initCreateCommunity() {
        if (el.createCommunityModal && el.createCommunityCloseBtn && el.createCommunityBtn) {
            el.createCommunityBtn.addEventListener('click', () => {
                closeMenu();
                el.createCommunityModal.classList.add('active-modal');
                resetCreateForm();
            });
            el.createCommunityCloseBtn.addEventListener('click', () => el.createCommunityModal.classList.remove('active-modal'));
            el.createCommunityModal.addEventListener('click', (e) => {
                if (e.target === el.createCommunityModal) el.createCommunityModal.classList.remove('active-modal');
            });
        }

        if (el.avatarUploadZone && el.communityFileAvatar) {
            el.avatarUploadZone.addEventListener('click', () => el.communityFileAvatar.click());
            el.communityFileAvatar.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (!file) return;
                const reader = new FileReader();
                reader.onload = (ev) => {
                    if (el.communityPreviewImg) el.communityPreviewImg.src = ev.target.result;
                    if (el.sidebarPreviewAvatar) el.sidebarPreviewAvatar.src = ev.target.result;
                };
                reader.readAsDataURL(file);
            });
        }

        if (el.commNameInput && el.sidebarPreviewName && el.commNameHint) {
            el.commNameInput.addEventListener('input', (e) => {
                const val = e.target.value.trim();
                if (val === "") {
                    el.sidebarPreviewName.innerText = "Новая Гильдия";
                    el.sidebarPreviewName.style.color = "rgba(255,255,255,0.4)";
                    el.sidebarPreviewName.style.fontStyle = "italic";
                } else {
                    el.sidebarPreviewName.innerText = val;
                    el.sidebarPreviewName.style.color = "#ffffff";
                    el.sidebarPreviewName.style.fontStyle = "normal";
                }
                if (val.length > 0 && val.length < 3) {
                    el.commNameHint.classList.add('invalid');
                    el.commNameHint.innerText = "❌ Слишком коротко! Минимум 3 символа";
                } else {
                    el.commNameHint.classList.remove('invalid');
                    el.commNameHint.innerText = "Имя должно быть от 3 до 30 символов";
                }
                validateEnterpriseForm();
            });
        }

        if (el.commDescInput && el.descCharCount) {
            el.commDescInput.addEventListener('input', (e) => {
                el.descCharCount.innerText = e.target.value.length;
                validateEnterpriseForm();
            });
        }

        if (el.submitCreateCommunityBtn) {
            el.submitCreateCommunityBtn.addEventListener('click', async (e) => {
                e.preventDefault();

                const clanName = el.commNameInput ? el.commNameInput.value.trim() : "";
                const clanDesc = el.commDescInput ? el.commDescInput.value.trim() : "";
                const privacyRadio = document.querySelector('input[name="commPrivacy"]:checked');
                const privacyType = privacyRadio && privacyRadio.value === "private" ? 1 : 0;
                const limitSelect = $('commLimitSelect');
                const limitValue = limitSelect ? parseInt(limitSelect.value, 10) : 100;

                if (clanName.length < 3 || clanName.length > 30) {
                    alert("Ошибка: название должно быть от 3 до 30 символов!");
                    return;
                }

                el.submitCreateCommunityBtn.disabled = true;
                const originalText = el.submitCreateCommunityBtn.innerText;
                el.submitCreateCommunityBtn.innerText = "РЕГИСТРАЦИЯ СООБЩЕСТВА В БАЗУ ДАННЫХ";

                try {
                    const response = await apiFetch('/api/chat/CreateCommunity', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            CommunityName: clanName,
                            Description: clanDesc,
                            PrivacyType: privacyType,
                            MembersLimit: limitValue
                        })
                    });

                    const resultData = await response.json().catch(() => null);

                    if (response.ok) {
                        const newGuid = typeof resultData === 'string'
                            ? resultData
                            : (resultData?.value ?? resultData?.Value ?? resultData?.id ?? resultData?.communityId ?? resultData?.CommunityId);
                        if (typeof newGuid !== 'string' || !/^[0-9a-f-]{36}$/i.test(newGuid)) {
                            throw new Error("Сервер создал сообщество, но вернул некорректный идентификатор.");
                        }
                        const avatarSrc = el.communityPreviewImg ? el.communityPreviewImg.src : "images/hamzaBro3.png";

                        addRoomToSidebar(clanName, avatarSrc, true, newGuid, clanDesc);

                        const createdCommunityItem = Array.from(el.chatList?.querySelectorAll('.chat-item') ?? [])
                            .find(item => item.dataset.id === newGuid);
                        createdCommunityItem?.click();

                        el.createCommunityModal.classList.remove('active-modal');
                        resetCreateForm();
                        const defRadio = document.querySelector('input[name="commPrivacy"][value="public"]');
                        if (defRadio) defRadio.checked = true;
                    } else {
                        const errors = Array.isArray(resultData?.errors) ? resultData.errors.join("\n") : null;
                        const message = resultData?.error ?? errors ?? "Сервер отклонил создание сообщества.";
                        alert(`Ошибка создания сообщества:\n${message}`);
                    }
                } catch (err) {
                    console.error("Критический сбой сети:", err);
                    alert(err instanceof Error ? err.message : "Не удалось связаться с сервером EduGame.");
                } finally {
                    el.submitCreateCommunityBtn.disabled = false;
                    el.submitCreateCommunityBtn.innerText = originalText;
                }
            });
        }
    }

    Chat.initCreateCommunity = initCreateCommunity;
})(window.EduGameChat);