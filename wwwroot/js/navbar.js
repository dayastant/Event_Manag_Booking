// Consolidated Navbar Logic
document.addEventListener('DOMContentLoaded', () => {
    initializeNavbar();
});

function initializeNavbar() {
  // Navbar authentication state is handled server-side in _Layout.cshtml
  // The logged-in/logged-out states are already rendered correctly
  // This function now only handles client-side interactions
  
  const logoutBtn = document.getElementById('nav-logout-btn');

  // Logout Functionality - Not needed as logout is handled via form submission in _Layout.cshtml
  // Keeping this for any additional client-side logout logic if needed
  if (logoutBtn) {
    logoutBtn.addEventListener('click', (e) => {
      // Logout is handled by the form submission in _Layout.cshtml
      // No need for localStorage cleanup as we use server-side sessions
    });
  }

  // Settings Link Logic (in Dropdown)
  const settingsLink = document.querySelector('a[href="settings.html"]');
  if (settingsLink) {
      settingsLink.href = 'dashboard.html#settings-section'; // Update href to point to dashboard anchor
      settingsLink.addEventListener('click', (e) => {
          // If already on dashboard, manual handling might be needed depending on implementation
          if(window.location.pathname.includes('dashboard.html')) {
             e.preventDefault();
             // Logic to switch tab in dashboard.js would handle hash change if implemented correctly
             // For now, just setting location hash works if dashboard.js listens to it
             window.location.hash = 'settings-section';
             window.location.reload(); // Simple reload to force tab switch logic in dashboard.js
          }
      });
  }

  // Update active nav link
  highlightActiveLink();
  
  // Show Logout Message if present in URL
  const urlParams = new URLSearchParams(window.location.search);
  if (urlParams.get('status') === 'logout') {
      // Use a simple alert or a custom popup if available
      alert('Login to continue as you serve signed out.');
      // Clean URL
      window.history.replaceState({}, document.title, window.location.pathname);
  }
}

function highlightActiveLink() {
  const currentPage = window.location.pathname.split('/').pop() || 'index.html';
  const navLinks = document.querySelectorAll('.nav-link');
  
  navLinks.forEach(link => {
    const href = link.getAttribute('href').split('/').pop();
    if (href === currentPage) {
      link.classList.add('active');
    } else {
      link.classList.remove('active');
    }
  });
}
