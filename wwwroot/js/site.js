function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);
    if (!input) return;

    const icon = button.querySelector("i");
    if (!icon) return;

    if (input.type === "password") {
        input.type = "text";
        icon.classList.remove("bi-eye");
        icon.classList.add("bi-eye-slash");
    } else {
        input.type = "password";
        icon.classList.remove("bi-eye-slash");
        icon.classList.add("bi-eye");
    }
}

// =========================
// PASSWORD STRENGTH FUNCTION
// =========================
const passwordInput = document.getElementById('passwordInput');
const strengthBar = document.getElementById('passwordStrengthBar');
const strengthText = document.getElementById('passwordStrengthText');

passwordInput.addEventListener('input', () => {
    const value = passwordInput.value;
    const strength = calculateStrength(value);

    // Ajustar barra
    strengthBar.style.width = strength.percent + '%';
    strengthBar.className = 'progress-bar ' + strength.colorClass;

    // Ajustar texto
    strengthText.textContent = strength.text;
});

function calculateStrength(password) {
    let score = 0;

    if (!password) return { percent: 0, text: '', colorClass: '' };
    if (password.length >= 8) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    let percent = (score / 5) * 100;
    let text = '';
    let colorClass = '';

    if (score <= 2) {
        text = 'Débil';
        colorClass = 'bg-danger';
    } else if (score === 3) {
        text = 'Aceptable';
        colorClass = 'bg-warning';
    } else if (score >= 4) {
        text = 'Fuerte';
        colorClass = 'bg-success';
    }

    return { percent, text, colorClass };
}

