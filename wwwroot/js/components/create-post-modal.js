/**
 * Create Post Modal Component
 * Handles post creation functionality with form validation and API calls
 */

(function () {
    'use strict';

    // ===== CONFIGURATION =====
    const FILE_UPLOAD_CONFIG = {
        VALID_TYPES: ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'],
        MAX_SIZE: 5 * 1024 * 1024, // 5MB in bytes
        MAX_SIZE_TEXT: '5MB'
    };

    const CHAR_LIMIT = {
        MAX: 500,
        WARNING: 450,
        DANGER: 490
    };

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

        // File upload elements
        const fileInput = document.getElementById('postImageFile');
        const imagePreview = document.getElementById('imagePreview');
        const imagePreviewContainer = document.getElementById('imagePreviewContainer');
        const removeImageBtn = document.getElementById('removeImageBtn');
        const uploadTabBtn = document.getElementById('upload-tab');
        const urlTabBtn = document.getElementById('url-tab');

        let uploadedFilePath = null;

        // ===== CHARACTER COUNTER =====
        if (contentTextarea && charCount) {
            contentTextarea.addEventListener('input', function () {
                const count = this.value.length;
                charCount.textContent = count;

                // Visual feedback for character limit
                if (count > CHAR_LIMIT.DANGER) {
                    charCount.style.color = 'var(--color-danger)';
                } else if (count > CHAR_LIMIT.WARNING) {
                    charCount.style.color = 'var(--color-warning)';
                } else {
                    charCount.style.color = 'var(--color-primary)';
                }
            });
        }

        // ===== FILE UPLOAD HANDLING =====
        if (fileInput) {
            fileInput.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (!file) return;

                // Validate file type
                if (!FILE_UPLOAD_CONFIG.VALID_TYPES.includes(file.type)) {
                    showError('Please select a valid image file (JPG, PNG, GIF, or WebP).');
                    fileInput.value = '';
                    return;
                }

                // Validate file size
                if (file.size > FILE_UPLOAD_CONFIG.MAX_SIZE) {
                    showError(`Image file size must not exceed ${FILE_UPLOAD_CONFIG.MAX_SIZE_TEXT}.`);
                    fileInput.value = '';
                    return;
                }

                // Show preview
                const reader = new FileReader();
                reader.onload = function (e) {
                    if (imagePreview) {
                        imagePreview.src = e.target.result;
                    }
                    if (imagePreviewContainer) {
                        imagePreviewContainer.classList.remove('d-none');
                    }
                };
                reader.readAsDataURL(file);
            });
        }

        // Remove image button
        if (removeImageBtn) {
            removeImageBtn.addEventListener('click', function () {
                if (fileInput) fileInput.value = '';
                if (imagePreview) imagePreview.src = '';
                if (imagePreviewContainer) imagePreviewContainer.classList.add('d-none');
                uploadedFilePath = null;
            });
        }

        // Tab switching - clear file data when switching to URL tab
        if (urlTabBtn) {
            urlTabBtn.addEventListener('click', function () {
                if (fileInput) fileInput.value = '';
                if (imagePreview) imagePreview.src = '';
                if (imagePreviewContainer) imagePreviewContainer.classList.add('d-none');
                uploadedFilePath = null;
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
                const content = document.getElementById('postContent').value.trim();
                const location = document.getElementById('postLocation').value.trim() || null;
                const imageUrl = document.getElementById('postImageUrl').value.trim() || null;

                // Validate content length
                if (content.length < 1 || content.length > CHAR_LIMIT.MAX) {
                    showError(`Post content must be between 1 and ${CHAR_LIMIT.MAX} characters.`);
                    return;
                }

                // Disable submit button
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Posting...';

                try {
                    let finalImageUrl = imageUrl;

                    // Upload file if selected
                    if (fileInput && fileInput.files && fileInput.files[0]) {
                        const file = fileInput.files[0];
                        const uploadFormData = new FormData();
                        uploadFormData.append('file', file);

                        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Uploading image...';

                        const uploadResponse = await fetch('/api/files/upload', {
                            method: 'POST',
                            body: uploadFormData,
                            credentials: 'include'
                        });

                        if (!uploadResponse.ok) {
                            const uploadData = await uploadResponse.json();
                            if (uploadResponse.status === 400 && uploadData.errors) {
                                const errors = uploadData.errors.join(' ');
                                showError(errors);
                            } else if (uploadResponse.status === 401) {
                                showError('You must be logged in to upload images.');
                            } else {
                                showError(uploadData.error || 'Failed to upload image. Please try again.');
                            }
                            return;
                        }

                        const uploadData = await uploadResponse.json();
                        finalImageUrl = uploadData.filePath;
                        uploadedFilePath = finalImageUrl;

                        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating post...';
                    }

                    // Create post data
                    const formData = {
                        content: content,
                        location: location,
                        imageUrl: finalImageUrl
                    };

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
                    if (imagePreview) imagePreview.src = '';
                    if (imagePreviewContainer) imagePreviewContainer.classList.add('d-none');
                    uploadedFilePath = null;

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
            if (fileInput) fileInput.value = '';
            if (imagePreview) imagePreview.src = '';
            if (imagePreviewContainer) imagePreviewContainer.classList.add('d-none');
            uploadedFilePath = null;
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
