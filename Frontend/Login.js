async function loginUser(formId) {
    const form = document.getElementById(formId);
    const data = Object.fromEntries(new FormData(form));

    try {
        const response = await apiFetch('/api/login/LoginUser', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (response.ok) {
            localStorage.setItem("isTwoFactorEnabled", result);
            window.location.href = "/Main.html"
        } else {       
            if (result.error) {
                const messages = result.error;
                alert("Ошибки:\n" + messages);
            } else {
                alert("Ошибка сервера: " + response.status);
            }
        }
    } catch (err) {
        console.error("Ошибка запроса:", err);
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

function fadeOutAndRedirect(url) {
    const form = document.querySelector('.login-card');
    
    form.classList.add('animate__animated', 'animate__fadeOutLeft');

    setTimeout(() => {
        window.location.href = url;
    }, 500);
}