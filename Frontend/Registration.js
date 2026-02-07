function hideAllForms() {
    document.querySelectorAll('.role-form').forEach(form => {
        form.classList.remove('active');
    });
}

document.addEventListener('submit', async (e) => {
    e.preventDefault(); 

    const form = e.target;
    const url = form.getAttribute('action');
    const data = Object.fromEntries(new FormData(form)); 

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            window.location.href = "/Profile.html";
        } else {
            alert("Ошибка сервера: " + response.status);
        }
    } catch (err) {
        alert("Ошибка сети");
    }
});

function showForm(role) {
    document.querySelectorAll('.role-form').forEach(f => f.classList.remove('active'));
    document.getElementById(role + 'Form').classList.add('active');
}