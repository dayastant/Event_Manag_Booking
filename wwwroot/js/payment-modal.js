// ===== NEW PAYMENT MODAL FUNCTIONS =====
let selectedPaymentTab = 'credit-card';
let selectedSavedCardLast4 = null;

function proceedToPayment() {
    if (!selectedSeatType) {
        alert('Please select a seat type first.');
        return;
    }
    
    // Update payment total amount
    const total = document.getElementById('summaryTotal').textContent.replace('$', '');
    document.getElementById('paymentTotalAmount').textContent = total;
    
    // Open modal
    document.getElementById('paymentModal').classList.add('active');
}

function closePaymentModal() {
    document.getElementById('paymentModal').classList.remove('active');
    selectedSavedCardLast4 = null;
    // Reset saved card selections
    document.querySelectorAll('.saved-card').forEach(card => card.classList.remove('selected'));
}

function switchPaymentTab(tabName) {
    selectedPaymentTab = tabName;
    
    // Update tab buttons
    document.querySelectorAll('.payment-tab').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelector(`[data-tab="${tabName}"]`).classList.add('active');
    
    // Update sections
    document.querySelectorAll('.payment-section').forEach(section => {
        section.classList.remove('active');
    });
    document.getElementById(`${tabName}-section`).classList.add('active');
    
    validatePayment();
}

function selectSavedCard(cardElement, last4) {
    // Deselect all cards
    document.querySelectorAll('.saved-card').forEach(card => {
        card.classList.remove('selected');
    });
    
    // Select clicked card
    cardElement.classList.add('selected');
    selectedSavedCardLast4 = last4;
    
    validatePayment();
}

function validatePayment() {
    const terms = document.getElementById('paymentTermsCheck').checked;
    const btn = document.getElementById('confirmBookingBtn');
    
    // Enable button if terms checked and either a saved card is selected or we're on a different payment tab
    let canProceed = false;
    
    if (selectedPaymentTab === 'credit-card') {
        canProceed = terms && (selectedSavedCardLast4 !== null || isNewCardFilled());
    } else {
        // For PayPal and Bank Transfer, just need terms checked
        canProceed = terms;
    }
    
    btn.disabled = !canProceed;
}

function isNewCardFilled() {
    const cardNumber = document.getElementById('cardNumber').value;
    const cardholderName = document.getElementById('cardholderName').value;
    const expiryDate = document.getElementById('expiryDate').value;
    const cvv = document.getElementById('cvv').value;
    
    return cardNumber.length > 0 && cardholderName.length > 0 && expiryDate.length > 0 && cvv.length > 0;
}

// Add listeners to card inputs for validation
document.addEventListener('DOMContentLoaded', function() {
    ['cardNumber', 'cardholderName', 'expiryDate', 'cvv'].forEach(id => {
        const input = document.getElementById(id);
        if (input) {
            input.addEventListener('input', validatePayment);
        }
    });
    
    document.getElementById('paymentTermsCheck').addEventListener('change', validatePayment);
    
    // Close modal on outside click
    document.getElementById('paymentModal').addEventListener('click', function(e) {
        if (e.target === this) {
            closePaymentModal();
        }
    });
});

function confirmBooking() {
    // Set payment method based on selectedTab
    let paymentMethod = 'Card';
    if (selectedPaymentTab === 'paypal') {
        paymentMethod = 'PayPal';
    } else if (selectedPaymentTab === 'bank-transfer') {
        paymentMethod = 'BankTransfer';
    } else if (selectedSavedCardLast4) {
        paymentMethod = 'SavedCard-' + selectedSavedCardLast4;
    }
    
    document.getElementById('paymentMethodInput').value = paymentMethod;
    
    // Submit the main form
    document.getElementById('bookingForm').submit();
}
