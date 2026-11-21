(() => {
    const gallery = document.querySelector('[data-property-gallery]');
    if (gallery) {
        const mainImage = gallery.querySelector('[data-gallery-main]');
        const openImage = gallery.querySelector('[data-gallery-open]');
        const thumbnails = gallery.querySelectorAll('[data-gallery-thumb]');
        thumbnails.forEach(link => link.addEventListener('click', event => {
            if (!mainImage || !openImage || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || event.button !== 0) return;
            event.preventDefault();
            mainImage.src = link.href;
            mainImage.alt = link.querySelector('img')?.alt || 'Ảnh bất động sản';
            openImage.href = link.href;
            thumbnails.forEach(item => {
                item.classList.toggle('is-selected', item === link);
                if (item === link) item.setAttribute('aria-current', 'true');
                else item.removeAttribute('aria-current');
            });
        }));
    }
    const copyButton = document.querySelector('[data-copy-listing]');
    const feedback = document.querySelector('[data-copy-feedback]');
    if (copyButton && feedback) {
        copyButton.addEventListener('click', async () => {
            try {
                if (!navigator.clipboard?.writeText) throw new Error('Clipboard unavailable');
                const url = new URL(window.location.href);
                url.hash = '';
                await navigator.clipboard.writeText(url.href);
                feedback.textContent = 'Đã sao chép liên kết tin.';
            } catch {
                feedback.textContent = 'Bạn có thể sao chép địa chỉ trên thanh trình duyệt.';
            }
        });
    }
})();
