# TruePal Theme Guide

## Color Scheme
**Primary:** Smokey Yellow (#c9a961)  
**Secondary:** White & Gray tones  
**Accent:** Darker Yellow (#b8985a)

---

## CSS Variables

### Colors
```css
var(--primary-yellow)        /* Main brand color */
var(--primary-yellow-dark)   /* Darker shade for hover states */
var(--primary-yellow-light)  /* Light transparent version */
var(--primary-yellow-glow)   /* For shadows/glows */
```

### Backgrounds
```css
var(--bg-primary)    /* Light gray #f5f5f5 */
var(--bg-secondary)  /* Medium gray #e8e8e8 */
var(--bg-tertiary)   /* Darker gray #d6d6d6 */
var(--bg-white)      /* Pure white */
```

### Text Colors
```css
var(--text-primary)    /* Dark gray #333 */
var(--text-secondary)  /* Medium gray #666 */
var(--text-muted)      /* Light gray #999 */
var(--text-white)      /* White */
```

### Spacing
```css
var(--spacing-xs)   /* 4px */
var(--spacing-sm)   /* 8px */
var(--spacing-md)   /* 16px */
var(--spacing-lg)   /* 24px */
var(--spacing-xl)   /* 32px */
var(--spacing-2xl)  /* 48px */
var(--spacing-3xl)  /* 64px */
```

### Border Radius
```css
var(--radius-sm)     /* 4px */
var(--radius-md)     /* 8px */
var(--radius-lg)     /* 16px */
var(--radius-xl)     /* 24px */
var(--radius-pill)   /* 20px (pill shape) */
var(--radius-circle) /* 50% (circle) */
```

### Shadows
```css
var(--shadow-sm)  /* Light shadow */
var(--shadow-md)  /* Medium shadow */
var(--shadow-lg)  /* Large shadow */
var(--shadow-xl)  /* Extra large shadow */
```

### Transitions
```css
var(--transition-fast)  /* 0.15s */
var(--transition-base)  /* 0.3s */
var(--transition-slow)  /* 0.5s */
```

---

## Utility Classes

### Text & Background
```html
<p class="text-primary">Yellow text</p>
<div class="bg-primary">Yellow background</div>
<div class="border-primary">Yellow border</div>
```

### Accent Classes
```html
<i class="bi bi-heart icon-theme"></i>
<i class="bi bi-star icon-theme-lg"></i>      <!-- 2rem size -->
<i class="bi bi-trophy icon-theme-xl"></i>    <!-- 3rem size -->
```

### Spacing
```html
<div class="p-md">Padding medium</div>
<div class="m-lg">Margin large</div>
```

---

## Component Classes

### Cards
```html
<div class="theme-card">
    <h3>Card Title</h3>
    <p>Card content with hover effect</p>
</div>
```

### Buttons
```html
<!-- Primary Button -->
<button class="btn-theme-primary">Click Me</button>

<!-- Outline Button -->
<button class="btn-theme-outline">Outline Style</button>
```

### Badges
```html
<span class="badge-theme">New</span>
```

### Alerts
```html
<div class="alert-theme">
    <strong>Info:</strong> This is a themed alert.
</div>
```

### Links
```html
<a href="#" class="link-theme">Themed Link</a>
```

### Dividers
```html
<div class="divider-theme"></div>
```

---

## Usage Examples

### Custom Styles with Variables
```html
<style>
.my-custom-box {
    background: var(--bg-white);
    border: 2px solid var(--primary-yellow);
    border-radius: var(--radius-lg);
    padding: var(--spacing-lg);
    box-shadow: var(--shadow-md);
    transition: var(--transition-base);
}

.my-custom-box:hover {
    border-color: var(--primary-yellow-dark);
    box-shadow: var(--shadow-lg);
}
</style>
```

### Icon with Theme Color
```html
<i class="bi bi-check-circle" style="color: var(--primary-yellow); font-size: 2rem;"></i>
```

### Card with Theme Styling
```html
<div class="theme-card">
    <div style="display: flex; align-items: center; gap: var(--spacing-md);">
        <i class="bi bi-star icon-theme-lg"></i>
        <div>
            <h4 style="margin: 0; color: var(--text-primary);">Featured</h4>
            <p style="margin: 0; color: var(--text-secondary);">Special content</p>
        </div>
    </div>
</div>
```

### Button Group
```html
<div style="display: flex; gap: var(--spacing-md);">
    <button class="btn-theme-primary">Save</button>
    <button class="btn-theme-outline">Cancel</button>
</div>
```

---

## Bootstrap Override

The theme automatically overrides Bootstrap's primary color:
- `.btn-primary` → Yellow buttons
- `.text-primary` → Yellow text
- `.border-primary` → Yellow borders
- `.btn-outline-primary` → Yellow outline buttons
- Form focus states → Yellow borders

---

## Pro Tips

1. **Consistency**: Always use CSS variables instead of hardcoded colors
2. **Spacing**: Use the spacing scale for margins and padding
3. **Shadows**: Apply shadows for depth and visual hierarchy
4. **Transitions**: Add smooth transitions for interactive elements
5. **Accessibility**: Ensure sufficient contrast (theme uses white text on yellow backgrounds)

---

## File Location
Theme CSS: `/wwwroot/css/theme.css`

To modify the theme, edit the `:root` variables in this file, and all components will update automatically!
