document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('mode') === 'register') {
        const backBtn = document.getElementById('backBtn');
        const emailStep = document.getElementById('step-email');
        const codeStep = document.getElementById('step-code');

        if (backBtn) backBtn.style.display = 'none';

        if (emailStep && codeStep) {
            emailStep.classList.remove('active');
            emailStep.style.display = 'none';
            codeStep.classList.add('active');
            codeStep.style.display = 'flex';

            const codeLabel = document.querySelector('.code-label');
            if (codeLabel) codeLabel.textContent = "На вашу почту отправлен 6-значный код для активации аккаунта EduGame";
        }
    }
});

function showStep(nextStepId, reverse = false) {
    const currentStep = document.querySelector('.step.active');
    const nextStep = document.getElementById(nextStepId);
    if (!nextStep || currentStep === nextStep) return;

    const outClass = reverse ? 'animate__fadeOutRightBig' : 'animate__fadeOutLeftBig';
    const inClass  = reverse ? 'animate__fadeInLeftBig'  : 'animate__fadeInRightBig';

    const showNext = () => {
        nextStep.style.display = 'flex';
        nextStep.classList.remove('animate__animated', 'animate__fadeOutLeftBig', 'animate__fadeOutRightBig', 'animate__fadeInLeftBig', 'animate__fadeInRightBig');
        void nextStep.offsetWidth;
        nextStep.classList.add('active', 'animate__animated', inClass);
    };

    if (currentStep) {
        let finished = false;
        const finishOut = () => {
            if (finished) return;
            finished = true;
            currentStep.removeEventListener('animationend', onEnd);
            currentStep.classList.remove('active', 'animate__animated', 'animate__fadeOutLeftBig', 'animate__fadeOutRightBig');
            currentStep.style.display = 'none';
            showNext();
        };

        const onEnd = (e) => {
            if (e.target !== currentStep) return;
            clearTimeout(safety);
            finishOut();
        };

        currentStep.classList.remove('animate__animated', 'animate__fadeInLeftBig', 'animate__fadeInRightBig');
        void currentStep.offsetWidth;
        currentStep.classList.add('animate__animated', outClass);
        currentStep.addEventListener('animationend', onEnd);

        const safety = setTimeout(finishOut, 1200);
    } else {
        showNext();
    }
}

async function handleEmail() {
    event.preventDefault();
    const emailInput = document.getElementById('email-field');
    const submitBtn = document.querySelector('#step-email .blue-btn');
    const originalBtnText = submitBtn.innerText;

    submitBtn.disabled = true;
    submitBtn.innerText = "Отправляем код...";

    try {
        const response = await fetch('/api/auth/ForgotPassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: emailInput.value,
                clientRoute: window.location.origin + window.location.pathname
            })
        });

        const raw = await response.text();
        let result = null;
        try { result = JSON.parse(raw); } catch { result = null; }

        if (response.ok) {
            let sessionId = null;
            if (typeof result === 'string' && result.trim()) {
                sessionId = result.trim();
            } else if (result && typeof result === 'object') {
                sessionId = result.sessionId || result.SessionId || result.sessionID || result.id || result.session || null;
            } else if (raw && !raw.trim().startsWith('{') && !raw.trim().startsWith('<')) {
                sessionId = raw.replace(/"/g, '').trim();
            }

            if (sessionId) {
                sessionStorage.setItem('resetSessionId', sessionId);
            } else {
                console.warn("⚠️ Сервер вернул 200 без sessionId. Убедись, что бэкенд делает: return Ok(new { sessionId = ... });");
            }

            showStep('step-code');
        } else {
            const errorMsg = (result && (result.error || result.message)) || `Ошибка сервера (${response.status})`;
            alert(errorMsg);
            submitBtn.disabled = false;
            submitBtn.innerText = originalBtnText;
        }
    } catch (error) {
        console.error("Сетевая ошибка:", error);
        alert("Не удалось связаться с сервером: " + error.message);
        submitBtn.disabled = false;
        submitBtn.innerText = originalBtnText;
    }
}

async function handleCode() {
    event.preventDefault();
    const backBtn = document.getElementById('backBtn');

    const code = Array.from(document.querySelectorAll('.code-input')).map(i => i.value).join('');
    const urlParams = new URLSearchParams(window.location.search);
    const isRegisterMode = urlParams.get('mode') === 'register';

    const sessionId = isRegisterMode
        ? localStorage.getItem('registerSessionId')
        : sessionStorage.getItem('resetSessionId');

    if (isRegisterMode) {
        const email = localStorage.getItem('registerEmail');

        if (!sessionId || sessionId === "null" || sessionId === "") {
            alert("Ошибка: сессия регистрации не найдена. Запросите код активации заново.");
            if (backBtn) backBtn.style.display = 'block';
            return;
        }

        try {
            const response = await fetch('/api/auth/ConfirmEmail', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: email, code: code })
            });

            if (response.ok) {
                localStorage.removeItem('registerSessionId');
                window.location.href = 'Main.html';
                return;
            } else {
                let errorMessage = "Неверный код";
                try {
                    const errorJson = await response.json();
                    errorMessage = errorJson.error || errorMessage;
                } catch {
                    errorMessage = `Ошибка сервера (${response.status})`;
                }
                alert("Ошибка активации: " + errorMessage);
                if (backBtn) backBtn.style.display = 'block';
                return;
            }
        } catch (e) {
            console.error("Критическая сетевая ошибка при регистрации:", e);
            alert("Не удалось связаться с сервером: " + e.message);
            if (backBtn) backBtn.style.display = 'block';
            return;
        }
    }

    try {
        const response = await fetch('/api/auth/CheckVerifyCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sessionId: sessionId, code: code })
        });

        if (response.ok) {
            showStep('step-password');
            setTimeout(() => document.getElementById('new-password')?.focus(), 700);
        } else {
            let errorMessage = "Неверный код";
            try {
                const errorJson = await response.json();
                errorMessage = errorJson.error || errorMessage;
            } catch {
                errorMessage = `Ошибка сервера (${response.status})`;
            }
            alert("Ошибка сброса: " + errorMessage);
        }
    } catch (e) {
        console.error("Критическая сетевая ошибка:", e);
        alert("Не удалось связаться с сервером: " + e.message);
    }
}

const inputs = document.querySelectorAll('.code-input');

inputs.forEach((input, index) => {
    input.addEventListener('input', () => {
        input.value = input.value.toUpperCase();
        if (input.value && index < inputs.length - 1) {
            inputs[index + 1].focus();
        }
    });

    input.addEventListener('keydown', (e) => {
        if (e.key === 'Backspace' && !input.value && index > 0) {
            inputs[index - 1].focus();
        }
    });
});

inputs[0].addEventListener('paste', (e) => {
    e.preventDefault();
    const data = e.clipboardData.getData('text');
    const values = data.split('').slice(0, inputs.length);
    values.forEach((value, i) => { if (inputs[i]) inputs[i].value = value; });
    const lastIndex = Math.min(values.length, inputs.length - 1);
    inputs[lastIndex].focus();
});

function exitToLogin() {
    event.preventDefault();
    showStep('step-email', true);
    setTimeout(() => {
        const submitBtn = document.querySelector('#step-email .blue-btn');
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerText = "Отправить код";
        }
    }, 600);
}

function goBackStep() {
    const currentStep = document.querySelector('.step.active');
    if (!currentStep) return;

    if (currentStep.id === 'step-code') {
        exitToLogin();
    } else if (currentStep.id === 'step-email') {
        window.location.href = 'login.html';
    } else if (currentStep.id === 'step-password') {
        showStep('step-code', true);
    }
}

window.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const codeFromUrl = urlParams.get('code');
    const sessionIdFromUrl = urlParams.get('session');

    if (codeFromUrl) {
        if (sessionIdFromUrl) {
            sessionStorage.setItem('resetSessionId', sessionIdFromUrl);
        }

        const emailStep = document.getElementById('step-email');
        const codeStep = document.getElementById('step-code');

        if (emailStep && codeStep) {
            emailStep.classList.remove('active', 'animate__animated');
            emailStep.style.display = 'none';

            codeStep.style.display = 'flex';
            codeStep.classList.add('active', 'animate__animated', 'animate__fadeInRightBig');
            codeStep.addEventListener('animationend', (e) => {
                if (e.target !== codeStep) return;
                codeStep.classList.remove('animate__animated', 'animate__fadeInRightBig');
            }, { once: true });
        }

        setTimeout(() => {
            const codeInputs = document.querySelectorAll('.code-input');
            codeFromUrl.split('').slice(0, 6).forEach((char, index) => {
                if (codeInputs[index]) {
                    codeInputs[index].value = char.toUpperCase();
                    codeInputs[index].classList.add('animate__animated', 'animate__bounceIn');
                    setTimeout(() => {
                        codeInputs[index].classList.remove('animate__animated', 'animate__bounceIn');
                    }, 1000);
                }
            });
        }, 300);

        setTimeout(() => {
            const codeForm = document.querySelector('#step-code form');
            if (codeForm) codeForm.requestSubmit();
        }, 1200);
    }
});

async function handleResetPassword() {
    event.preventDefault();

    const newPassword = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;
    const sessionId = sessionStorage.getItem('resetSessionId');
    const submitBtn = document.querySelector('#step-password .blue-btn');
    const originalBtnText = submitBtn.innerText;

    if (newPassword !== confirmPassword) {
        alert("Ошибка: введенные пароли не совпадают");
        return;
    }

    if (!sessionId || sessionId == "null" || sessionId == "undefined") {
        alert("Ошибка сессии восстановления. Начните процесс заново со ввода Email");
        return;
    }

    submitBtn.disabled = true;
    submitBtn.innerText = "Сохраняем новый пароль...";

    try {
        const response = await fetch('/api/auth/ChangePassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sessionId: sessionId, newPassword: newPassword })
        });

        if (response.ok) {
            sessionStorage.removeItem('resetSessionId');
            window.location.href = 'Main.html';
        } else {
            const errorData = await response.json();
            if (errorData && errorData.error) {
                alert("Ошибки:\n" + errorData.error);
            } else {
                alert("Ошибка сервера: " + response.status);
            }
            submitBtn.disabled = false;
            submitBtn.innerText = originalBtnText;
        }
    } catch (e) {
        console.error("Критическая сетевая ошибка:", e);
        alert("Не удалось связаться с сервером. Проверьте интернет-соединение.");
        submitBtn.disabled = false;
        submitBtn.innerText = originalBtnText;
    }
}

function togglePass(btn) {
    const input = btn.previousElementSibling;
    if (input.getAttribute('type') === 'password') {
        input.setAttribute('type', 'text');
        btn.textContent = '🙈';
    } else {
        input.setAttribute('type', 'password');
        btn.textContent = '👁️';
    }
}