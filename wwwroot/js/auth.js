/* ========== AUTH PAGE SCRIPTS ========== */

/* Custom Toast Notification */
function showToast(message, type = 'error', duration = 4000) {
    // Remove any existing toasts
    const existingToast = document.querySelector('.toast-notification');
    if (existingToast) {
        existingToast.remove();
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}`;
    
    // Icon based on type
    const icons = {
        error: '❌',
        success: '✅',
        warning: '⚠️',
        info: 'ℹ️'
    };
    
    const titles = {
        error: 'Error',
        success: 'Success',
        warning: 'Warning',
        info: 'Info'
    };
    
    toast.innerHTML = `
        <div class="toast-icon">${icons[type]}</div>
        <div class="toast-content">
            <div class="toast-title">${titles[type]}</div>
            <div class="toast-message">${message}</div>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()">×</button>
    `;
    
    document.body.appendChild(toast);
    
    // Auto remove after duration
    setTimeout(() => {
        toast.classList.add('hiding');
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

/* Toggle Password Visibility */
function togglePassword(icon) {
    // Find the input within the same parent .password container
    // This is more robust than previousElementSibling which fails if other elements (like strength meter) are injected
    const input = icon.closest('.password').querySelector('input');
    
    if (input.type === "password") {
        input.type = "text";
        icon.classList.remove("fa-eye");
        icon.classList.add("fa-eye-slash");
    } else {
        input.type = "password";
        icon.classList.remove("fa-eye-slash");
        icon.classList.add("fa-eye");
    }
}

/* OAuth Coming Soon Message */
function showComingSoon(provider) {
    showToast(`${provider} login is coming soon! Stay tuned for this feature.`, 'info', 3000);
}


/* Multi-step Form Navigation */
async function goToProfileStep() {
    const step1 = document.getElementById("step-1");
    const step2 = document.getElementById("step-2");
    
    // Get all inputs
    const firstname = document.getElementById("firstname");
    const lastname = document.getElementById("lastname");
    const email = document.getElementById("email");
    const password = document.getElementById("password");
    const confirmpassword = document.getElementById("confirmpassword");
    
    let isValid = true;
    let errorMessage = "";
    
    // Reset all borders
    [firstname, lastname, email, password, confirmpassword].forEach(input => {
        if (input) input.style.borderColor = "";
    });
    
    // Check required fields
    if (!firstname.value.trim()) {
        firstname.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "First name is required";
    }
    if (!lastname.value.trim()) {
        lastname.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Last name is required";
    }
    if (!email.value.trim()) {
        email.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Email is required";
    }
    if (!password.value) {
        password.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Password is required";
    } else if (password.value.length < 8 || password.value.length > 15) {
        password.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Password must be between 8 and 15 characters";
    } else {
        // Check password strength requirements
        const hasUpperCase = /[A-Z]/.test(password.value);
        const hasLowerCase = /[a-z]/.test(password.value);
        const hasNumber = /\d/.test(password.value);
        const hasSpecialChar = /[@$!%*?-_&]/.test(password.value);
        
        if (!hasUpperCase) {
            password.style.borderColor = "#dc3545";
            isValid = false;
            errorMessage = "Password must contain at least one uppercase letter";
        } else if (!hasLowerCase) {
            password.style.borderColor = "#dc3545";
            isValid = false;
            errorMessage = "Password must contain at least one lowercase letter";
        } else if (!hasNumber) {
            password.style.borderColor = "#dc3545";
            isValid = false;
            errorMessage = "Password must contain at least one number";
        } else if (!hasSpecialChar) {
            password.style.borderColor = "#dc3545";
            isValid = false;
            errorMessage = "Password must contain at least one special character (@$!%*-_?&)";
        }
    }
    if (!confirmpassword.value) {
        confirmpassword.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Please confirm your password";
    } else if (password.value !== confirmpassword.value) {
        password.style.borderColor = "#dc3545";
        confirmpassword.style.borderColor = "#dc3545";
        isValid = false;
        errorMessage = "Passwords do not match";
    }

    if (isValid) {
        // Check if email already exists
        const nextBtn = document.querySelector("#step-1 button.btn-primary");
        const originalText = nextBtn.innerHTML;
        nextBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Checking...';
        nextBtn.disabled = true;

        try {
            const response = await fetch(`/Account/CheckEmailExists?email=${encodeURIComponent(email.value)}`);
            const data = await response.json();

            if (data.exists) {
                email.style.borderColor = "#dc3545";
                showToast("Email already registered. Please login or use a different email.", 'error');
                email.focus();
            } else {
                step1.classList.add("hidden");
                step2.classList.remove("hidden");
            }
        } catch (error) {
            console.error('Error checking email:', error);
            showToast("Error verifying email. Please try again.", 'error');
        } finally {
            nextBtn.innerHTML = originalText;
            nextBtn.disabled = false;
        }
    } else {
        // Show error and focus first invalid field
        if (errorMessage) {
            showToast(errorMessage, 'error');
        }
        const firstInvalid = [firstname, lastname, email, password, confirmpassword]
            .find(input => input.style.borderColor === "rgb(220, 53, 69)");
        if (firstInvalid) firstInvalid.focus();
    }
}

function goToPreferencesStep() {
    console.log("Going to preferences step");
    const step2 = document.getElementById("step-2");
    const step3 = document.getElementById("step-3");
    
    if (!step2 || !step3) {
        console.error("Step elements not found!");
        return;
    }
    
    step2.classList.add("hidden");
    step3.classList.remove("hidden");
    console.log("Successfully moved to step 3");
}

function skipProfilePicture() {
    // Clear the file input
    document.getElementById("profilePictureInput").value = "";
    goToPreferencesStep();
}

function prevStep(stepNumber) {
    if (stepNumber === 1) {
        document.getElementById("step-1").classList.remove("hidden");
        document.getElementById("step-2").classList.add("hidden");
    } else if (stepNumber === 2) {
        document.getElementById("step-2").classList.remove("hidden");
        document.getElementById("step-3").classList.add("hidden");
    }
}

/* Profile Picture Preview */
function previewProfilePicture(input) {
    const preview = document.getElementById("profilePreview");
    
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            preview.innerHTML = `<img src="${e.target.result}" alt="Profile Preview" style="width: 100%; height: 100%; object-fit: cover; border-radius: 50%;">`;
        };
        
        reader.readAsDataURL(input.files[0]);
    }
}

/* Preference Toggling */
function togglePreference(chip) {
    chip.classList.toggle("selected");
    updatePreferences();
}

function updatePreferences() {
    const selectedChips = document.querySelectorAll(".chip.selected");
    const values = Array.from(selectedChips).map(chip => chip.dataset.value);
    
    if (values.length > 0) {
        document.getElementById("preferences-input").value = values[0];
    } else {
        document.getElementById("preferences-input").value = "";
    }
}

/* Combine country code with phone number on form submit */
document.addEventListener('DOMContentLoaded', function() {
    const registerForm = document.getElementById('register-form');
    if (registerForm) {
        registerForm.addEventListener('submit', function(e) {
            const phoneInput = document.getElementById('phone');
            const countryCode = document.getElementById('countryCode');
            
            if (phoneInput && countryCode && phoneInput.value.trim()) {
                // Combine country code with phone number
                phoneInput.value = countryCode.value + phoneInput.value.trim();
            }
        });
    }
});
