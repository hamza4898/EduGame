fetch("/api/home")
    .then(r => r.json())
    .then(data => console.log(data));

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
    
    await fetch(form.action, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(Object.fromEntries(new FormData(form)))
    });

    window.location.href = "/Main.html";
});