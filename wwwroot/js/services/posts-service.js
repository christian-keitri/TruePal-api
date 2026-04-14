/**
 * TruePal - Posts Service
 * Handles posts display and rendering
 */

import { samplePosts } from '../data/sample-posts.js';
import { locations } from '../data/locations.js';

export class PostsService {
    constructor(containerId, titleId, subtitleId) {
        this.postsContainer = document.getElementById(containerId);
        this.locationTitle = document.getElementById(titleId);
        this.locationSubtitle = document.getElementById(subtitleId);
        this.welcomeMessage = document.getElementById('welcome-message');
    }

    /**
     * Display posts for a specific region
     */
    showPosts(regionName) {
        const posts = samplePosts[regionName] || [];
        const location = locations.find(l => l.name === regionName);

        this.updateHeader(regionName, posts.length, location);
        this.hideWelcomeMessage();

        if (posts.length === 0) {
            this.showEmptyState(regionName);
        } else {
            this.renderPosts(posts);
        }

        this.scrollToTop();
    }

    /**
     * Update the header with location info
     */
    updateHeader(regionName, postsCount, location) {
        const icon = location?.icon || 'bi-geo-alt-fill';
        this.locationTitle.innerHTML = `<i class="bi ${icon} me-2"></i>${regionName}`;
        this.locationSubtitle.textContent = `${postsCount} post${postsCount !== 1 ? 's' : ''} from travelers`;
    }

    /**
     * Hide welcome message
     */
    hideWelcomeMessage() {
        if (this.welcomeMessage) {
            this.welcomeMessage.style.display = 'none';
        }
    }

    /**
     * Show empty state when no posts exist
     */
    showEmptyState(regionName) {
        this.postsContainer.innerHTML = `
            <div class="welcome-message" style="display: block;">
                <i class="bi bi-camera" style="font-size: 4rem; color: var(--color-primary); margin-bottom: 1rem;"></i>
                <h3>No Posts Yet</h3>
                <p class="text-muted">No one has shared about ${regionName} yet.<br>Be the first to post!</p>
            </div>`;
    }

    /**
     * Render posts list
     */
    renderPosts(posts) {
        const postsHTML = posts.map(post => this.createPostCard(post)).join('');
        this.postsContainer.innerHTML = postsHTML;
    }

    /**
     * Create HTML for a single post card
     */
    createPostCard(post) {
        const imageBlock = post.image
            ? `<div class="flex-card-image" style="background-image: url('${post.image}')">
                   <div class="flex-card-image-overlay">
                       <span class="badge"><i class="bi bi-geo-alt-fill me-1"></i>${post.location}</span>
                   </div>
               </div>`
            : '';

        const locationInline = !post.image
            ? `<span style="color: var(--color-primary); font-size: 0.75rem;"><i class="bi bi-geo-alt-fill"></i> ${post.location}</span>`
            : '';

        return `
            <div class="flex-card mb-3">
                ${imageBlock}
                <div class="flex-card-body">
                    <div class="flex-card-user">
                        <div class="flex-card-avatar">${post.initials}</div>
                        <div>
                            <div class="flex-card-name">${post.user}</div>
                            <div class="flex-card-time">${post.time} ${locationInline}</div>
                        </div>
                    </div>
                    <div class="flex-card-content">${post.content}</div>
                    <div class="flex-card-actions">
                        <button class="flex-card-action">
                            <i class="bi bi-heart"></i> 
                            <span class="count">${post.likes}</span>
                        </button>
                        <button class="flex-card-action">
                            <i class="bi bi-chat"></i> 
                            <span class="count">${post.comments}</span>
                        </button>
                        <button class="flex-card-action">
                            <i class="bi bi-share"></i>
                        </button>
                    </div>
                </div>
            </div>`;
    }

    /**
     * Scroll to top of posts container
     */
    scrollToTop() {
        this.postsContainer.scrollTop = 0;
    }
}
