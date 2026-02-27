// Voice reminder helper using the Web Speech API (SpeechSynthesis)

window.voiceHelper = {

    // Check if sound is enabled in user preferences
    isSoundEnabled: function () {
        const stored = localStorage.getItem('user_preferences');
        if (stored) {
            try {
                const settings = JSON.parse(stored);
                return settings.soundEnabled !== false;
            } catch (e) {
                return true;
            }
        }
        return true;
    },

    // Cancel any ongoing or queued speech
    cancel: function () {
        if (window.speechSynthesis) {
            window.speechSynthesis.cancel();
        }
    },

    // Queue a speech utterance (queues behind any already-speaking utterance)
    speak: function (text, rate) {
        if (!window.speechSynthesis) return;
        if (!this.isSoundEnabled()) return;
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = (typeof rate === 'number') ? rate : 1.0;
        window.speechSynthesis.speak(utterance);
    }
};
