// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    var root = document.documentElement;
    var btn = document.getElementById('themeToggle');
    if (!btn) return;

    function refresh() {
        var dark = root.getAttribute('data-bs-theme') === 'dark';
        btn.textContent = dark ? '☀️ Light' : '🌙 Dark';
        btn.setAttribute('aria-pressed', dark ? 'true' : 'false');
    }

    btn.addEventListener('click', function () {
        var next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        try { localStorage.setItem('it-theme', next); } catch (e) { }
        refresh();
    });

    refresh();
})();
