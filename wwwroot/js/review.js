// Review functionality for Events page
// This file handles review modals and AJAX submissions

// Ensure modals are hidden on page load
document.addEventListener('DOMContentLoaded', () => {
    const reviewModal = document.getElementById('review-modal');
    if (reviewModal) {
        reviewModal.style.display = 'none';
    }
    
    const viewReviewsModal = document.getElementById('view-reviews-modal');
    if (viewReviewsModal) {
        viewReviewsModal.style.display = 'none';
    }
});

// Wrapper function for opening review modal - Only called on user click
function openReviewModal(eventName, eventId) {
    const modal = document.getElementById('review-modal');
    if (modal) {
        modal.style.display = 'flex';
        
        const eventNameEl = document.getElementById('review-event-name');
        if (eventNameEl) {
            eventNameEl.textContent = eventName;
        }
        
        document.getElementById('review-event-id').value = eventId;
        document.body.style.overflow = 'hidden';
        
        // Reset form
        const form = document.getElementById('review-form');
        if (form) {
            form.reset();
        }
        currentRating = 0;
        resetStars();
    }
}

// Handle review form submission - No client-side validation, rely on server
async function handleReview(event) {
    event.preventDefault();
    
    const form = event.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Submitting...';

    const reviewData = {
        EventID: parseInt(document.getElementById('review-event-id').value),
        Rating: parseInt(document.getElementById('rating-value').value) || 0,
        Comment: document.getElementById('review-text').value || ''
    };

    try {
        const response = await fetch('/Review/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(reviewData)
        });

        const result = await response.json();

        if (result.success) {
            alert(result.message);
            closeModal('review-modal');
            // Reset form
            form.reset();
            currentRating = 0;
            resetStars();
            // Optionally reload to show new review
            window.location.reload();
        } else {
            // Display server-side validation error
            alert(result.message || 'Failed to submit review. Please try again.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred. Please try again.');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// View reviews for an event
async function viewReviews(eventName, eventId) {
    const modal = document.getElementById('view-reviews-modal');
    if (!modal) return;
    
    document.getElementById('reviews-event-name').textContent = eventName;
    modal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
    
    const reviewsList = document.getElementById('reviews-list');
    const avgRatingEl = document.getElementById('avg-rating');
    
    reviewsList.innerHTML = '<p style="text-align: center; padding: 20px;">Loading reviews...</p>';
    
    try {
        const response = await fetch(`/Review/GetReviews?eventId=${eventId}`);
        const result = await response.json();
        
        if (result.success) {
            avgRatingEl.textContent = result.averageRating || '0.0';
            
            if (result.reviews.length === 0) {
                reviewsList.innerHTML = `
                    <div style="text-align: center; padding: 60px 20px;">
                        <i class="far fa-comment-dots" style="font-size: 4rem; color: #ddd; margin-bottom: 20px;"></i>
                        <p style="color: #999; font-size: 1.1rem;">No reviews yet. Be the first to review!</p>
                    </div>
                `;
            } else {
                reviewsList.innerHTML = result.reviews.map(review => {
                    const stars = '★'.repeat(review.rating || 0) + '☆'.repeat(5 - (review.rating || 0));
                    return `
                        <div style="background: linear-gradient(135deg, #f9f9f9, #ffffff); padding: 25px; border-radius: 15px; margin-bottom: 20px; border-left: 4px solid var(--primary-color);">
                            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
                                <div>
                                    <strong style="font-size: 1.1rem; color: var(--text-dark);">${review.memberName}</strong>
                                    <div style="color: #FFD700; font-size: 1.2rem; margin-top: 5px;">${stars}</div>
                                </div>
                                <small style="color: #999; font-size: 0.85rem;"><i class="far fa-calendar"></i> ${review.reviewDate}</small>
                            </div>
                            <p style="color: #666; margin: 0; line-height: 1.7; font-size: 0.95rem;">${review.comment || 'No comment'}</p>
                        </div>
                    `;
                }).join('');
            }
        } else {
            reviewsList.innerHTML = '<p style="text-align: center; padding: 20px; color: #999;">Failed to load reviews</p>';
        }
    } catch (error) {
        console.error('Error:', error);
        reviewsList.innerHTML = '<p style="text-align: center; padding: 20px; color: #999;">Error loading reviews</p>';
    }
}

// Star rating functions
let currentRating = 0;

function setRating(val) {
    currentRating = val;
    const ratingInput = document.getElementById('rating-value');
    if (ratingInput) ratingInput.value = val;
    
    const stars = document.querySelectorAll('#review-stars i, .star-rating i');
    stars.forEach((star, index) => {
        if (index < val) {
            star.classList.remove('far');
            star.classList.add('fas');
        } else {
            star.classList.remove('fas');
            star.classList.add('far');
        }
    });
}

function hoverRating(val) {
    const stars = document.querySelectorAll('#review-stars i, .star-rating i');
    stars.forEach((star, index) => {
        if (index < val) {
            star.classList.remove('far');
            star.classList.add('fas');
        } else {
            star.classList.remove('fas');
            star.classList.add('far');
        }
    });
}

function resetHover() {
    if (currentRating > 0) {
        setRating(currentRating);
    } else {
        resetStars();
    }
}

function resetStars() {
    const stars = document.querySelectorAll('#review-stars i, .star-rating i');
    stars.forEach(star => {
        star.classList.remove('fas');
        star.classList.add('far');
    });
}

// Character counter for review text
function updateCharCount() {
    const textarea = document.getElementById('review-text');
    const charCount = document.getElementById('char-count');
    if (textarea && charCount) {
        const currentLength = textarea.value.length;
        const maxLength = 500;
        charCount.innerText = `${currentLength} / ${maxLength}`;
        
        if (currentLength > maxLength * 0.9) {
            charCount.style.color = '#ff6b6b';
        } else {
            charCount.style.opacity = '0.7';
            charCount.style.color = 'inherit';
        }
    }
}

// Close modal utility
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

// Close modals when clicking outside
window.addEventListener('click', function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
});
