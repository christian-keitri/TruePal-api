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

        // Show posts for location
        postsService.showPosts(location.name);
    }

    // ===== BUILD UI COMPONENTS =====
    UIService.buildLocationsSidebar('locations-sidebar', handleLocationClick);

    // ===== MAP IS NOW CLEAN (no markers, no chips) =====
    // The sidebar on the left shows all locations vertically

    // ===== MAP CLICK HANDLER (Optional) =====
    mapService.onMapClick((e) => {
        if (!e.originalEvent.defaultPrevented) {
            // Optional: reset view or do nothing
        }
    });

});
