// ===== Theme toggle =====
(function () {
    var root = document.documentElement;
    var btn = document.getElementById('themeToggle');
    if (!btn) return;

    function refresh() {
        var dark = root.getAttribute('data-bs-theme') === 'dark';
        var label = dark ? 'Switch to light mode' : 'Switch to dark mode';
        btn.setAttribute('aria-label', label);
        btn.setAttribute('title', label);
    }

    btn.addEventListener('click', function () {
        var next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        try { localStorage.setItem('it-theme', next); } catch (e) { }
        refresh();
    });

    refresh();
})();

// ===== Home player (only runs when the Home page has tracks) =====
(function () {
    var app = document.getElementById('itPlayer');
    if (!app) return;

    var tracks = Array.prototype.map.call(app.querySelectorAll('#itQueue li'), function (li) {
        return {
            id: parseInt(li.getAttribute('data-id'), 10),
            title: li.getAttribute('data-title') || '',
            artist: li.getAttribute('data-artist') || '',
            genre: li.getAttribute('data-genre') || '',
            liked: li.getAttribute('data-liked') === '1',
            likes: parseInt(li.getAttribute('data-likes'), 10) || 0
        };
    });
    if (!tracks.length) return;

    var tokenInput = app.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenInput ? tokenInput.value : '';
    var audioUrl = app.getAttribute('data-audio-url');
    var playUrl = app.getAttribute('data-play-url');
    var likeUrl = app.getAttribute('data-like-url');

    var audio = new Audio();
    audio.preload = 'none';

    var cur = 0, playing = false, shuffle = false, failed = false, recordedFor = null;
    var start = parseInt(app.getAttribute('data-start'), 10);
    for (var s = 0; s < tracks.length; s++) { if (tracks[s].id === start) cur = s; }

    function $(id) { return document.getElementById(id); }
    function setText(id, text) { var el = $(id); if (el) el.textContent = text; }
    function make(tag, cls) { var el = document.createElement(tag); if (cls) el.className = cls; return el; }
    function fmt(sec) {
        var m = Math.floor(sec / 60), r = Math.floor(sec % 60);
        return m + ':' + (r < 10 ? '0' : '') + r;
    }
    function hasDuration() { return isFinite(audio.duration) && audio.duration > 0; }

    function postForm(url, id) {
        var body = new URLSearchParams();
        body.set('__RequestVerificationToken', token);
        body.set('id', id);
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString(),
            credentials: 'same-origin'
        });
    }

    function nextIndex() {
        if (tracks.length === 1) return 0;
        if (shuffle) {
            var r;
            do { r = Math.floor(Math.random() * tracks.length); } while (r === cur);
            return r;
        }
        return (cur + 1) % tracks.length;
    }

    function tryPlay() {
        var p = audio.play();
        if (p && p.catch) p.catch(function () { playing = false; failed = true; render(); });
    }

    function goTo(i, autoplay) {
        cur = i;
        failed = false;
        recordedFor = null;
        audio.src = audioUrl + tracks[i].id;
        updateProgress();
        if (autoplay) tryPlay();
        render();
    }

    function toggle() {
        if (!audio.src) { goTo(cur, true); return; }
        if (audio.paused) tryPlay(); else audio.pause();
    }

    function playById(id) {
        for (var i = 0; i < tracks.length; i++) {
            if (tracks[i].id === id) {
                if (i === cur && audio.src) toggle(); else goTo(i, true);
                return;
            }
        }
    }

    function prev() {
        if (audio.src && audio.currentTime > 3) { audio.currentTime = 0; return; }
        goTo((cur - 1 + tracks.length) % tracks.length, true);
    }

    function like() {
        var t = tracks[cur];
        postForm(likeUrl, t.id)
            .then(function (r) { if (!r.ok) throw new Error('like failed'); return r.json(); })
            .then(function (d) { t.liked = !!d.liked; t.likes = d.count; render(); })
            .catch(function () { });
    }

    function recordPlay() {
        var t = tracks[cur];
        if (recordedFor === t.id) return;
        recordedFor = t.id;
        postForm(playUrl, t.id).catch(function () { });
    }

    var seek = $('itSeek');
    function updateProgress() {
        var pct = hasDuration() ? (audio.currentTime / audio.duration) * 100 : 0;
        if (seek) { seek.value = pct; seek.style.setProperty('--p', pct + '%'); }
        setText('itTime', fmt(audio.currentTime || 0));
        setText('itDur', hasDuration() ? fmt(audio.duration) : '0:00');
    }

    function renderStops() {
        var host = $('itStops');
        if (!host) return;
        var n = Math.min(5, tracks.length);
        var w0 = Math.max(0, Math.min(cur - 2, tracks.length - n));

        host.textContent = '';
        var inner = make('div', 'it-stops-inner');
        inner.style.setProperty('--n', n);

        var line = make('div', 'it-stops-line');
        var fill = make('div', 'it-stops-fill');
        fill.style.width = (n > 1 ? ((cur - w0) / (n - 1)) * 100 : 0) + '%';
        line.appendChild(fill);
        inner.appendChild(line);

        var row = make('div', 'it-stops-row' + (n === 1 ? ' single' : ''));
        for (var j = 0; j < n; j++) {
            (function (i) {
                var t = tracks[i];
                var b = make('button', 'it-stop' + (i === cur ? ' here' : '') + (i <= cur ? ' done' : ''));
                b.type = 'button';
                b.setAttribute('aria-label', 'Play ' + t.title);

                var top = make('span', 'it-stop-top');
                if (i === cur) {
                    var jeep = make('span', 'it-jeep');
                    jeep.innerHTML = '<svg width="44" height="30" viewBox="0 0 40 26" aria-hidden="true">' +
                        '<rect x="1" y="3" width="38" height="16" rx="5" class="it-jeep-body"></rect>' +
                        '<rect x="5" y="6" width="8" height="6" rx="1" class="it-jeep-win"></rect>' +
                        '<rect x="15" y="6" width="8" height="6" rx="1" class="it-jeep-win"></rect>' +
                        '<rect x="25" y="6" width="9" height="6" rx="1" class="it-jeep-win"></rect>' +
                        '<circle cx="11" cy="21" r="4" fill="#222"></circle>' +
                        '<circle cx="30" cy="21" r="4" fill="#222"></circle></svg>';
                    top.appendChild(jeep);
                }
                b.appendChild(top);
                b.appendChild(make('span', 'it-stop-dot'));

                var name = make('span', 'it-stop-name');
                name.textContent = t.title;
                b.appendChild(name);

                var k = make('span', 'it-stop-k');
                k.textContent = 'Stop ' + (i + 1);
                b.appendChild(k);

                b.addEventListener('click', function () { goTo(i, true); });
                row.appendChild(b);
            })(w0 + j);
        }
        inner.appendChild(row);
        host.appendChild(inner);
    }

    function render() {
        var t = tracks[cur];
        var status = failed ? 'UNAVAILABLE' : (playing ? 'NOW PLAYING' : 'PAUSED');
        setText('itTicker', status + ' ► ' + t.title);

        var sub = t.artist + (t.genre ? ' · ' + t.genre : '');
        if (audio.src && hasDuration()) sub += ' · ' + fmt(audio.duration);
        setText('itTickerSub', sub);

        setText('itDockTitle', t.title);
        setText('itDockArtist', t.artist);

        var label = playing ? 'Pause' : 'Play';
        Array.prototype.forEach.call(document.querySelectorAll('[data-action="toggle"]'), function (b) {
            b.setAttribute('aria-label', label);
        });
        Array.prototype.forEach.call(document.querySelectorAll('[data-label="toggle"]'), function (el) {
            el.textContent = label;
        });
        Array.prototype.forEach.call(document.querySelectorAll('[data-action="like"]'), function (b) {
            b.classList.toggle('on', t.liked);
            b.setAttribute('aria-pressed', t.liked ? 'true' : 'false');
            b.setAttribute('aria-label', t.liked ? 'Remove from favorites' : 'Add to favorites');
        });
        Array.prototype.forEach.call(document.querySelectorAll('[data-action="shuffle"]'), function (b) {
            b.classList.toggle('on', shuffle);
            b.setAttribute('aria-pressed', shuffle ? 'true' : 'false');
        });

        document.body.classList.toggle('it-is-playing', playing);
        renderStops();
    }

    audio.addEventListener('play', function () { playing = true; failed = false; render(); });
    audio.addEventListener('playing', recordPlay);
    audio.addEventListener('pause', function () { playing = false; render(); });
    audio.addEventListener('ended', function () { goTo(nextIndex(), true); });
    audio.addEventListener('timeupdate', updateProgress);
    audio.addEventListener('loadedmetadata', function () { updateProgress(); render(); });
    audio.addEventListener('error', function () { playing = false; failed = true; render(); });

    if (seek) {
        seek.addEventListener('input', function () {
            if (hasDuration()) audio.currentTime = (seek.value / 100) * audio.duration;
        });
    }

    document.addEventListener('click', function (e) {
        var playEl = e.target.closest('[data-play]');
        if (playEl) { playById(parseInt(playEl.getAttribute('data-play'), 10)); return; }

        var act = e.target.closest('[data-action]');
        if (!act) return;
        switch (act.getAttribute('data-action')) {
            case 'toggle': toggle(); break;
            case 'next': goTo(nextIndex(), true); break;
            case 'prev': prev(); break;
            case 'shuffle': shuffle = !shuffle; render(); break;
            case 'like': like(); break;
        }
    });

    document.body.classList.add('it-has-player');
    updateProgress();
    render();
})();