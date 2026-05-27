/**
 * Global Popup Modal System
 * Version: 1.0
 * Dependencies: None (Vanilla JavaScript)
 */

class GlobalPopup {
    constructor() {
        this.overlay = null;
        this.popup = null;
        this.currentCallback = null;
        this.init();
    }

    init() {
        // Create overlay and popup if they don't exist
        if (!document.querySelector('.global-popup-overlay')) {
            this.createPopupStructure();
        }
        
        this.overlay = document.querySelector('.global-popup-overlay');
        this.popup = document.querySelector('.global-popup');
        
        // Event listeners
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay) {
                this.close();
            }
        });
        
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.overlay.classList.contains('active')) {
                this.close();
            }
        });
    }

    createPopupStructure() {
        const overlay = document.createElement('div');
        overlay.className = 'global-popup-overlay';
        overlay.innerHTML = `
            <div class="global-popup">
                <div class="popup-header">
                    <h3 id="popupTitle"></h3>
                    <button class="popup-close" onclick="globalPopup.close()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="popup-body" id="popupBody"></div>
                <div class="popup-footer" id="popupFooter"></div>
            </div>
        `;
        document.body.appendChild(overlay);
    }

    open(options) {
        const {
            title,
            body,
            buttons = [],
            onClose = null
        } = options;

        // Set title
        document.getElementById('popupTitle').textContent = title;

        // Set body
        const bodyElement = document.getElementById('popupBody');
        if (typeof body === 'string') {
            bodyElement.innerHTML = body;
        } else {
            bodyElement.innerHTML = '';
            bodyElement.appendChild(body);
        }

        // Set buttons
        const footerElement = document.getElementById('popupFooter');
        footerElement.innerHTML = '';
        buttons.forEach(btn => {
            const button = document.createElement('button');
            button.className = `popup-btn ${btn.className || 'popup-btn-secondary'}`;
            button.innerHTML = btn.icon ? `<i class="${btn.icon}"></i> ${btn.text}` : btn.text;
            button.onclick = () => {
                if (btn.onClick) {
                    btn.onClick();
                }
                if (!btn.keepOpen) {
                    this.close();
                }
            };
            footerElement.appendChild(button);
        });

        this.currentCallback = onClose;
        this.overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    close() {
        this.overlay.classList.remove('active');
        document.body.style.overflow = '';
        
        if (this.currentCallback) {
            this.currentCallback();
            this.currentCallback = null;
        }
    }

    // Helper: Confirmation Dialog
    confirm(options) {
        return new Promise((resolve) => {
            const {
                title = 'Confirm Action',
                message,
                icon = 'fa-exclamation-triangle',
                iconType = 'danger', // Default to danger for delete/cancel actions per design
                confirmText = 'Confirm',
                cancelText = 'Cancel',
                onConfirm,
                onCancel
            } = options;

            // Custom HTML to match the specific design (Icon in circle, centered text)
            const body = `
                <div style="text-align: center; padding: 10px 20px;">
                    <div style="width: 80px; height: 80px; background: #fee2e2; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px auto;">
                        <i class="fas ${icon}" style="font-size: 32px; color: #dc2626;"></i>
                    </div>
                    <h3 style="margin: 0 0 10px 0; color: #111827; font-size: 1.5rem; font-weight: 700;">${title}</h3>
                    <p style="margin: 0 0 20px 0; color: #6b7280; font-size: 1rem; line-height: 1.5;">${message}</p>
                </div>
            `;

            this.open({
                title: '', // Hide default title bar title since we include it in the body
                body,
                buttons: [
                    {
                        text: cancelText,
                        className: 'popup-btn-secondary', // White/Outline
                        onClick: () => {
                            if (onCancel) onCancel();
                            resolve(false);
                        }
                    },
                    {
                        text: confirmText,
                        className: 'popup-btn-danger', // Red
                        onClick: () => {
                            if (onConfirm) onConfirm();
                            resolve(true);
                        }
                    }
                ]
            });
            
            // Hide the default header since we are mocking it in the body for this specific design
            const header = document.querySelector('.popup-header');
            if (header) header.style.display = 'none';
            
            // Restore header when closing
            const originalClose = this.close.bind(this);
            this.close = () => {
                if (header) header.style.display = 'flex';
                originalClose();
                this.close = originalClose; // Reset
                resolve(false); // If closed without clicking a button, resolve as false
            };
        });
    }

    // Helper: Form Dialog
    form(options) {
        const {
            title,
            fields = [],
            submitText = 'Submit',
            cancelText = 'Cancel',
            onSubmit
        } = options;

        const form = document.createElement('form');
        form.className = 'popup-form';
        form.onsubmit = (e) => {
            e.preventDefault();
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            
            // Validate
            let isValid = true;
            fields.forEach(field => {
                if (field.required && !data[field.name]) {
                    isValid = false;
                    const group = form.querySelector(`[name="${field.name}"]`).closest('.popup-form-group');
                    group.classList.add('has-error');
                    group.querySelector('.error-message').textContent = field.errorMessage || 'This field is required';
                }
            });

            if (isValid && onSubmit) {
                onSubmit(data);
            }
        };

        // Build form fields
        fields.forEach(field => {
            const group = document.createElement('div');
            group.className = 'popup-form-group';
            
            let input;
            if (field.type === 'textarea') {
                input = `<textarea name="${field.name}" placeholder="${field.placeholder || ''}" ${field.required ? 'required' : ''}></textarea>`;
            } else if (field.type === 'select') {
                input = `
                    <select name="${field.name}" ${field.required ? 'required' : ''}>
                        ${field.options.map(opt => `<option value="${opt.value}">${opt.label}</option>`).join('')}
                    </select>
                `;
            } else {
                input = `<input type="${field.type || 'text'}" name="${field.name}" placeholder="${field.placeholder || ''}" ${field.required ? 'required' : ''}>`;
            }

            group.innerHTML = `
                <label>${field.label}${field.required ? ' <span style="color: #dc2626;">*</span>' : ''}</label>
                ${input}
                <span class="error-message"></span>
            `;
            form.appendChild(group);
        });

        this.open({
            title,
            body: form,
            buttons: [
                {
                    text: cancelText,
                    className: 'popup-btn-secondary'
                },
                {
                    text: submitText,
                    className: 'popup-btn-primary',
                    onClick: () => form.requestSubmit(),
                    keepOpen: true
                }
            ]
        });
    }

    // Helper: Alert Dialog
    alert(options) {
        const {
            title = 'Alert',
            message,
            icon = 'fa-info-circle',
            iconType = 'info',
            buttonText = 'OK'
        } = options;

        // Define colors based on iconType
        let bgColor, iconColor;
        switch(iconType) {
            case 'success':
                bgColor = '#d1fae5';  // Light green background
                iconColor = '#00FF00'; // Bright green icon
                break;
            case 'warning':
                bgColor = '#fef3c7';   // Light amber background
                iconColor = '#FFBF00'; // Amber icon
                break;
            case 'danger':
                bgColor = '#fee2e2';   // Light red background
                iconColor = '#dc2626'; // Red icon
                break;
            default: // 'info'
                bgColor = '#dbeafe';   // Light blue background
                iconColor = '#3b82f6'; // Blue icon
        }

        const body = `
            <div style="text-align: center; padding: 10px 20px;">
                <div style="width: 80px; height: 80px; background: ${bgColor}; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px auto;">
                    <i class="fas ${icon}" style="font-size: 32px; color: ${iconColor};"></i>
                </div>
                <h3 style="margin: 0 0 10px 0; color: #111827; font-size: 1.5rem; font-weight: 700;">${title}</h3>
                <p style="margin: 0 0 20px 0; color: #6b7280; font-size: 1rem; line-height: 1.5;">${message}</p>
            </div>
        `;

        this.open({
            title: '',
            body,
            buttons: [
                {
                    text: buttonText,
                    className: 'popup-btn-primary'
                }
            ]
        });
        
        // Hide the default header
        const header = document.querySelector('.popup-header');
        if (header) header.style.display = 'none';
        
        // Restore header when closing
        const originalClose = this.close.bind(this);
        this.close = () => {
            if (header) header.style.display = 'flex';
            originalClose();
            this.close = originalClose;
        };
    }
}

// Initialize global instance
const globalPopup = new GlobalPopup();

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GlobalPopup;
}
