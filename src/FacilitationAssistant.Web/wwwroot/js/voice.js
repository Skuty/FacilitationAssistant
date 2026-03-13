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

    // Pick a female voice if available, otherwise fall back to any male voice
    getFemaleVoice: function () {
        const voices = window.speechSynthesis.getVoices();
        return voices.find(v => /female/i.test(v.name))
            || voices.find(v => /zira|samantha|victoria|karen|moira|fiona|tessa|susan/i.test(v.name))
            || voices.find(v => /male/i.test(v.name))
            || voices[0]
            || null;
    },

    // Queue a speech utterance (queues behind any already-speaking utterance)
    speak: function (text, rate) {
        if (!window.speechSynthesis) return;
        if (!this.isSoundEnabled()) return;
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = (typeof rate === 'number') ? rate : 1.0;
        const femaleVoice = this.getFemaleVoice();
        if (femaleVoice) utterance.voice = femaleVoice;
        window.speechSynthesis.speak(utterance);
    }
};
