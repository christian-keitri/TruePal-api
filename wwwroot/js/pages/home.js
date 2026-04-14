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

        // Update active chip
        UIService.setActiveChip(location.name);

        // Show posts for location
        postsService.showPosts(location.name);
    }

    // ===== BUILD UI COMPONENTS =====
    UIService.buildRegionChips('region-chips', handleLocationClick);

    // ===== ADD MARKERS TO MAP =====
    mapService.addMarkers(handleLocationClick);

    // ===== MAP CLICK HANDLER (Optional) =====
    mapService.onMapClick((e) => {
        if (!e.originalEvent.defaultPrevented) {
            // Optional: reset view or do nothing
        }
    });

});
