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
}
