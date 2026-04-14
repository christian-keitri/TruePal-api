# JavaScript Architecture - Quick Reference

## 📂 Folder Structure

```
wwwroot/js/
│
├── 📁 data/                    # Data & Constants
│   ├── locations.js           # All Philippine destinations
│   └── sample-posts.js        # Demo posts data
│
├── 📁 services/               # Business Logic Services
│   ├── map-service.js         # Map initialization & interactions
│   ├── posts-service.js       # Posts display & rendering
│   └── ui-service.js          # UI components (chips, etc.)
│
└── 📁 pages/                  # Page Entry Points
    └── home.js                # Home page orchestration
```

---

## 🔄 Data Flow

```
┌─────────────┐
│  home.js    │  ← Entry point (pages/)
│ (Page Init) │
└──────┬──────┘
       │
       ├─────→ MapService ──────→ locations.js
       │       (services/)        (data/)
       │
       ├─────→ PostsService ────→ sample-posts.js
       │       (services/)        (data/)
       │
       └─────→ UIService ────────→ locations.js
               (services/)        (data/)
```

---

## ⚡ Quick Examples

### Creating a Map
```javascript
import { MapService } from '../services/map-service.js';

const mapService = new MapService('philippine-map');
mapService.initialize();
mapService.addMarkers((location) => console.log(location));
```

### Displaying Posts
```javascript
import { PostsService } from '../services/posts-service.js';

const postsService = new PostsService('posts-container', 'title-id', 'subtitle-id');
postsService.showPosts('Boracay');
```

### Building UI Components
```javascript
import { UIService } from '../services/ui-service.js';

UIService.buildRegionChips('region-chips', (location) => {
    // Handle chip click
});
```

---

## 🎯 Benefits

✅ **Separation of Concerns** - Each file has one job  
✅ **Reusability** - Services can be used across pages  
✅ **Testability** - Easy to write unit tests  
✅ **Maintainability** - Easy to find and fix bugs  
✅ **Scalability** - Add new features without breaking existing code  

---

## 📝 Adding New Features

1. **New Service?** → Create in `services/`
2. **New Data?** → Create in `data/`
3. **New Page?** → Create in `pages/`

**Example - Adding API Service:**
```javascript
// services/api-service.js
export class ApiService {
    static async getPosts(location) {
        const response = await fetch(`/api/posts?location=${location}`);
        return response.json();
    }
}
```

**Use it:**
```javascript
import { ApiService } from '../services/api-service.js';
const posts = await ApiService.getPosts('Cebu');
```

---

## 🔗 See Full Documentation

👉 [Complete JavaScript Architecture Guide](README.md)

---

**Result:** Clean, scalable, maintainable JavaScript! 🚀
