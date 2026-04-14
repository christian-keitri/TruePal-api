/**
 * TruePal - Home Page
 * Main entry point for the home page with interactive map
 */

import { MapService } from '../services/map-service.js';
import { PostsService } from '../services/posts-service.js';
import { UIService } from '../services/ui-service.js';

document.addEventListener('DOMContentLoaded', function () {

    // ===== INITIALIZE SERVICES =====
    const mapService = new MapService('philippine-map');
    const postsService = new PostsService('posts-container', 'location-title', 'location-subtitle');

    // Initialize map
    const map = mapService.initialize();

    // ===== HANDLE LOCATION SELECTION =====
    function handleLocationClick(location) {
        // Fly to location on map
        mapService.flyToLocation(location);

        // Update active state in sidebar
        UIService.setActiveSidebarLocation(location.name);

        // Show posts for location (this will hide carousel)
        postsService.showPosts(location.name);
    }

    // ===== RESET VIEW (Show carousel again) =====
    function resetView() {
        // Reset map view
        mapService.resetView();

        // Clear active sidebar location
        UIService.setActiveSidebarLocation(null);

        // Show carousel
        postsService.showWelcomeMessage();
    }

    // ===== BUILD UI COMPONENTS =====
    UIService.buildLocationsSidebar('locations-sidebar', handleLocationClick);

    // ===== MAP IS NOW CLEAN (no markers, no chips) =====
    // The sidebar on the left shows all locations vertically

    // ===== MAP CLICK HANDLER =====
    mapService.onMapClick((e) => {
        if (!e.originalEvent.defaultPrevented) {
            // Reset view when clicking on empty map area
            resetView();
        }
    });

    // ===== TRENDING POSTS CAROUSEL =====
    const carouselContainer = document.querySelector('.trending-carousel');
    const carouselTrack = document.querySelector('.carousel-track');
    const prevBtn = document.querySelector('.carousel-prev');
    const nextBtn = document.querySelector('.carousel-next');
    const indicatorsContainer = document.getElementById('carousel-indicators');

    if (carouselTrack && prevBtn && nextBtn && carouselContainer) {
        let currentIndex = 0;
        const cards = document.querySelectorAll('.trending-card');
        const totalCards = cards.length;
        let isDragging = false;
        let startPos = 0;
        let currentTranslate = 0;
        let prevTranslate = 0;
        let animationID = 0;

        // Show one card at a time
        const getVisibleCards = () => {
            return 1; // Always show 1 card in slider mode
        };

        const getCardWidth = () => {
            const containerWidth = carouselContainer.offsetWidth;
            const gap = 16; // spacing-lg
            return containerWidth + gap;
        };

        // Create indicator dots
        const createIndicators = () => {
            if (!indicatorsContainer) return;

            indicatorsContainer.innerHTML = '';

            for (let i = 0; i < totalCards; i++) {
                const indicator = document.createElement('button');
                indicator.classList.add('carousel-indicator');
                indicator.setAttribute('aria-label', `Go to slide ${i + 1}`);
                if (i === 0) indicator.classList.add('active');

                indicator.addEventListener('click', () => {
                    currentIndex = i;
                    updateCarousel();
                });

                indicatorsContainer.appendChild(indicator);
            }
        };

        const updateIndicators = () => {
            if (!indicatorsContainer) return;

            const indicators = indicatorsContainer.querySelectorAll('.carousel-indicator');
            indicators.forEach((indicator, index) => {
                indicator.classList.toggle('active', index === currentIndex);
            });
        };

        const updateCarousel = () => {
            const cardWidth = getCardWidth();
            const maxIndex = totalCards - 1; // One card at a time

            // Ensure currentIndex is within bounds
            currentIndex = Math.max(0, Math.min(currentIndex, maxIndex));

            // Move the track
            const translateX = -(currentIndex * cardWidth);
            carouselTrack.style.transform = `translateX(${translateX}px)`;
            prevTranslate = translateX;
            currentTranslate = translateX;

            // Update button states
            prevBtn.disabled = currentIndex === 0;
            nextBtn.disabled = currentIndex >= maxIndex;

            // Update indicators
            updateIndicators();
        };

        // Button controls
        prevBtn.addEventListener('click', () => {
            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextBtn.addEventListener('click', () => {
            const maxIndex = totalCards - 1;
            if (currentIndex < maxIndex) {
                currentIndex++;
                updateCarousel();
            }
        });

        // Touch/Mouse drag support
        const getPositionX = (event) => {
            return event.type.includes('mouse') ? event.pageX : event.touches[0].clientX;
        };

        const touchStart = (index) => {
            return function (event) {
                isDragging = true;
                startPos = getPositionX(event);
                animationID = requestAnimationFrame(animation);
                carouselTrack.classList.add('dragging');
            };
        };

        const touchMove = (event) => {
            if (isDragging) {
                const currentPosition = getPositionX(event);
                currentTranslate = prevTranslate + currentPosition - startPos;
            }
        };

        const touchEnd = () => {
            isDragging = false;
            cancelAnimationFrame(animationID);
            carouselTrack.classList.remove('dragging');

            const movedBy = currentTranslate - prevTranslate;

            // Swipe threshold
            if (movedBy < -50 && currentIndex < totalCards - 1) {
                currentIndex++;
            }
            if (movedBy > 50 && currentIndex > 0) {
                currentIndex--;
            }

            updateCarousel();
        };

        const animation = () => {
            if (isDragging) {
                carouselTrack.style.transform = `translateX(${currentTranslate}px)`;
                requestAnimationFrame(animation);
            }
        };

        // Add event listeners for drag/swipe
        carouselContainer.addEventListener('mousedown', touchStart(0));
        carouselContainer.addEventListener('touchstart', touchStart(0));
        carouselContainer.addEventListener('mousemove', touchMove);
        carouselContainer.addEventListener('touchmove', touchMove);
        carouselContainer.addEventListener('mouseup', touchEnd);
        carouselContainer.addEventListener('mouseleave', touchEnd);
        carouselContainer.addEventListener('touchend', touchEnd);

        // Prevent context menu on long press
        carouselContainer.addEventListener('contextmenu', (e) => {
            e.preventDefault();
        });

        // Prevent image dragging
        cards.forEach(card => {
            card.addEventListener('dragstart', (e) => e.preventDefault());
        });

        // Update on window resize
        window.addEventListener('resize', () => {
            updateCarousel();
        });

        // Initial setup
        createIndicators();
        updateCarousel();
    }

});
