(() => {
    const toggles = document.querySelectorAll('[data-nav-toggle]');
    function setMenu(id, open) {
        const menu = document.getElementById(id);
        if (!menu) return;
        menu.classList.toggle('is-open', open);
        document.querySelectorAll(`[data-nav-toggle="${id}"]`).forEach(button => {
            button.setAttribute('aria-expanded', String(open));
        });
        document.querySelectorAll(`[data-nav-close="${id}"]`).forEach(scrim => {
            scrim.classList.toggle('is-open', open);
        });
    }
    toggles.forEach(button => button.addEventListener('click', () => {
        setMenu(button.dataset.navToggle, button.getAttribute('aria-expanded') !== 'true');
    }));
    document.querySelectorAll('[data-nav-close]').forEach(button => {
        button.addEventListener('click', () => setMenu(button.dataset.navClose, false));
    });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        toggles.forEach(button => {
            if (button.getAttribute('aria-expanded') === 'true') {
                setMenu(button.dataset.navToggle, false);
                button.focus();
            }
        });
    });
    document.querySelectorAll('#site-navigation a').forEach(link => {
        link.addEventListener('click', () => setMenu('site-navigation', false));
    });
})();
