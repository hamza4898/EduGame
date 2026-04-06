async function loginUser(formId) {
    const form = document.getElementById(formId);
    const data = Object.fromEntries(new FormData(form));

    try {
        const response = await fetch('/api/login/LoginUser', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            window.location.href = "/Profile.html";
        } else {
            const errorData = await response.json();
            
            if (errorData && errorData.error) {
                const messages = errorData.error;
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