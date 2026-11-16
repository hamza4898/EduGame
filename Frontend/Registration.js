function hideAllForms() {
    document.querySelectorAll('.role-form').forEach(form => {
        form.classList.remove('active');
    });
}

document.addEventListener('DOMContentLoaded', () => {
    const studentForm = document.getElementById('studentForm');
    const teacherForm = document.getElementById('teacherForm');

    async function handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;
        
        const submitBtn = form.querySelector('[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        const url = form.getAttribute('action');
        const data = Object.fromEntries(new FormData(form));
        data.ClientRoute = window.location.origin + "/ForgotPassword.html?mode=register";

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.text();

            if (response.ok) {
                localStorage.setItem('registerSessionId', result);
                if (data.email || data.Email) {
                    localStorage.setItem('registerEmail', data.email || data.Email);
                }
                window.location.href = 'ForgotPassword.html?mode=register';
            } else {
                if (submitBtn) submitBtn.disabled = false; // Возвращаем кнопку в случае ошибки
                try {
                    const errorJson = JSON.parse(result);
                    alert("Ошибка регистрации:\n" + (errorJson.error || "Дубликат данных"));
                } catch (p) {
                    alert("Ошибка регистрации:\n" + result);
                }
            }
        } catch (err) {
            if (submitBtn) submitBtn.disabled = false;
            console.error("Критический сбой сети:", err);
            alert("Ошибка сети при регистрации");
        }
    }

    if (studentForm) studentForm.addEventListener('submit', handleFormSubmit);
    if (teacherForm) teacherForm.addEventListener('submit', handleFormSubmit);

});


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

function showForm(role) {
    document.querySelectorAll('.role-form').forEach(f => f.classList.remove('active'));
    
    const targetForm = document.getElementById(role + 'Form');
    if (targetForm) {
        targetForm.classList.add('active');
    } else {
        console.error(`[DOM Error] Форма с ID ${role}Form не найдена в верстке!`);
    }
}

window.showForm = showForm;
window.togglePass = togglePass;