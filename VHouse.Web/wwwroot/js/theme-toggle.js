// VHouse Dark Mode Toggle
// Por los animales. Por la liberación. Por un mundo sin sufrimiento.

document.addEventListener('DOMContentLoaded', function() {
    // Create dark mode toggle button
    const toggleButton = document.createElement('button');
    toggleButton.className = 'dark-mode-toggle';
    toggleButton.innerHTML = '🌙';
    toggleButton.setAttribute('aria-label', 'Toggle Dark Mode');
    toggleButton.setAttribute('title', 'Cambiar Tema');
    
    document.body.appendChild(toggleButton);
    
    // Get stored theme preference or default to light
    const storedTheme = localStorage.getItem('vhouse-theme') || 'light';
    
    // Apply stored theme on page load
    applyTheme(storedTheme);
    
    // Toggle theme on button click
    toggleButton.addEventListener('click', function() {
        const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        
        applyTheme(newTheme);
        localStorage.setItem('vhouse-theme', newTheme);
        
        // Add click animation
        this.style.transform = 'scale(0.9)';
        setTimeout(() => {
            this.style.transform = '';
        }, 150);
    });
    
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        // Update button icon
        if (theme === 'dark') {
            toggleButton.innerHTML = '☀️';
            toggleButton.setAttribute('title', 'Modo Claro');
        } else {
            toggleButton.innerHTML = '🌙';
            toggleButton.setAttribute('title', 'Modo Oscuro');
        }
        
        // Animate theme transition
        document.body.style.transition = 'background-color 0.3s, color 0.3s';
        setTimeout(() => {
            document.body.style.transition = '';
        }, 300);
    }
    
    // Add keyboard support
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + D to toggle theme
        if ((e.ctrlKey || e.metaKey) && e.key === 'd') {
            e.preventDefault();
            toggleButton.click();
        }
    });
});