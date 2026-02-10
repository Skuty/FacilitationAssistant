// User Settings & Preferences JavaScript Interop

window.settingsHelper = {
    // Default settings
    getDefaults: function () {
        return {
            theme: 'system',
            soundEnabled: true,
            toastEnabled: true,
            reducedMotion: false,
            highContrast: false
        };
    },

    // Load settings from localStorage
    loadSettings: function () {
        const stored = localStorage.getItem('user_preferences');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.error('Failed to parse settings:', e);
                return this.getDefaults();
            }
        }
        return this.getDefaults();
    },

    // Save settings to localStorage
    saveSettings: function (settings) {
        try {
            localStorage.setItem('user_preferences', JSON.stringify(settings));
            return true;
        } catch (e) {
            console.error('Failed to save settings:', e);
            return false;
        }
    },

    // Apply theme to the document
    applyTheme: function (theme) {
        const root = document.documentElement;
        
        if (theme === 'system') {
            // Use system preference
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            root.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
        } else {
            root.setAttribute('data-theme', theme);
        }
    },

    // Check if user has OS-level reduced motion preference
    hasOSReducedMotion: function () {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    },

    // Initialize theme on page load
    initializeTheme: function () {
        const settings = this.loadSettings();
        this.applyTheme(settings.theme);
        
        // Respect OS-level reduced motion
        if (this.hasOSReducedMotion()) {
            document.documentElement.classList.add('reduced-motion');
        }
    }
};

// Initialize theme immediately on page load
window.settingsHelper.initializeTheme();

// Listen for system theme changes
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    const settings = window.settingsHelper.loadSettings();
    if (settings.theme === 'system') {
        window.settingsHelper.applyTheme('system');
    }
});
