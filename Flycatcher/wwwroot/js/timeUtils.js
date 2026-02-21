window.timeUtils = {
    getUserTimeZone: function () {
        return Intl.DateTimeFormat().resolvedOptions().timeZone;
    },
    formatUtcToLocal: function (utcDateString) {
        if (!utcDateString) return '';
        // Parse as UTC
        const date = new Date(utcDateString + (utcDateString.endsWith('Z') ? '' : 'Z'));
        return date.toLocaleString(undefined, {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    },
    formatUtcToLocalShort: function (utcDateString) {
        if (!utcDateString) return '';
        const date = new Date(utcDateString + (utcDateString.endsWith('Z') ? '' : 'Z'));
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        if (diffMins < 1) return 'just now';
        if (diffMins < 60) return diffMins + 'm ago';
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours < 24) return diffHours + 'h ago';
        return date.toLocaleString(undefined, {
            month: 'short', day: 'numeric',
            hour: 'numeric', minute: '2-digit', hour12: true
        });
    }
};
