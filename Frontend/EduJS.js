function showForm(role) {
    hideAllForms();
    document.getElementById(role + 'Form').classList.add('active');
    document.querySelector('#cooperation').scrollIntoView({ behavior: 'smooth' });
}

function hideAllForms() {
    document.querySelectorAll('.role-form').forEach(form => {
        form.classList.remove('active');
    });
}

function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
}

window.onclick = function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = 'none';
    }
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
            window.location.href = "/Main.html";
        } else {
            alert("Ошибка сервера: " + response.status);
        }
    } catch (err) {
        alert("Ошибка сети (проверь бэкенд)");
    }
});

function showForm(role) {
    document.querySelectorAll('.role-form').forEach(f => f.classList.remove('active'));
    document.getElementById(role + 'Form').classList.add('active');
}
