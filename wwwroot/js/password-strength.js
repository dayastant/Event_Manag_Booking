/**
 * Password Strength Indicator
 * Real-time password strength calculation with color transitions
 */

function calculatePasswordStrength(password) {
    if (!password) {
        return { strength: 0, level: '', color: '' };
    }

    let score = 0;
    
    // Length check
    if (password.length >= 8) score += 25;
    if (password.length >= 12) score += 10;
    
    // Contains lowercase
    if (/[a-z]/.test(password)) score += 15;
    
    // Contains uppercase
    if (/[A-Z]/.test(password)) score += 15;
    
    // Contains numbers
    if (/[0-9]/.test(password)) score += 15;
    
    // Contains special characters
    if (/[^a-zA-Z0-9]/.test(password)) score += 20;
    
    // Determine level and color
    let level, color;
    if (score < 40) {
        level = 'Weak';
        color = '#ff3b30'; // Bright Red
    } else if (score < 70) {
        level = 'Medium';
        color = '#ffcc00'; // Bright Yellow/Orange
    } else {
        level = 'Strong';
        color = '#00e676'; // Bright Neon Green
    }
    
    return { strength: score, level, color };
}

function updatePasswordStrength(inputElement) {
    const password = inputElement.value;
    const container = inputElement.closest('.password').querySelector('.password-strength-container');
    
    if (!container) return;
    
    const strengthBar = container.querySelector('.strength-bar-fill');
    const strengthText = container.querySelector('.strength-text');
    
    const result = calculatePasswordStrength(password);
    
    if (password.length === 0) {
        // Hide indicator when password is empty
        container.style.display = 'none';
        return;
    }
    
    // Show indicator
    container.style.display = 'block';
    
    // Update bar width with smooth transition
    strengthBar.style.width = result.strength + '%';
    strengthBar.style.backgroundColor = result.color;
    
    // Update text
    strengthText.textContent = result.level;
    strengthText.style.color = result.color;
}

// Initialize password strength indicator
document.addEventListener('DOMContentLoaded', function() {
    const passwordInput = document.getElementById('password');
    
    if (passwordInput) {
        // Create and append strength indicator if it doesn't exist
        const passwordContainer = passwordInput.closest('.password');
        if (passwordContainer && !passwordContainer.querySelector('.password-strength-container')) {
            const strengthContainer = document.createElement('div');
            strengthContainer.className = 'password-strength-container';
            strengthContainer.style.display = 'none';
            strengthContainer.innerHTML = `
                <div class="strength-bar">
                    <div class="strength-bar-fill"></div>
                </div>
                <div class="strength-text-container">
                    <span>Password strength: </span>
                    <span class="strength-text"></span>
                </div>
            `;
            
            // Insert after the password input
            passwordInput.parentNode.insertBefore(strengthContainer, passwordInput.nextSibling);
        }
        
        // Add input event listener
        passwordInput.addEventListener('input', function() {
            updatePasswordStrength(this);
        });
    }
});
