// booking.js
document.addEventListener('DOMContentLoaded', () => {
    // Ensure modals are hidden on page load
    const bookingModal = document.getElementById('booking-modal');
    if (bookingModal) {
        bookingModal.style.display = 'none';
    }
    
    // Initialize seat type radio buttons styling
    const ticketRadios = document.querySelectorAll('#booking-form input[name="TicketID"]');
    ticketRadios.forEach(radio => {
        radio.addEventListener('change', updateSummary);
    });
    
    // Initialize quantity input
    const qtyInput = document.getElementById('ticket-qty');
    if (qtyInput) {
        qtyInput.addEventListener('change', updateSummary);
    }
    
    // Add form submit event listener
    const bookingForm = document.getElementById('booking-form');
    if (bookingForm) {
        bookingForm.addEventListener('submit', handleBooking);
    }
});

// Modal State
let currentEventId = null;
let quantity = 1;

// Open Booking Modal - Only called on user click
function openBookingModal(eventName, eventId, price) {
    currentEventId = eventId;
    quantity = 1;

    document.getElementById('modal-event-name').textContent = eventName;
    document.getElementById('booking-event-id').value = eventId;

    // Reset UI
    document.getElementById('ticket-qty').value = 1;
    
    // Reset radio buttons
    const ticketRadios = document.querySelectorAll('#booking-form input[name="TicketID"]');
    ticketRadios.forEach(radio => {
        radio.checked = false;
        const label = radio.closest('label');
        if (label) {
            label.querySelector('span').style.borderColor = 'transparent';
            label.querySelector('span').style.background = 'rgba(255,255,255,0.1)';
        }
    });

    // Clear special requests
    const specialReqs = document.getElementById('special-requests');
    if (specialReqs) {
        specialReqs.value = '';
    }

    updateSummary();

    const modal = document.getElementById('booking-modal');
    modal.style.display = "flex";
    document.body.style.overflow = "hidden";
}

// Close Modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "none";
        document.body.style.overflow = "auto";
    }
}

window.onclick = function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = "none";
        document.body.style.overflow = "auto";
    }
}

// Seat Selection
function selectSeatType(type) {
    currentSeatType = type;
    document.querySelectorAll('.seat-option').forEach(el => el.classList.remove('selected'));
    const selectedOption = document.querySelector(`.seat-option[data-type="${type}"]`);
    if (selectedOption) {
        selectedOption.classList.add('selected');
    }
    updateSummary();
}

// Quantity Logic
function incrementQuantity() {
    const input = document.getElementById('ticket-qty');
    if (input && parseInt(input.value) < 10) {
        input.value = parseInt(input.value) + 1;
        quantity = parseInt(input.value);
        updateSummary();
    }
}

function decrementQuantity() {
    const input = document.getElementById('ticket-qty');
    if (input && parseInt(input.value) > 1) {
        input.value = parseInt(input.value) - 1;
        quantity = parseInt(input.value);
        updateSummary();
    }
}

// Update Summary
function updateSummary() {
    const ticketRadio = document.querySelector('#booking-form input[name="TicketID"]:checked');
    const qtyInput = document.getElementById('ticket-qty');
    
    if (!qtyInput) return;
    
    quantity = parseInt(qtyInput.value) || 1;
    let price = 0;
    let seatName = '-';
    
    if (ticketRadio) {
        // Get price from the parent label's span content
        const label = ticketRadio.closest('label');
        if (label) {
            const spans = label.querySelectorAll('span');
            if (spans.length > 0) {
                seatName = spans[0].textContent.split('\n')[0].trim();
            }
        }
        
        // Set price based on ticket ID
        price = ticketRadio.value === '1' ? 50 : 100; // Standard: $50, VIP: $100
    }
    
    const subtotal = price * quantity;
    const serviceFee = 5;
    const total = subtotal + serviceFee;
    
    // Update display
    const totalPriceEl = document.getElementById('total-price');
    if (totalPriceEl) {
        totalPriceEl.textContent = total.toFixed(2);
    }
}

// Handle Booking Submission
function handleBooking(event) {
    event.preventDefault();
    event.stopPropagation();
    
    const form = document.getElementById('booking-form');
    const ticketIdInput = form.querySelector('input[name="TicketID"]:checked');
    
    if (!ticketIdInput) {
        if (typeof CustomModal !== 'undefined') {
            CustomModal.showError('Validation Error', 'Please select a seat type before booking.');
        } else {
            alert('Please select a seat type before booking.');
        }
        return false;
    }

    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';

    // Submit the form normally - let the server handle it
    // The controller will redirect to confirmation page
    form.submit();
    
    return false;
}
