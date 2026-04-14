/**
 * TruePal - Map Service
 * Handles all map-related functionality using MapLibre GL
 */

import { locations } from '../data/locations.js';

export class MapService {
    constructor(containerId, config = {}) {
        this.containerId = containerId;
        this.config = {
            style: config.style || 'https://tiles.openfreemap.org/styles/liberty',
            center: config.center || [122.5, 12.5],
            zoom: config.zoom || 5.8,
            pitch: config.pitch || 55,
            bearing: config.bearing || -10,
            ...config
        };
        this.map = null;
        this.markers = [];
    }

    /**
     * Initialize the map
     */
    initialize() {
        this.map = new maplibregl.Map({
            container: this.containerId,
            style: this.config.style,
            center: this.config.center,
            zoom: this.config.zoom,
            pitch: this.config.pitch,
            bearing: this.config.bearing,
            antialias: true,
            attributionControl: false
        });

        // Add controls
        // Attribution hidden for cleaner look
        this.map.addControl(
            new maplibregl.NavigationControl({ visualizePitch: true }),
            'top-right'
        );

        return this.map;
    }

    /**
     * Add location markers to the map
     * @param {Function} onMarkerClick - Callback when marker is clicked
     */
    addMarkers(onMarkerClick) {
        this.map.on('load', () => {
            locations.forEach(loc => {
                const el = this.createMarkerElement(loc);

                el.addEventListener('click', (e) => {
                    e.stopPropagation();
                    e.preventDefault();
                    if (onMarkerClick) {
                        onMarkerClick(loc);
                    }
                });

                const marker = new maplibregl.Marker({
                    element: el,
                    anchor: 'bottom'
                })
                    .setLngLat([loc.lng, loc.lat])
                    .addTo(this.map);

                this.markers.push({ location: loc, marker, element: el });
            });
        });
    }

    /**
     * Create a marker DOM element
     */
    createMarkerElement(location) {
        const el = document.createElement('div');
        el.className = 'map-marker';
        el.innerHTML = `
            <div class="map-marker-pulse"></div>
            <div class="map-marker-circle">
                <i class="bi ${location.icon}"></i>
            </div>
        `;
        return el;
    }

    /**
     * Fly to a specific location
     */
    flyToLocation(location, options = {}) {
        const flyOptions = {
            center: [location.lng, location.lat],
            zoom: location.zoom || 12,
            pitch: options.pitch || 60,
            bearing: options.bearing || (Math.random() * 40 - 20),
            duration: options.duration || 2500,
            essential: true
        };

        this.map.flyTo(flyOptions);
    }

    /**
     * Reset map to default view
     */
    resetView() {
        this.map.flyTo({
            center: this.config.center,
            zoom: this.config.zoom,
            pitch: this.config.pitch,
            bearing: this.config.bearing,
            duration: 2000
        });
    }

    /**
     * Add click listener to map
     */
    onMapClick(callback) {
        this.map.on('click', callback);
    }

    /**
     * Get the map instance
     */
    getMap() {
        return this.map;
    }
}
