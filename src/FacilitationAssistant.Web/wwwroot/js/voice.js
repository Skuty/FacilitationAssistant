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
    },

    // Play N short beep dots using Web Audio API for stage time-remaining alerts:
    //   1 dot  = 10% of time remaining
    //   2 dots =  5% of time remaining
    //   3 dots =  0% of time remaining (time is up)
    playDotSignal: function (dots) {
        if (!this.isSoundEnabled()) return;
        var AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) return;
        var ctx = new AudioCtx();
        var dotDuration = 0.12;   // seconds each beep lasts
        var gapDuration = 0.10;   // seconds of silence between beeps
        for (var i = 0; i < dots; i++) {
            var startTime = ctx.currentTime + i * (dotDuration + gapDuration);
            var osc = ctx.createOscillator();
            var gain = ctx.createGain();
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.type = 'sine';
            osc.frequency.value = 880;
            gain.gain.setValueAtTime(0.5, startTime);
            gain.gain.exponentialRampToValueAtTime(0.001, startTime + dotDuration);
            osc.start(startTime);
            osc.stop(startTime + dotDuration);
        }
        // Close AudioContext after all dots have played
        setTimeout(function () { ctx.close(); }, (dots * (dotDuration + gapDuration) + 0.2) * 1000);
    }
};
