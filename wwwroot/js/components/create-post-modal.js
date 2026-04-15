/**
 * Create Post Modal Component
 * Handles post creation functionality with form validation and API calls
 */

(function () {
    'use strict';

    // ===== INITIALIZATION =====
    document.addEventListener('DOMContentLoaded', function () {
        initCreatePostModal();
    });

    /**
     * Initialize the create post modal
     */
    function initCreatePostModal() {
        const modal = document.getElementById('createPostModal');
        if (!modal) return;

        const form = document.getElementById('createPostForm');
        const submitBtn = document.getElementById('submitPostBtn');
        const contentTextarea = document.getElementById('postContent');
        const charCount = document.getElementById('charCount');
        const errorAlert = document.getElementById('createPostError');
        const errorMessage = document.getElementById('createPostErrorMessage');
        const successAlert = document.getElementById('createPostSuccess');

        // ===== CHARACTER COUNTER =====
        if (contentTextarea && charCount) {
            contentTextarea.addEventListener('input', function () {
                const count = this.value.length;
                charCount.textContent = count;

                // Visual feedback for character limit
                if (count > 450) {
                    charCount.style.color = 'var(--color-warning)';
                } else if (count > 490) {
                    charCount.style.color = 'var(--color-danger)';
                } else {
                    charCount.style.color = 'var(--color-primary)';
                }
            });
        }

        // ===== FORM SUBMISSION =====
        if (form) {
            form.addEventListener('submit', async function (e) {
                e.preventDefault();

                // Hide previous messages
                hideAlert(errorAlert);
                hideAlert(successAlert);

                // Validate form
                if (!form.checkValidity()) {
                    form.classList.add('was-validated');
                    return;
                }

                // Get form data
                const formData = {
                    content: document.getElementById('postContent').value.trim(),
                    location: document.getElementById('postLocation').value.trim() || null,
                    imageUrl: document.getElementById('postImageUrl').value.trim() || null
                };

                // Validate content length
                if (formData.content.length < 1 || formData.content.length > 500) {
                    showError('Post content must be between 1 and 500 characters.');
                    return;
                }

                // Disable submit button
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Posting...';

                try {
                    // Call API
                    const response = await fetch('/api/posts', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(formData),
                        credentials: 'include' // Include cookies for authentication
                    });

                    const data = await response.json();

                    if (!response.ok) {
                        // Handle validation errors
                        if (response.status === 400 && data.errors) {
                            const errors = Object.values(data.errors).flat().join(' ');
                            showError(errors);
                        } else if (response.status === 401) {
                            showError('You must be logged in to create a post.');
                        } else {
                            showError(data.error || 'Failed to create post. Please try again.');
                        }
                        return;
                    }

                    // Success!
                    showSuccess();

                    // Reset form
                    form.reset();
                    form.classList.remove('was-validated');
                    if (charCount) charCount.textContent = '0';

                    // Close modal after 1.5 seconds
                    setTimeout(() => {
                        const bootstrapModal = bootstrap.Modal.getInstance(modal);
                        if (bootstrapModal) {
                            bootstrapModal.hide();
                        }

                        // Reload page to show new post
                        setTimeout(() => {
                            window.location.reload();
                        }, 300);
                    }, 1500);

                } catch (error) {
                    console.error('Error creating post:', error);
                    showError('Network error. Please check your connection and try again.');
                } finally {
                    // Re-enable submit button
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="bi bi-send me-2"></i>Post';
                }
            });
        }

        // ===== MODAL RESET ON CLOSE =====
        modal.addEventListener('hidden.bs.modal', function () {
            if (form) {
                form.reset();
                form.classList.remove('was-validated');
            }
            if (charCount) {
                charCount.textContent = '0';
                charCount.style.color = 'var(--color-primary)';
            }
            hideAlert(errorAlert);
            hideAlert(successAlert);
        });

        // ===== HELPER FUNCTIONS =====
        function showError(message) {
            if (errorMessage) errorMessage.textContent = message;
            showAlert(errorAlert);
        }

        function showSuccess() {
            showAlert(successAlert);
        }

        function showAlert(alert) {
            if (alert) {
                alert.classList.remove('d-none');
            }
        }

        function hideAlert(alert) {
            if (alert) {
                alert.classList.add('d-none');
            }
        }
    }

})();
