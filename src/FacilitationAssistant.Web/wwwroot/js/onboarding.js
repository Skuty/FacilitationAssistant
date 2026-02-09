// Onboarding Tour JavaScript Interop

window.onboardingHelper = {
    // Check if a tour has been completed
    hasSeen: function (tourKey) {
        return localStorage.getItem(tourKey) === 'true';
    },

    // Mark a tour as completed
    markAsSeen: function (tourKey) {
        localStorage.setItem(tourKey, 'true');
    },

    // Reset a tour (for manual restart via help icon)
    resetTour: function (tourKey) {
        localStorage.removeItem(tourKey);
    },

    // Reset all tours
    resetAllTours: function () {
        localStorage.removeItem('facilitator-tour-seen');
        localStorage.removeItem('attendee-tour-seen');
    }
};
