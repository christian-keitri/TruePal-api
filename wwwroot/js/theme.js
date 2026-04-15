/**
 * TruePal Theme Switcher
 * Supports Dark Mode and Light Mode with localStorage persistence
 */

(function () {
    'use strict';

    const THEME_KEY = 'truepal-theme';
    const THEME_DARK = 'dark';
    const THEME_LIGHT = 'light';

    // Theme icons
    const ICONS = {
        dark: 'bi-moon-stars-fill',
        light: 'bi-sun-fill'
    };

    /**
     * Get the current theme from localStorage or default to dark
     */
    function getCurrentTheme() {
        return localStorage.getItem(THEME_KEY) || THEME_DARK;
    }

    /**
     * Set theme on the document
     */
    function setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(THEME_KEY, theme);
        updateThemeIcons(theme);
    }

    /**
     * Update theme toggle button icons
     */
    function updateThemeIcons(theme) {
        const isDark = theme === THEME_DARK;
        const icon = isDark ? ICONS.light : ICONS.dark; // Show opposite icon (what clicking will do)

        // Update desktop icon
        const desktopIcon = document.getElementById('theme-icon');
        if (desktopIcon) {
            desktopIcon.className = `bi ${icon}`;
        }

        // Update mobile icon
        const mobileIcon = document.getElementById('theme-icon-mobile');
        if (mobileIcon) {
            mobileIcon.className = `bi ${icon}`;
        }

        // Update button titles
        const title = isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode';
        const desktopBtn = document.getElementById('theme-toggle');
        const mobileBtn = document.getElementById('theme-toggle-mobile');

        if (desktopBtn) desktopBtn.setAttribute('title', title);
        if (mobileBtn) mobileBtn.setAttribute('title', title);
    }

    /**
     * Toggle between dark and light themes
     */
    function toggleTheme() {
        const currentTheme = getCurrentTheme();
        const newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;
        setTheme(newTheme);
    }

    /**
     * Initialize theme on page load
     */
    function initTheme() {
        const savedTheme = getCurrentTheme();
        setTheme(savedTheme);

        // Add click handlers for both desktop and mobile toggle buttons
        const desktopToggle = document.getElementById('theme-toggle');
        const mobileToggle = document.getElementById('theme-toggle-mobile');

        if (desktopToggle) {
            desktopToggle.addEventListener('click', toggleTheme);
        }

        if (mobileToggle) {
            mobileToggle.addEventListener('click', toggleTheme);
        }
    }

    // Initialize theme as early as possible to prevent flash
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTheme);
    } else {
        initTheme();
    }

    // Also set theme immediately (before DOM ready) to prevent flash
    const savedTheme = localStorage.getItem(THEME_KEY) || THEME_DARK;
    document.documentElement.setAttribute('data-theme', savedTheme);
})();
