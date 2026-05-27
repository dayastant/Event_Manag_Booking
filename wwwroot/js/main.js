document.addEventListener("DOMContentLoaded", () => {
    
    // ================= SCROLL ANIMATIONS =================
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.reveal, .event-card, .service-card, .about-content, .stats-bar, .hero h1, .hero p').forEach(el => {
        el.classList.add('reveal');
        observer.observe(el);
    });

    // ================= STICKY HEADER =================
    const header = document.querySelector('header');
    if (header) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 50) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        });
    }

    // ================= STATS ANIMATION =================
    const counters = document.querySelectorAll('.stat-number');
    const speed = 200; // The lower the slower

    counters.forEach(counter => {
        const updateCount = () => {
            const target = +counter.getAttribute('data-target');
            const count = +counter.innerText;
            const inc = target / speed;

            if (count < target) {
                counter.innerText = Math.ceil(count + inc);
                setTimeout(updateCount, 20);
            } else {
                counter.innerText = target;
            }
        };
        updateCount();
    });

    // ================= POPUP LOGIC =================
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');

    if (status) {
        showPopup(status);
        window.history.replaceState({}, document.title, window.location.pathname);
    }
});



// ================= REVIEW MANAGER =================
class ReviewManager {
    constructor() {
        this.init();
    }

    async init() {
        if (!localStorage.getItem('reviews')) {
            try {
                const response = await fetch('data/reviews.json');
                const reviews = await response.json();
                localStorage.setItem('reviews', JSON.stringify(reviews));
            } catch (error) {
                console.error("Failed to load review data:", error);
                localStorage.setItem('reviews', JSON.stringify([]));
            }
        }
    }

    getReviews() {
        return JSON.parse(localStorage.getItem('reviews') || '[]');
    }

    getReviewsByEvent(eventName) {
        const reviews = this.getReviews();
        return reviews.filter(r => r.eventName === eventName);
    }

    addReview(eventName, rating, text) {
        const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
        const userName = currentUser.fullname || 'Anonymous';
        
        const review = {
            eventName,
            userName,
            rating,
            text,
            date: new Date().toISOString().split('T')[0]
        };

        const reviews = this.getReviews();
        reviews.push(review);
        localStorage.setItem('reviews', JSON.stringify(reviews));
        return review;
    }

    getAverageRating(eventName) {
        const reviews = this.getReviewsByEvent(eventName);
        if (reviews.length === 0) return 0;
        const sum = reviews.reduce((acc, r) => acc + r.rating, 0);
        return (sum / reviews.length).toFixed(1);
    }
}

const reviewManager = new ReviewManager();

// ================= EVENTS LOGIC =================

// Filter Events
function filterEvents() {
    const search = document.getElementById('search-input').value.toLowerCase();
    const category = document.getElementById('category-filter').value;
    const cards = document.querySelectorAll('.event-card');
    
    cards.forEach(card => {
        const title = card.querySelector('h3').innerText.toLowerCase();
        const cardCat = card.getAttribute('data-category');
        
        const matchesSearch = title.includes(search);
        const matchesCategory = category === 'all' || cardCat === category;
        
        if (matchesSearch && matchesCategory) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });
}

// Toast Notification System
function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; display: flex; flex-direction: column; gap: 10px;';
        document.body.appendChild(container);
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.style.cssText = `
        background: ${type === 'error' ? '#EF4444' : type === 'success' ? '#10B981' : '#3B82F6'};
        color: white;
        padding: 12px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        min-width: 300px;
        opacity: 0;
        transform: translateX(100%);
        transition: all 0.3s ease;
        display: flex;
        align-items: center;
        gap: 10px;
    `;
    
    // Icon based on type
    const icon = type === 'error' ? 'fa-circle-exclamation' : type === 'success' ? 'fa-check-circle' : 'fa-info-circle';
    
    toast.innerHTML = `
        <i class="fas ${icon}"></i>
        <span style="font-weight: 500;">${message}</span>
    `;

    container.appendChild(toast);

    // Animate in
    requestAnimationFrame(() => {
        toast.style.opacity = '1';
        toast.style.transform = 'translateX(0)';
    });

    // Remove after 3 seconds
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => {
            container.removeChild(toast);
        }, 300);
    }, 3000);
}

// Guest Action Handler
function handleGuestAction(actionName) {
    if (!window.isLoggedIn) {
        showToast(`Please login or register to ${actionName}.`, 'error');
        return true; // Action handled (blocked) - return true to stop onclick
    }
    return false; // User is logged in, proceed with default action
}


// Contact Form
function handleContact(e) {
    e.preventDefault();
    showPopup('inquiry_success');
    e.target.reset(); // Clear form
}

// Utils
function closeModal(id) {
    document.getElementById(id).style.display = 'none';
}

function showPopup(status) {
    const popupContainer = document.getElementById('popup-container');
    const title = document.getElementById('popup-title');
    const message = document.getElementById('popup-message');
    const progressBar = document.getElementById('progress-bar');

    if (!popupContainer) return; // Guard clause if on a page without popup

    if (status === 'login_success') {
        title.innerText = 'Welcome Back!';
        message.innerText = 'You have successfully logged in.';
    } else if (status === 'register_success') {
        title.innerText = 'Registration Successful';
        message.innerText = 'Welcome to the community! Please explore events.';
    } else if (status === 'booking_success') {
        title.innerText = 'Booking Confirmed';
        message.innerText = 'Your tickets have been reserved.';
    } else if (status === 'review_success') {
        title.innerText = 'Review Submitted';
        message.innerText = 'Thank you for your feedback.';
    } else if (status === 'inquiry_success') {
        title.innerText = 'Message Sent';
        message.innerText = 'We will get back to you shortly.';
    }

    // Show popup
    popupContainer.style.display = 'block';

    // Animate Progress Bar
    let width = 100;
    const interval = 50; 
    const totalTime = 5000; 
    const decrement = 100 / (totalTime / interval);

    const timer = setInterval(() => {
        width -= decrement;
        progressBar.style.width = width + '%';

        if (width <= 0) {
            clearInterval(timer);
            closePopup();
        }
    }, interval);
}

function closePopup() {
    const popupContainer = document.getElementById('popup-container');
    if (popupContainer) {
        popupContainer.style.animation = 'slideOut 0.3s ease-out forwards';
        setTimeout(() => {
            popupContainer.style.display = 'none';
            popupContainer.style.animation = '';
        }, 300);
    }
}

// ================= SMOOTH SCROLL =================
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');
        if (href !== '#' && href.length > 1) {
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        }
    });
});

// ================= FLOATING LABEL ANIMATION =================
document.querySelectorAll('.floating-label input, .floating-label textarea').forEach(input => {
    input.addEventListener('focus', function() {
        const label = this.nextElementSibling;
        if (label && label.tagName === 'LABEL') {
            label.style.top = '-10px';
            label.style.fontSize = '0.75rem';
            label.style.color = 'var(--primary-color)';
        }
    });
    
    input.addEventListener('blur', function() {
        if (!this.value) {
            const label = this.nextElementSibling;
            if (label && label.tagName === 'LABEL') {
                label.style.top = '50%';
                label.style.fontSize = '1rem';
                label.style.color = 'var(--text-light)';
            }
        }
    });
    
    // Check on page load
    if (input.value) {
        const label = input.nextElementSibling;
        if (label && label.tagName === 'LABEL') {
            label.style.top = '-10px';
            label.style.fontSize = '0.75rem';
            label.style.color = 'var(--primary-color)';
        }
    }
});
