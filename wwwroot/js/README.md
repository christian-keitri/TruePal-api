# JavaScript Architecture

## 📁 Folder Structure

```
wwwroot/js/
├── data/                    # Data files and constants
│   ├── locations.js         # Philippine destinations data
│   └── sample-posts.js      # Sample posts (to be replaced with API)
├── services/                # Business logic and services
│   ├── map-service.js       # Map functionality (MapLibre GL)
│   ├── posts-service.js     # Posts display and rendering
│   └── ui-service.js        # UI interactions (chips, etc.)
└── pages/                   # Page-specific entry points
    └── home.js              # Home page initialization
```

---

## 🎯 Design Principles

### 1. **Separation of Concerns**
Each module has a single, well-defined purpose:
- **Data** modules contain only data structures
- **Services** handle business logic and interactions
- **Pages** orchestrate services for specific pages

### 2. **ES6 Modules**
Uses modern JavaScript module syntax (`import`/`export`) for:
- Better code organization
- Explicit dependencies
- Tree-shaking support
- Improved maintainability

### 3. **Scalability**
Easy to extend:
- Add new services in `/services`
- Add new data sources in `/data`
- Create page-specific scripts in `/pages`

---

## 📋 Module Documentation

### Data Modules (`/data`)

#### **locations.js**
```javascript
export const locations = [...]      // Array of all Philippine destinations
export const featuredLocations = [...]  // Featured regions for chips
```

**Usage:**
```javascript
import { locations, featuredLocations } from '../data/locations.js';
```

#### **sample-posts.js**
```javascript
export const samplePosts = {...}   // Posts organized by location
```

**Usage:**
```javascript
import { samplePosts } from '../data/sample-posts.js';
```

---

### Service Modules (`/services`)

#### **MapService** (`map-service.js`)
Handles all map-related functionality using MapLibre GL.

**API:**
```javascript
const mapService = new MapService(containerId, config);

// Initialize map
mapService.initialize();

// Add markers
mapService.addMarkers(onClickCallback);

// Fly to location
mapService.flyToLocation(location, options);

// Reset to default view
mapService.resetView();

// Add click listener
mapService.onMapClick(callback);
```

**Example:**
```javascript
import { MapService } from '../services/map-service.js';

const mapService = new MapService('philippine-map');
const map = mapService.initialize();

mapService.addMarkers((location) => {
    console.log('Clicked:', location.name);
});
```

---

#### **PostsService** (`posts-service.js`)
Handles posts display and rendering in the details section.

**API:**
```javascript
const postsService = new PostsService(containerId, titleId, subtitleId);

// Show posts for a region
postsService.showPosts(regionName);

// Internal methods (called automatically)
postsService.updateHeader(regionName, count, location);
postsService.renderPosts(posts);
postsService.showEmptyState(regionName);
```

**Example:**
```javascript
import { PostsService } from '../services/posts-service.js';

const postsService = new PostsService(
    'posts-container',
    'location-title',
    'location-subtitle'
);

postsService.showPosts('Boracay');
```

---

#### **UIService** (`ui-service.js`)
Handles UI interactions like region chips.

**API:**
```javascript
// Build region chips
UIService.buildRegionChips(containerId, onClickCallback);

// Set active chip
UIService.setActiveChip(locationName);

// Clear all active chips
UIService.clearActiveChips();
```

**Example:**
```javascript
import { UIService } from '../services/ui-service.js';

UIService.buildRegionChips('region-chips', (location) => {
    console.log('Chip clicked:', location.name);
});

UIService.setActiveChip('Cebu');
```

---

### Page Scripts (`/pages`)

#### **home.js**
Entry point for the home page. Orchestrates all services.

**Structure:**
```javascript
import { MapService } from '../services/map-service.js';
import { PostsService } from '../services/posts-service.js';
import { UIService } from '../services/ui-service.js';

document.addEventListener('DOMContentLoaded', function () {
    // 1. Initialize services
    // 2. Set up event handlers
    // 3. Wire everything together
});
```

---

## 🚀 Adding New Features

### Example: Adding a Search Service

**1. Create the service (`/services/search-service.js`):**
```javascript
/**
 * Search Service
 * Handles location search functionality
 */

import { locations } from '../data/locations.js';

export class SearchService {
    static search(query) {
        return locations.filter(loc => 
            loc.name.toLowerCase().includes(query.toLowerCase())
        );
    }
}
```

**2. Use it in your page script:**
```javascript
import { SearchService } from '../services/search-service.js';

const results = SearchService.search('boracay');
console.log(results);
```

---

### Example: Adding API Integration

**1. Create an API service (`/services/api-service.js`):**
```javascript
/**
 * API Service
 * Handles all API communications
 */

export class ApiService {
    static async getPosts(locationId) {
        const response = await fetch(`/api/posts?location=${locationId}`);
        return await response.json();
    }

    static async createPost(postData) {
        const response = await fetch('/api/posts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(postData)
        });
        return await response.json();
    }
}
```

**2. Update PostsService to use API:**
```javascript
import { ApiService } from './api-service.js';

export class PostsService {
    async showPosts(regionName) {
        // Replace sample data with API call
        const posts = await ApiService.getPosts(regionName);
        this.renderPosts(posts);
    }
}
```

---

## 📝 Usage in Views

### In Razor Views (`.cshtml`)

```html
@section Scripts {
    <!-- External libraries -->
    <script src="https://unpkg.com/maplibre-gl@4.7.1/dist/maplibre-gl.js"></script>
    
    <!-- Your app scripts (ES6 modules) -->
    <script type="module" src="~/js/pages/home.js" asp-append-version="true"></script>
}
```

**Important:** Always use `type="module"` for ES6 modules!

---

## 🔧 Development Guidelines

### 1. **File Naming**
- Use kebab-case: `map-service.js`, `sample-posts.js`
- Be descriptive: `posts-service.js` not `posts.js`

### 2. **Class vs Static**
- Use **classes** for stateful services (MapService, PostsService)
- Use **static methods** for utilities (UIService)

### 3. **Comments**
- Add JSDoc comments for public APIs
- Explain complex logic
- Document parameters and return values

### 4. **Error Handling**
```javascript
export class ApiService {
    static async getPosts(locationId) {
        try {
            const response = await fetch(`/api/posts?location=${locationId}`);
            if (!response.ok) throw new Error('Failed to fetch posts');
            return await response.json();
        } catch (error) {
            console.error('Error fetching posts:', error);
            return [];
        }
    }
}
```

---

## 🧪 Testing

### Unit Testing (Future Enhancement)
```javascript
// tests/services/map-service.test.js
import { MapService } from '../../services/map-service.js';

describe('MapService', () => {
    it('should initialize with correct config', () => {
        const mapService = new MapService('test-map');
        expect(mapService.containerId).toBe('test-map');
    });
});
```

---

## 🎯 Benefits of This Structure

### ✅ **Maintainability**
- Easy to find and fix bugs
- Clear ownership of functionality
- Consistent patterns

### ✅ **Scalability**
- Add new features without touching existing code
- Services are independent
- Easy to extend

### ✅ **Testability**
- Services can be tested in isolation
- Mock dependencies easily
- Clear inputs and outputs

### ✅ **Reusability**
- Services can be used across multiple pages
- Data modules are centralized
- No code duplication

### ✅ **Performance**
- Browser can cache individual modules
- Tree-shaking removes unused code
- Parallel loading of modules

---

## 🔄 Migration from Inline Scripts

**Before (inline in `.cshtml`):**
```html
<script>
    const map = new maplibregl.Map({...});
    const locations = [...];
    // 500 lines of code mixed together
</script>
```

**After (modular):**
```html
<script type="module" src="~/js/pages/home.js"></script>
```

**Result:** Clean views, organized code, better performance!

---

## 📚 Further Reading

- [ES6 Modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules)
- [Clean Code JavaScript](https://github.com/ryanmcdermott/clean-code-javascript)
- [Service Pattern](https://en.wikipedia.org/wiki/Service_locator_pattern)

---

**Questions?** Check [CODING_STANDARDS.md](../CODING_STANDARDS.md) for general coding guidelines.
