// Lightweight, dependency-free sound effects for the chess board.
// Uses the Web Audio API to synthesize short tones instead of loading
// external audio files (no assets to host, works offline).
window.chessSounds = (function () {
    let ctx = null;

    function getCtx() {
        if (!ctx) {
            ctx = new (window.AudioContext || window.webkitAudioContext)();
        }
        // Browsers require a user gesture before audio can play; resuming
        // here is a no-op if it's already running.
        if (ctx.state === "suspended") {
            ctx.resume();
        }
        return ctx;
    }

    function tone(freq, duration, type, delayMs) {
        delayMs = delayMs || 0;
        setTimeout(function () {
            const c = getCtx();
            const osc = c.createOscillator();
            const gain = c.createGain();
            osc.type = type || "sine";
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0.15, c.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, c.currentTime + duration);
            osc.connect(gain);
            gain.connect(c.destination);
            osc.start();
            osc.stop(c.currentTime + duration);
        }, delayMs);
    }

    const sounds = {
        move: function () {
            tone(440, 0.09, "sine");
        },
        capture: function () {
            tone(220, 0.11, "square");
            tone(160, 0.11, "square", 60);
        },
        castle: function () {
            tone(392, 0.09, "sine");
            tone(523, 0.12, "sine", 90);
        },
        check: function () {
            tone(660, 0.13, "triangle");
            tone(880, 0.13, "triangle", 110);
        },
        gameOver: function () {
            tone(523, 0.15, "sine");
            tone(392, 0.15, "sine", 150);
            tone(261, 0.3, "sine", 300);
        },
        tick: function () {
            tone(1000, 0.03, "square");
        }
    };

    return {
        // Play.razor calls chessSounds.play("move" | "capture" | "castle" | "check" | "gameOver")
        play: function (type) {
            var fn = sounds[type];
            if (fn) fn();
        }
    };
})();