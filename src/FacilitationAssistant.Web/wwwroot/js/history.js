// Meeting History - localStorage management

window.meetingHistory = {
    _key: 'meeting_history',
    _maxEntries: 50,

    addEntry: function (id, title, createdAt, role, token) {
        var history = this.getHistory();

        // Remove any existing entry for the same meeting+role to avoid duplicates
        history = history.filter(function (e) {
            return !(e.id === id && e.role === role);
        });

        history.unshift({ id: id, title: title, createdAt: createdAt, role: role, token: token });

        if (history.length > this._maxEntries) {
            history = history.slice(0, this._maxEntries);
        }

        try {
            localStorage.setItem(this._key, JSON.stringify(history));
        } catch (e) {
            console.error('meetingHistory: failed to save', e);
        }
    },

    getHistory: function () {
        try {
            var stored = localStorage.getItem(this._key);
            return stored ? JSON.parse(stored) : [];
        } catch (e) {
            return [];
        }
    },

    clearHistory: function () {
        localStorage.removeItem(this._key);
    }
};
