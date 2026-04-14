function showStep(nextStepId) {
    const currentStep = document.querySelector('.step.active');
    const nextStep = document.getElementById(nextStepId);

    if (currentStep) {
        currentStep.classList.add('animate__animated', 'animate__fadeOutLeftBig');

        setTimeout(() => {
            currentStep.classList.remove('active', 'animate__animated', 'animate__fadeOutLeftBig');
            currentStep.style.display = 'none';

            nextStep.style.display = 'flex';
            nextStep.classList.add('active', 'animate__animated', 'animate__fadeInRightBig');
        }, 500);
    } else {
        nextStep.style.display = 'flex';
        nextStep.classList.add('active', 'animate__animated', 'animate__fadeIn');
    }
}

function handleEmail() {
    event.preventDefault();
    console.log("Email отправлен:", document.getElementById('email-field').value);
    showStep('step-code');
}

function handleCode() {
    event.preventDefault();
    
    const currentStep = document.getElementById('step-code');
    const backBtn = document.querySelector('.back-circle');

    currentStep.classList.add('animate__animated', 'animate__zoomOut');
    backBtn.classList.add('animate__animated', 'animate__zoomOut');

    setTimeout(() => {
        window.location.href = 'login.html';
    }, 500); 
}

const inputs = document.querySelectorAll('.code-input');

inputs.forEach((input, index) => {
  input.addEventListener('input', () => {
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

    values.forEach((value, i) => {
        if (inputs[i]) {
            inputs[i].value = value;
        }
    });

    const lastIndex = Math.min(values.length, inputs.length - 1);
    inputs[lastIndex].focus();
});


function exitToLogin() {
    event.preventDefault();

    const currentStep = document.getElementById('step-code');
    const prevStep = document.getElementById('step-email');

    if (currentStep && prevStep) {
        currentStep.classList.remove('animate__fadeInRightBig');
        
        currentStep.classList.add('animate__animated', 'animate__fadeOutRightBig');

        setTimeout(() => {
            currentStep.classList.remove('active', 'animate__animated', 'animate__fadeOutRightBig');
            currentStep.style.display = 'none';

            prevStep.style.display = 'flex';
            prevStep.classList.remove('animate__fadeOutLeftBig'); 
            prevStep.classList.add('active', 'animate__animated', 'animate__fadeInLeftBig');
        }, 500);
    }
}

function goBackStep() {
    const currentStep = document.querySelector('.step.active');
    
    if (currentStep.id === 'step-code') {
        exitToLogin(); 
    } else if (currentStep.id === 'step-email') {
        window.location.href = 'login.html'; 
    }
}




