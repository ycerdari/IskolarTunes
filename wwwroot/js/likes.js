function ttToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

async function ttPost(url) {
    return fetch(url, { method: 'POST', headers: { 'RequestVerificationToken': ttToken() } });
}

document.addEventListener('click', async (e) => {
    const btn = e.target.closest('.tt-like');
    if (!btn) return;

    btn.disabled = true;
    try {
        const res = await ttPost('/Tracks/ToggleLike/' + btn.dataset.trackId);
        if (res.ok) {
            const data = await res.json();
            btn.classList.toggle('is-liked', data.liked);
            btn.setAttribute('aria-pressed', data.liked);
            btn.querySelector('.tt-count').textContent = data.count;
        }
    } finally {
        btn.disabled = false;
    }
});

// called by the audio player in the next step
function recordPlay(trackId) {
    ttPost('/Tracks/RecordPlay/' + trackId);
}