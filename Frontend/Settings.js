document.addEventListener("DOMContentLoaded", async () => {

    const API = {
        passport:      "/api/user/GetMySecurityPassport",
        changeEmail:   "/api/auth/ChangeEmail",
        linkGoogle:    "/api/auth/LinkGoogleToCurrentAccount",
        unlinkGoogle:  "/api/auth/UnlinkGoogleAccount",
        deleteAccount: "/api/auth/DeleteAccount",
        logout:        "/api/auth/Logout",
        enable2fa:     "/api/auth/EnableTwoFactorSecurity",
        verify2fa:     "/api/auth/VerifyTwoFactorSecurity",
        disable2fa:    "/api/auth/DisableTwoFactorSecurity"
    };

    const $ = (id) => document.getElementById(id);

    async function parseAuthResponse(response) {
        let body = null;
        try { body = await response.json(); } catch (e) { }
        const bodyFailed = body && (body.isSuccess === false ||
            ["Error", "Invalid", "NotFound", "Unauthorized", "Forbidden"].includes(body.status));
        return { ok: response.ok && !bodyFailed, body };
    }

    function decodeJwtPayload(token) {
        try {
            const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            const json = decodeURIComponent(atob(base64).split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
            return JSON.parse(json);
        } catch (e) { return null; }
    }

    const emailSecureModal   = $("emailSecureModal");
    const triggerEmailBtn    = $("triggerEmailModalBtn");
    const submitEmailBtn     = $("submitSecureEmailBtn");
    const closeEmailBtn      = $("closeSecureEmailBtn");
    const emailInput         = $("settingsEmailInput");
    const emailStatusBadge   = $("emailStatusBadge");

    const googleLinkBtn      = $("googleLinkBtn");
    const googleLinkStatus   = $("googleLinkStatus");

    const action2faBtn       = $("action2faBtn");
    const status2faDisplay   = $("status2faDisplay");
    const qrSecureModal      = $("qrSecureModal");
    const qrCodeImg          = $("qrCodeImg");
    const qrSecretKeyText    = $("qrSecretKeyText");
    const qrCodeInput        = $("qrVerificationCodeInput");
    const submit2faBtn       = $("submit2faVerificationBtn");
    const closeQrBtn         = $("btn-close-tree-trigger");

    const actionLogoutBtn    = $("actionLogoutBtn");
    const actionDeleteBtn    = $("actionDeleteBtn");

    let googleLinked = false;
    let googleEmail  = null;
    let hasPassword  = true;
    let twoFactorOn  = false;
    let qrMode       = "enable";


    async function loadPassport() {
        try {
            const r = await window.apiFetch(API.passport);
            if (!r.ok) return;
            const p = await r.json();

            if (emailInput) emailInput.value = p.email || "";
            window.currentEmail = p.email || "";

            googleEmail  = p.googleEmail ?? null;
            googleLinked = !!(p.hasGoogleLogin || p.googleEmail);
            hasPassword  = p.hasPassword ?? true;
            twoFactorOn  = p.twoFactorEnabled ?? p.isTwoFactorEnabled ?? false;

            if (emailStatusBadge) {
                if (p.emailConfirmed) {
                    emailStatusBadge.textContent = "Подтверждён";
                    emailStatusBadge.className = "status-badge status-enabled";
                } else {
                    emailStatusBadge.textContent = "Ожидает подтверждения";
                    emailStatusBadge.className = "status-badge status-disabled";
                }
            }
        } catch (e) {
            console.error("[Settings] Ошибка паспорта безопасности:", e);
        }
    }

    function renderGoogleState() {
        if (!googleLinkBtn || !googleLinkStatus) return;
        if (googleLinked) {
            googleLinkStatus.textContent = googleEmail ? `Привязан: ${googleEmail}` : "Привязан";
            googleLinkStatus.className = "status-badge status-enabled";
            googleLinkBtn.innerHTML = '<i class="fa-solid fa-link-slash"></i> Отвязать Google аккаунт';
            googleLinkBtn.classList.add("unlink-mode");
        } else {
            googleLinkStatus.textContent = "Не привязан";
            googleLinkStatus.className = "status-badge status-disabled";
            googleLinkBtn.innerHTML = '<i class="fa-brands fa-google"></i> Привязать Google аккаунт';
            googleLinkBtn.classList.remove("unlink-mode");
        }
    }

    function render2faState() {
        if (!action2faBtn || !status2faDisplay) return;
        if (twoFactorOn) {
            status2faDisplay.textContent = "Активна";
            status2faDisplay.className = "status-badge status-enabled";
            action2faBtn.textContent = "Отключить 2FA защиту";
            action2faBtn.classList.add("active-mode");
        } else {
            status2faDisplay.textContent = "Отключена";
            status2faDisplay.className = "status-badge status-disabled";
            action2faBtn.textContent = "Включить 2FA защиту";
            action2faBtn.classList.remove("active-mode");
        }
    }

    if (triggerEmailBtn && emailSecureModal) {
        triggerEmailBtn.addEventListener("click", () => emailSecureModal.classList.add("open"));
    }
    if (emailSecureModal) {
        emailSecureModal.addEventListener("click", (e) => {
            if (e.target === emailSecureModal) emailSecureModal.classList.remove("open");
        });
    }
    if (closeEmailBtn) {
        closeEmailBtn.addEventListener("click", () => emailSecureModal.classList.remove("open"));
    }

    if (submitEmailBtn) {
        submitEmailBtn.addEventListener("click", async () => {
            const currentPassword = ($("secureCurrentPassword") || {}).value || "";
            const newEmail = (($("secureNewEmail") || {}).value || "").trim();

            if (!currentPassword) { alert("Введите текущий пароль от аккаунта."); return; }
            if (!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(newEmail)) { alert("Новый email выглядит некорректно."); return; }
            if (newEmail.toLowerCase() === (window.currentEmail || "").toLowerCase()) { alert("Новый адрес совпадает с текущим."); return; }

            submitEmailBtn.disabled = true;
            submitEmailBtn.innerText = "Отправка на сервер...";
            try {
                const r = await window.apiFetch(API.changeEmail, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ currentPassword, newEmail })   // → ChangeEmailDto
                });
                const { ok, body } = await parseAuthResponse(r);
                if (ok) {
                    alert("✅ Токен смены создан! Письмо с подтверждением отправлено на " + newEmail);
                    if (emailInput) emailInput.value = newEmail + " (ожидает подтверждения)";
                    if (emailStatusBadge) {
                        emailStatusBadge.textContent = "Ожидает подтверждения";
                        emailStatusBadge.className = "status-badge status-disabled";
                    }
                    emailSecureModal.classList.remove("open");
                    if ($("secureCurrentPassword")) $("secureCurrentPassword").value = "";
                    if ($("secureNewEmail")) $("secureNewEmail").value = "";
                } else {
                    alert("Ошибка смены email: " + ((body && (body.error || body.message)) || r.status) + ". Возможно, неверный текущий пароль.");
                }
            } catch (e) {
                alert("Не удалось связаться с сервером.");
            } finally {
                submitEmailBtn.disabled = false;
                submitEmailBtn.innerText = "Подтвердить на сервере";
            }
        });
    }

    if (googleLinkBtn) {
        googleLinkBtn.removeAttribute("onclick");

        googleLinkBtn.addEventListener("click", async () => {
            if (!googleLinked) {
                if (typeof getGoogleIdToken !== "function") {
                    alert("google-auth.js не загрузился. Обнови страницу.");
                    return;
                }
                let idToken;
                try { idToken = await getGoogleIdToken(); }
                catch (e) { alert(e.message || "Окно Google не открылось."); return; }

                try {
                    const r = await window.apiFetch(API.linkGoogle, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ idToken })
                    });
                    const { ok, body } = await parseAuthResponse(r);
                    if (ok) {
                        const payload = decodeJwtPayload(idToken);
                        googleLinked = true;
                        googleEmail = payload?.email || window.currentEmail || null;
                        renderGoogleState();
                        alert("✅ Google аккаунт привязан!");
                    } else {
                        alert("Ошибка привязки: " + ((body && (body.error || body.message)) || r.status));
                    }
                } catch (e) {
                    alert("Не удалось связаться с сервером.");
                }
                return;
            }

            if (!hasPassword) {
                alert("Сори бро 🙃 У аккаунта нет пароля — Google это ЕДИНСТВЕННЫЙ способ входа.\nСначала задай пароль, потом отвязывай.");
                return;
            }
            if (!confirm("Отвязать Google аккаунт?\nДальше будешь входить через email + пароль.")) return;

            try {
                const r = await window.apiFetch(API.unlinkGoogle, { method: "POST" });   // без тела
                const { ok, body } = await parseAuthResponse(r);
                if (ok) {
                    googleLinked = false;
                    googleEmail = null;
                    renderGoogleState();
                    alert("✅ Google отвязан. Теперь вход через email и пароль.");
                } else {
                    alert("Ошибка отвязки: " + ((body && (body.error || body.message)) || r.status));
                }
            } catch (e) {
                alert("Не удалось связаться с сервером.");
            }
        });
    }

    function setQrModalMode(mode) {
        const show = mode === "enable";

        if (qrCodeImg) {
            const qrZone = qrCodeImg.closest(".qr-code-zone-container") || qrCodeImg.parentElement;
            if (qrZone) qrZone.style.display = show ? "" : "none";
            const qrLabel = qrZone ? qrZone.previousElementSibling : null;
            if (qrLabel && qrLabel.classList.contains("step-label")) qrLabel.style.display = show ? "" : "none";
        }

        if (qrSecretKeyText) {
            qrSecretKeyText.style.display = show ? "" : "none";
            const secretLabel = qrSecretKeyText.previousElementSibling;
            if (secretLabel && secretLabel.classList.contains("step-label")) secretLabel.style.display = show ? "" : "none";
        }
    }

    function setQrModalTexts(mode) {
        const title   = document.querySelector(".cyber-alert-title");
        const desc    = document.querySelector(".cyber-alert-desc");
        const bullets = document.querySelector(".security-bullet-card");

        if (mode === "disable") {
            if (title) title.innerText = "⚠️ ДЕАКТИВАЦИЯ КИБЕР-ЩИТА";
            if (desc)  desc.innerText  = "Для удаления привязки TOTP введите текущий 6-значный код из приложения-аутентификатора.";
            if (bullets) bullets.style.display = "none";
            if (submit2faBtn) {
                submit2faBtn.innerHTML = '<i class="fa-solid fa-shield-slash"></i> ОТКЛЮЧИТЬ 2FA';
                submit2faBtn.style.background = "linear-gradient(135deg, #e63946, #c9184a)";
            }
        } else {
            if (title) title.innerText = "ЖЕЛАЕТЕ УСИЛИТЬ БЕЗОПАСНОСТЬ ЯДРА?";
            if (desc)  desc.innerText  = "Отсканируйте QR-код и подтвердите привязку кодом. В текущей версии код пока не запрашивается при обычном входе.";
            if (bullets) bullets.style.display = "";
            if (submit2faBtn) {
                submit2faBtn.innerHTML = '<i class="fa-solid fa-unlock-keyhole"></i> АКТИВИРОВАТЬ ЗАЩИТУ АККАУНТА';
                submit2faBtn.style.background = "";
            }
        }
    }

    window.initializeTwoFactor = async function () {
        if (twoFactorOn) {
            qrMode = "disable";
            setQrModalMode("disable");
            setQrModalTexts("disable");
            if (qrCodeInput) qrCodeInput.value = "";
            if (qrSecureModal) qrSecureModal.classList.add("open");
            return;
        }

        qrMode = "enable";
        try {
            const r = await window.apiFetch(API.enable2fa, { method: "POST" });
            const { ok, body } = await parseAuthResponse(r);
            if (!ok) {
                alert("Не удалось начать включение 2FA: " + ((body && (body.error || body.message)) || r.status));
                return;
            }

            const secretKey   = body?.unformattedKey || body?.UnformattedKey || "";
            const base64Image = body?.base64Image    || body?.Base64Image    || "";

            setQrModalMode("enable");
            setQrModalTexts("enable");
            if (qrCodeImg) qrCodeImg.src = base64Image;
            if (qrSecretKeyText) qrSecretKeyText.value = secretKey || "ключ не получен";
            if (qrCodeInput) qrCodeInput.value = "";
            if (qrSecureModal) qrSecureModal.classList.add("open");
        } catch (e) {
            console.error("[Settings] 2FA enable error:", e);
            alert("Не удалось связаться с сервером.");
        }
    };

    if (submit2faBtn) {
        submit2faBtn.addEventListener("click", async () => {
            const code = (qrCodeInput?.value || "").replace(/\s/g, "");
            if (!/^\d{6}$/.test(code)) { alert("Введите 6-значный код из Authenticator."); return; }

            const url = qrMode === "enable" ? API.verify2fa : API.disable2fa;

            try {
                const r = await window.apiFetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ code })
                });
                const { ok, body } = await parseAuthResponse(r);

                if (ok) {
                    twoFactorOn = (qrMode === "enable");
                    render2faState();
                    if (qrSecureModal) qrSecureModal.classList.remove("open");
                    alert(twoFactorOn ? "✅ 2FA активирована! Кибер-щит развёрнут." : "✅ 2FA отключена.");
                } else {
                    alert("Код не принят: " + ((body && (body.error || body.message)) || "проверь Authenticator"));
                }
            } catch (e) {
                alert("Не удалось связаться с сервером.");
            }
        });
    }

    if (closeQrBtn && qrSecureModal) {
        closeQrBtn.addEventListener("click", () => qrSecureModal.classList.remove("open"));
    }

    if (actionLogoutBtn) {
        actionLogoutBtn.addEventListener("click", async () => {
            if (!confirm("Выйти с аккаунта EduGame?")) return;
            try {
                const response = await window.apiFetch(API.logout, { method: "POST" });
                const { ok, body } = await parseAuthResponse(response);
                if (!ok) {
                    alert("Не удалось завершить сессию на сервере: " +
                        ((body && (body.error || body.message)) || response.status));
                    return;
                }

                localStorage.clear();
                window.location.href = "Login.html";
            } catch (error) {
                console.error("[Settings] Ошибка выхода:", error);
                alert("Не удалось связаться с сервером. Сессия не была завершена.");
            }
        });
    }

    function ensureDeleteModal() {
        let modal = $("deleteSecureModal");
        if (modal) return modal;

        modal = document.createElement("div");
        modal.className = "secure-modal-overlay";
        modal.id = "deleteSecureModal";
        modal.innerHTML = `
            <div class="secure-modal-content animate__animated animate__zoomIn animate__fast">
                <div class="secure-modal-icon"><i class="fa-solid fa-triangle-exclamation"></i></div>
                <h3>Подтверждение удаления</h3>
                <p>Чтобы уничтожить аккаунт, подтверди личность: введи пароль или подтверди через Google.</p>
                <div class="secure-modal-form">
                    <div class="secure-input-wrapper" id="deletePasswordWrapper">
                        <label><i class="fa-solid fa-lock"></i> Текущий пароль</label>
                        <input type="password" id="deletePasswordInput" placeholder="••••••••••••">
                    </div>
                </div>
                <div class="secure-modal-actions">
                    <button class="secure-btn confirm" id="submitDeleteBtn" style="background:linear-gradient(135deg,#e63946,#c9184a);">Удалить навсегда</button>
                    <button class="secure-btn cancel" id="closeDeleteBtn">Отмена</button>
                </div>
                <p style="text-align:center;margin-top:10px;">
                    <a href="#" id="deleteViaGoogleLink" style="color:#a855f7;font-size:12px;">Подтвердить через Google вместо пароля</a>
                </p>
            </div>`;
        document.body.appendChild(modal);
        return modal;
    }

    if (actionDeleteBtn) {
        actionDeleteBtn.addEventListener("click", () => {
            if (!confirm("🚨 Удалить аккаунт EduGame безвозвратно?")) return;
            const modal = ensureDeleteModal();
            const pwWrapper = $("deletePasswordWrapper");
            const gLink = $("deleteViaGoogleLink");
            if (pwWrapper) pwWrapper.style.display = hasPassword ? "" : "none";
            if (gLink) gLink.style.display = hasPassword ? "none" : "";
            modal.classList.add("open");

            if (!modal.dataset.wired) {
                modal.dataset.wired = "1";

                modal.addEventListener("click", (e) => {
                    if (e.target === modal) modal.classList.remove("open");
                });
                $("closeDeleteBtn").addEventListener("click", () => modal.classList.remove("open"));

                // удаление с паролем
                $("submitDeleteBtn").addEventListener("click", async () => {
                    const password = ($("deletePasswordInput").value || "").trim();
                    if (hasPassword && !password) { alert("Введите пароль для подтверждения."); return; }

                    try {
                        const r = await window.apiFetch(API.deleteAccount, {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify({ password: password || null, googleIdToken: null })
                        });
                        const { ok, body } = await parseAuthResponse(r);
                        if (ok) {
                            localStorage.clear();
                            alert("Аккаунт удалён. До встречи!");
                            window.location.href = "Login.html";
                        } else {
                            alert("Ошибка удаления: " + ((body && (body.error || body.message)) || r.status));
                        }
                    } catch (e) {
                        alert("Не удалось связаться с сервером.");
                    }
                });

                $("deleteViaGoogleLink").addEventListener("click", async (e) => {
                    e.preventDefault();
                    if (typeof getGoogleIdToken !== "function") { alert("google-auth.js не загрузился."); return; }
                    let idToken;
                    try { idToken = await getGoogleIdToken(); }
                    catch (err) { alert("Окно Google не открылось."); return; }

                    try {
                        const r = await window.apiFetch(API.deleteAccount, {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify({ password: null, googleIdToken: idToken })
                        });
                        const { ok, body } = await parseAuthResponse(r);
                        if (ok) {
                            localStorage.clear();
                            alert("Аккаунт удалён. До встречи!");
                            window.location.href = "Login.html";
                        } else {
                            alert("Ошибка удаления: " + ((body && (body.error || body.message)) || r.status));
                        }
                    } catch (err) {
                        alert("Не удалось связаться с сервером.");
                    }
                });
            }
        });
    }

    await loadPassport();
    renderGoogleState();
    render2faState();
});