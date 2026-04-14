/**
 * TruePal - Dashboard Page
 * Main entry point for the dashboard with feed and interactions
 */

document.addEventListener('DOMContentLoaded', function () {

    // ===== FILTER BUTTONS =====
    const filterButtons = document.querySelectorAll('.filter-btn');
    const postCards = document.querySelectorAll('.post-card');

    filterButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            // Update active state
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            const filterCategory = this.textContent.trim();

            // Filter posts
            postCards.forEach(card => {
                const categorySpan = card.querySelector('.post-category');
                const cardCategory = categorySpan ? categorySpan.textContent.trim() : '';

                if (filterCategory === 'All' || cardCategory === filterCategory) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    });

    // ===== POST ACTION BUTTONS =====
    const actionButtons = document.querySelectorAll('.btn-action');

    actionButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            const action = this.textContent.trim();
            const postCard = this.closest('.post-card');
            const location = postCard.querySelector('.post-location').textContent.trim();

            // Visual feedback
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);

            // Handle different actions
            if (action === 'Like') {
                handleLike(this, postCard);
            } else if (action === 'Comment') {
                handleComment(postCard, location);
            } else if (action === 'Save') {
                handleSave(this, postCard);
            }
        });
    });

    // ===== LIKE HANDLER =====
    function handleLike(button, postCard) {
        const icon = button.querySelector('i');
        const statsLike = postCard.querySelector('.post-stats span:first-child');
        const likeCount = statsLike.querySelector('i').nextSibling;
        let count = parseInt(likeCount.textContent.trim());

        // Toggle liked state
        if (icon.classList.contains('bi-heart-fill')) {
            icon.classList.remove('bi-heart-fill');
            icon.classList.add('bi-heart');
            count--;
            button.style.color = '';
        } else {
            icon.classList.remove('bi-heart');
            icon.classList.add('bi-heart-fill');
            count++;
            button.style.color = 'var(--color-primary)';
        }

        likeCount.textContent = ` ${count}`;
    }

    // ===== COMMENT HANDLER =====
    function handleComment(postCard, location) {
        // Future: Open comment modal/section
        console.log('Comment on post:', location);
    }

    // ===== SAVE HANDLER =====
    function handleSave(button, postCard) {
        const icon = button.querySelector('i');

        // Toggle saved state
        if (icon.classList.contains('bi-bookmark-fill')) {
            icon.classList.remove('bi-bookmark-fill');
            icon.classList.add('bi-bookmark');
            button.style.color = '';
        } else {
            icon.classList.remove('bi-bookmark');
            icon.classList.add('bi-bookmark-fill');
            button.style.color = 'var(--color-primary)';
        }
    }

    // ===== MINI POST CARDS (Your Posts) =====
    const miniPostCards = document.querySelectorAll('.mini-post-card');

    miniPostCards.forEach(card => {
        card.addEventListener('click', function () {
            const location = this.querySelector('.mini-post-location').textContent.trim();
            console.log('View your post:', location);
            // Future: Navigate to post detail page
        });
    });

    // ===== SAVED PLACES =====
    const savedPlaceItems = document.querySelectorAll('.saved-place-item');

    savedPlaceItems.forEach(item => {
        item.addEventListener('click', function () {
            const location = this.querySelector('.saved-place-name').textContent.trim();
            console.log('View saved place:', location);
            // Future: Navigate to location on map or show details
        });
    });

    // ===== TRENDING ITEMS =====
    const trendingItems = document.querySelectorAll('.trending-item');

    trendingItems.forEach(item => {
        item.addEventListener('click', function () {
            const location = this.querySelector('.trending-place').textContent.trim();
            console.log('View trending place:', location);
            // Future: Filter feed to show posts from this location
        });
    });

    // ===== CREATE POST BUTTON =====
    const createPostBtn = document.querySelector('.btn-create-post');

    if (createPostBtn) {
        createPostBtn.addEventListener('click', function (e) {
            e.preventDefault();
            console.log('Create new post');
            // Future: Open create post modal/page
        });
    }

});
