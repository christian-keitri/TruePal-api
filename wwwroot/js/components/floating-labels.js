/**
 * Floating Labels Component
 * Handles floating label animations for form inputs
 */
(function () {
    'use strict';

    function initFloatingLabels() {
        const inputs = document.querySelectorAll('.floating-label-group input');

        inputs.forEach(input => {
            // Check if input has value on page load
            if (input.value) {
                input.classList.add('has-value');
            }

            // Add has-value class when input is filled
            input.addEventListener('input', function () {
                if (this.value) {
                    this.classList.add('has-value');
                } else {
                    this.classList.remove('has-value');
                }
            });

            // Handle autofill
            input.addEventListener('change', function () {
                if (this.value) {
                    this.classList.add('has-value');
                }
            });
        });
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFloatingLabels);
    } else {
        initFloatingLabels();
    }
})();
