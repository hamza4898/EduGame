function showAuthButtons() {
    const startBtn = document.getElementById('startButton');
    const authContainer = document.getElementById('authContainer');
    const loginBtn = document.getElementById('loginButton');
    const regBtn = document.getElementById('regButton');

    startBtn.classList.add('animate__animated', 'animate__zoomOut', 'animate__faster');

    setTimeout(() => {
        startBtn.style.display = 'none';

        authContainer.style.display = 'flex';

        loginBtn.classList.add('animate__animated', 'animate__backInLeft');
        regBtn.classList.add('animate__animated', 'animate__backInRight');

        setTimeout(() => {
            loginBtn.classList.remove('animate__backInLeft');
            regBtn.classList.remove('animate__backInRight');
        }, 1000);
        
    }, 500);
}
