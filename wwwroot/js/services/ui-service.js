/**
 * TruePal - UI Service
 * Handles UI interactions like region chips
 */

import { locations, featuredLocations } from '../data/locations.js';

export class UIService {
    /**
     * Build region chips at the bottom of the map
     */
    static buildRegionChips(containerId, onChipClick) {
        const chipsContainer = document.getElementById(containerId);
        if (!chipsContainer) return;

        chipsContainer.innerHTML = ''; // Clear existing

        featuredLocations.forEach(name => {
            const location = locations.find(l => l.name === name);
            if (!location) return;

            const chip = document.createElement('span');
            chip.className = 'region-chip';
            chip.textContent = name;
            chip.dataset.locationName = name;

            chip.addEventListener('click', () => {
                if (onChipClick) {
                    onChipClick(location);
                }
            });

            chipsContainer.appendChild(chip);
        });
    }

    /**
     * Set active state for a region chip
     */
    static setActiveChip(locationName) {
        document.querySelectorAll('.region-chip').forEach(chip => {
            chip.classList.remove('active');
            if (chip.dataset.locationName === locationName) {
                chip.classList.add('active');
            }
        });
    }

    /**
     * Clear all active chips
     */
    static clearActiveChips() {
        document.querySelectorAll('.region-chip').forEach(chip => {
            chip.classList.remove('active');
        });
    }

    /**
     * Build vertical locations sidebar
     */
    static buildLocationsSidebar(containerId, onLocationClick) {
        const sidebar = document.getElementById(containerId);
        if (!sidebar) return;

        sidebar.innerHTML = ''; // Clear existing

        locations.forEach(location => {
            const item = document.createElement('div');
            item.className = 'sidebar-location';
            item.dataset.locationName = location.name;
            item.innerHTML = `
                <div class="sidebar-marker-circle">
                    <i class="bi ${location.icon}"></i>
                </div>
                <span class="sidebar-location-name">${location.name}</span>
            `;

            item.addEventListener('click', () => {
                // Update active state
                document.querySelectorAll('.sidebar-location').forEach(loc => {
                    loc.classList.remove('active');
                });
                item.classList.add('active');

                // Trigger callback
                if (onLocationClick) {
                    onLocationClick(location);
                }
            });

            sidebar.appendChild(item);
        });
    }

    /**
     * Set active location in sidebar
     */
    static setActiveSidebarLocation(locationName) {
        document.querySelectorAll('.sidebar-location').forEach(item => {
            item.classList.remove('active');
            if (item.dataset.locationName === locationName) {
                item.classList.add('active');
            }
        });
    }
}
