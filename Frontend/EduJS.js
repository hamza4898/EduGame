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

function download(platform) {
    document.getElementById('downloadModal').style.display = 'block';
    console.log(`Скачивание для ${platform}...`);
}

document.querySelectorAll('.role-form form').forEach(form => {
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        alert('✅ Заявка отправлена!\n\nМы свяжемся с вами в ближайшее время.\nСпасибо за интерес к EduGame!');
        this.reset();
        hideAllForms();
    });
});

function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
}

window.onclick = function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = 'none';
    }
}