// Card type detection based on card number
function detectCardType(cardNumber) {
    const cleaned = cardNumber.replace(/\s/g, '');
    
    // Visa: starts with 4
    if (/^4/.test(cleaned)) {
        return { type: 'visa', icon: 'fab fa-cc-visa', color: '#1434cb' };
    }
    
    // Mastercard: starts with 51-55 or 2221-2720
    if (/^5[1-5]/.test(cleaned) || /^2(22[1-9]|2[3-9][0-9]|[3-6][0-9]{2}|7[01][0-9]|720)/.test(cleaned)) {
        return { type: 'mastercard', icon: 'fab fa-cc-mastercard', color: '#eb001b' };
    }
    
    // American Express: starts with 34 or 37
    if (/^3[47]/.test(cleaned)) {
        return { type: 'amex', icon: 'fab fa-cc-amex', color: '#006fcf' };
    }
    
    // Discover: starts with 6011, 622126-622925, 644-649, or 65
    if (/^(6011|65|64[4-9]|622)/.test(cleaned)) {
        return { type: 'discover', icon: 'fab fa-cc-discover', color: '#ff6000' };
    }
    
    return { type: 'unknown', icon: 'far fa-credit-card', color: '#6b7280' };
}

// Format card number with spaces
function formatCardNumber(input) {
    let value = input.value.replace(/\s/g, '');
    let formatted = '';
    
    for (let i = 0; i < value.length && i < 16; i++) {
        if (i > 0 && i % 4 === 0) {
            formatted += ' ';
        }
        formatted += value[i];
    }
    
    input.value = formatted;
    
    // Detect and display card type
    const cardType = detectCardType(value);
    const cardIcon = document.getElementById('cardTypeIcon');
    if (cardIcon) {
        cardIcon.innerHTML = `<i class="${cardType.icon}" style="color: ${cardType.color}; font-size: 24px;"></i>`;
    }
}

// Format expiry date (MM/YY)
function formatExpiryDate(input) {
    let value = input.value.replace(/\D/g, '');
    if (value.length >= 2) {
        value = value.slice(0, 2) + '/' + value.slice(2, 4);
    }
    input.value = value;
}

// Allow only numbers
function numbersOnly(input) {
    input.value = input.value.replace(/\D/g, '');
}
