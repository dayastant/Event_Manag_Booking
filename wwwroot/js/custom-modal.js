// ==================== CUSTOM MODAL SYSTEM ====================
// Premium notification modals to replace alert() calls

const CustomModal = {
    // Create modal HTML if it doesn't exist
    init() {
        if (document.getElementById('custom-modal-container')) return;

        const modalHTML = `
            <div id="custom-modal-container" class="custom-modal-overlay" style="display: none;">
                <div class="custom-modal glass-card">
                    <div class="custom-modal-icon" id="custom-modal-icon"></div>
                    <h2 class="custom-modal-title" id="custom-modal-title"></h2>
                    <p class="custom-modal-message" id="custom-modal-message"></p>
                    <div class="custom-modal-actions">
                        <button class="btn-primary custom-modal-btn" id="custom-modal-btn">OK</button>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHTML);
        this.attachEventListeners();
    },

    // Attach event listeners
    attachEventListeners() {
        const overlay = document.getElementById('custom-modal-container');
        const btn = document.getElementById('custom-modal-btn');

        btn.addEventListener('click', () => this.close());
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) this.close();
        });

        // Close on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && overlay.style.display === 'flex') {
                this.close();
            }
        });
    },

    // Show success modal
    showSuccess(title, message, callback = null) {
        this.init();
        this.show('success', title, message, callback);
    },

    // Show error modal
    showError(title, message, callback = null) {
        this.init();
        this.show('error', title, message, callback);
    },

    // Show warning modal
    showWarning(title, message, callback = null) {
        this.init();
        this.show('warning', title, message, callback);
    },

    // Show info modal
    showInfo(title, message, callback = null) {
        this.init();
        this.show('info', title, message, callback);
    },

    // Generic show method
    show(type, title, message, callback = null) {
        const overlay = document.getElementById('custom-modal-container');
        const iconEl = document.getElementById('custom-modal-icon');
        const titleEl = document.getElementById('custom-modal-title');
        const messageEl = document.getElementById('custom-modal-message');
        const modal = overlay.querySelector('.custom-modal');

        // Set icon based on type
        const icons = {
            success: '<i class="fas fa-check-circle" style="color: #28a745; font-size: 4rem;"></i>',
            error: '<i class="fas fa-times-circle" style="color: #dc3545; font-size: 4rem;"></i>',
            warning: '<i class="fas fa-exclamation-triangle" style="color: #ffc107; font-size: 4rem;"></i>',
            info: '<i class="fas fa-info-circle" style="color: #3B82F6; font-size: 4rem;"></i>'
        };

        iconEl.innerHTML = icons[type] || icons.info;
        titleEl.textContent = title;
        messageEl.textContent = message;

        // Store callback
        this.currentCallback = callback;

        // Show modal with animation
        overlay.style.display = 'flex';
        setTimeout(() => {
            overlay.classList.add('active');
            modal.classList.add('active');
        }, 10);
    },

    // Close modal
    close() {
        const overlay = document.getElementById('custom-modal-container');
        const modal = overlay.querySelector('.custom-modal');

        overlay.classList.remove('active');
        modal.classList.remove('active');

        setTimeout(() => {
            overlay.style.display = 'none';
            if (this.currentCallback) {
                this.currentCallback();
                this.currentCallback = null;
            }
        }, 300);
    },

    currentCallback: null
};

// Global convenience functions
function showSuccessModal(title, message, callback) {
    CustomModal.showSuccess(title, message, callback);
}

function showErrorModal(title, message, callback) {
    CustomModal.showError(title, message, callback);
}

function showWarningModal(title, message, callback) {
    CustomModal.showWarning(title, message, callback);
}

function showInfoModal(title, message, callback) {
    CustomModal.showInfo(title, message, callback);
}

// Auto-initialize on DOM load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => CustomModal.init());
} else {
    CustomModal.init();
}
