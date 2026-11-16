const GOOGLE_CLIENT_ID = "19946886621-hfvjnrv80pi4qm9811sj3s5dfacdvt5a.apps.googleusercontent.com";

let _googleResolve = null;

function ensureGoogleReady() {
    return new Promise((resolve) => {
        const wait = () => {
            if (window.google && window.google.accounts) {
                google.accounts.id.initialize({
                    client_id: GOOGLE_CLIENT_ID,
                    callback: (resp) => {
                        if (_googleResolve) { const r = _googleResolve; _googleResolve = null; r(resp.credential); }
                    },
                });
                resolve();
            } else setTimeout(wait, 150);
        };
        wait();
    });
}

async function getGoogleIdToken() {
    return new Promise((resolve, reject) => {
        _googleResolve = resolve;
        
        if (!window.google || !window.google.accounts) {
            reject(new Error("Библиотека Google еще загружается, подождите секунду и кликните еще раз"));
            return;
        }

        google.accounts.id.prompt((n) => {
            if (n.isNotDisplayed() || n.isDismissedMoment()) {
                if (_googleResolve) { 
                    _googleResolve = null; 
                    reject(new Error("Окно Google заблокировано браузером — кликни ещё раз")); 
                }
            }
        });
    });
}

function decodeJwtPayload(token) {
    try {
        const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
        const json = decodeURIComponent(
            atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
        );
        return JSON.parse(json);
    } catch (e) { return null; }
}

function applySession(result) {
    if (result.accessToken) localStorage.setItem("accessToken", result.accessToken);
    if (result.refreshToken) localStorage.setItem("refreshToken", result.refreshToken);
}

async function googleAuthUniversal() {
    let idToken;
    try { idToken = await getGoogleIdToken(); }
    catch (e) { alert(e.message); return; }

    const googlePayload = decodeJwtPayload(idToken);

    try {
        const response = await fetch("/api/login/GoogleLogin", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idToken }),
            credentials: "include"
        });
        const result = await response.json();
        if (!response.ok) { alert(result.error || "Ошибка Google входа"); return; }

        applySession(result);

        if (result === false) {
            openGoogleCompleteModal(googlePayload);
        } else {
            window.location.href = "Main.html";
        }
    } catch (e) {
        alert("Не удалось связаться с сервером");
    }
}

async function googleLinkCurrentAccount() {
    let idToken;
    try { idToken = await getGoogleIdToken(); }
    catch (e) { alert(e.message); return false; }

    try {
        const response = await window.apiFetch("/api/auth/LinkGoogle", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idToken })
        });
        if (response.ok) { alert("✅ Google аккаунт привязан!"); return true; }
        const err = await response.json().catch(() => null);
        alert("Ошибка привязки: " + ((err && err.error) || response.status));
        return false;
    } catch (e) { alert("Не удалось связаться с сервером"); return false; }
}

function openGoogleCompleteModal(g) {
    if (document.getElementById("googleCompleteModal")) return;
    g = g || {};

    const wrap = document.createElement("div");
    wrap.id = "googleCompleteModal";
    wrap.innerHTML = `
    <style>
        #googleCompleteModal { position: fixed; inset: 0; z-index: 3000; background: rgba(4,6,12,.94);
            backdrop-filter: blur(20px); -webkit-backdrop-filter: blur(20px);
            display: flex; align-items: center; justify-content: center; font-family: sans-serif; }
        #googleCompleteModal .gc-card { width: 520px; max-width: 92vw; max-height: 92vh; overflow-y: auto;
            background: rgba(15,20,36,.96); border: 1px solid rgba(255,77,109,.35); border-radius: 20px;
            padding: 2.2rem; position: relative; box-shadow: 0 25px 60px rgba(0,0,0,.6); }
        #googleCompleteModal .gc-card::before { content:''; position:absolute; top:0; left:0; right:0; height:3px;
            background: linear-gradient(90deg, transparent, #ff4d6d, #c9184a, transparent); }
        #googleCompleteModal h3 { color:#fff; font-size:24px; font-weight:900; margin-bottom:6px; text-align:center; }
        #googleCompleteModal .gc-sub { color:#94a3b8; font-size:13px; text-align:center; margin-bottom:22px; }
        #googleCompleteModal .gc-roles { display:grid; grid-template-columns:1fr 1fr; gap:14px; margin-bottom:20px; }
        #googleCompleteModal .gc-role-card { padding:20px 12px; border-radius:16px; text-align:center; cursor:pointer;
            background:rgba(255,255,255,.03); border:1px solid rgba(255,255,255,.1);
            display:flex; flex-direction:column; gap:6px; align-items:center; transition:all .2s; }
        #googleCompleteModal .gc-role-card:hover { transform:translateY(-3px); }
        #googleCompleteModal .gc-role-icon { font-size:28px; }
        #googleCompleteModal .gc-role-card strong { color:#fff; font-size:16px; }
        #googleCompleteModal .gc-role-desc { font-size:10.5px; color:#64748b; }
        #googleCompleteModal .gc-role-card.active-student { border-color:#10b981; background:rgba(16,185,129,.08);
            box-shadow:0 0 18px rgba(16,185,129,.15); }
        #googleCompleteModal .gc-role-card.active-teacher { border-color:#f59e0b; background:rgba(245,158,11,.08);
            box-shadow:0 0 18px rgba(245,158,11,.15); }
        #googleCompleteModal .gc-field { margin-bottom:14px; }
        #googleCompleteModal label { display:block; font-size:11px; font-weight:800; color:#64748b;
            text-transform:uppercase; letter-spacing:1px; margin-bottom:6px; }
        #googleCompleteModal input { width:100%; height:50px; border-radius:12px; background:rgba(0,0,0,.35);
            border:1px solid rgba(255,255,255,.1); color:#fff; padding:0 14px; font-size:14px; outline:none;
            font-family:inherit; transition:border-color .2s, box-shadow .2s; }
        #googleCompleteModal input:focus { border-color:#ff4d6d; box-shadow:0 0 12px rgba(255,77,109,.25); }
        #googleCompleteModal input[type="date"] { color-scheme:dark; }
        #googleCompleteModal .gc-row { display:flex; gap:12px; }
        #googleCompleteModal .gc-row .gc-field { flex:1; }
        #googleCompleteModal .gc-submit { width:100%; margin-top:6px; height:54px; border:none; border-radius:100px;
            background:linear-gradient(135deg,#ff4d6d,#c9184a); color:#fff; font-size:15px; font-weight:800;
            cursor:pointer; box-shadow:0 10px 26px rgba(201,24,74,.4); transition:all .25s; }
        #googleCompleteModal .gc-submit:hover { transform:translateY(-2px); filter:brightness(1.08); }
    </style>

    <div class="gc-card">
        <h3>Добро пожаловать в EduGame! 🎮</h3>
        <p class="gc-sub">Выбери роль — Google уже подсказал имя и фамилию, проверь их и заполни остальное.</p>

        <div class="gc-roles">
            <div class="gc-role-card active-student" id="gcRoleStudent">
                <span class="gc-role-icon">👨‍🎓</span>
                <strong>Ученик</strong>
                <span class="gc-role-desc">Проходить квесты, качать XP и соревноваться</span>
            </div>
            <div class="gc-role-card" id="gcRoleTeacher">
                <span class="gc-role-icon">👨‍🏫</span>
                <strong>Учитель</strong>
                <span class="gc-role-desc">Создавать курсы и вести свою аудиторию</span>
            </div>
        </div>

        <div class="gc-field"><label>Никнейм</label><input id="gcNick" placeholder=""></div>
        <div class="gc-row">
            <div class="gc-field"><label>Имя</label><input id="gcFirstName" value="${g.given_name || ""}"></div>
            <div class="gc-field"><label>Фамилия</label><input id="gcLastName" value="${g.family_name || ""}"></div>
        </div>
        <div class="gc-field"><label>Дата рождения</label><input id="gcBirth" type="date"></div>

        <button class="gc-submit" id="gcSubmit">Сохранить и войти</button>
    </div>`;
    document.body.appendChild(wrap);

    let selectedRole = 1;
    const rS = wrap.querySelector("#gcRoleStudent");
    const rT = wrap.querySelector("#gcRoleTeacher");
    
    rS.addEventListener("click", () => {
        selectedRole = 1;
        rS.className = "gc-role-card active-student";
        rT.className = "gc-role-card";
    });
    rT.addEventListener("click", () => {
        selectedRole = 2;
        rT.className = "gc-role-card active-teacher";
        rS.className = "gc-role-card";
    });

    wrap.querySelector("#gcSubmit").addEventListener("click", async () => {
        const birthInput = wrap.querySelector("#gcBirth");
        
        const payload = {
            selectedRole: selectedRole,
            userName: wrap.querySelector("#gcNick").value.trim(),
            firstName: wrap.querySelector("#gcFirstName").value.trim(),
            lastName: wrap.querySelector("#gcLastName").value.trim(),
            dateOfBirth: birthInput.value ? birthInput.value + "T00:00:00" : ""
        };

        if (!payload.firstName || !payload.lastName) { alert("Укажи имя и фамилию."); return; }
        if (!payload.dateOfBirth) { alert("Укажи дату рождения."); return; }

        const btn = wrap.querySelector("#gcSubmit");
        btn.disabled = true;
        btn.innerText = "Сохраняем профиль...";

        try {
            const response = await window.apiFetch("/api/user/CompleteProfile", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload) 
            });

            if (response.ok) {
                wrap.remove();
                window.location.href = "Main.html";
            } else {
                const err = await response.json().catch(() => null);
                alert("Ошибка: " + ((err && (err.error || err.message)) || ("статус " + response.status)));
                btn.disabled = false;
                btn.innerText = "Сохранить и войти";
            }
        } catch (e) {
            alert("Не удалось связаться с сервером");
            btn.disabled = false;
            btn.innerText = "Сохранить и войти";
        }
    });
}

ensureGoogleReady();

window.googleAuthUniversal = googleAuthUniversal;
window.googleLinkCurrentAccount = googleLinkCurrentAccount;