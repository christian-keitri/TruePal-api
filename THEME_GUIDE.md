# TruePal Theme Guide

## Color Scheme

**Primary:** Goldenrod `#FFC107`
**Accent:** Amber `#FFB300`
**Background:** Jet Black `#121212` / Charcoal `#1E1E1E`
**Text on dark:** White `#FFFFFF` / Cool Gray `#B0B0B0`

Dark-themed UI with gold accents. All colors are defined as CSS variables in `wwwroot/css/theme.css`.

---

## CSS Variables

### Primary Colors
```css
var(--color-primary)          /* #FFC107 - Goldenrod, main brand color */
var(--color-primary-dark)     /* #FFB300 - Amber, hover states */
var(--color-primary-darker)   /* #FFA000 - Deeper amber, active states */
var(--color-primary-light)    /* rgba(255,193,7,0.15) - Subtle backgrounds */
var(--color-primary-border)   /* rgba(255,193,7,0.2) - Borders on dark */
var(--color-primary-glow)     /* rgba(255,193,7,0.4) - Focus rings */
var(--color-primary-gradient) /* linear-gradient(90deg, #FFC107, #FFB300) */
```

### Accent Colors
```css
var(--color-accent)       /* #FFB300 - Amber */
var(--color-accent-dark)  /* #FFA000 - Hover amber */
var(--color-blue)         /* #007BFF - Electric Blue (CTAs) */
var(--color-orange)       /* #FF9800 - Warm Orange */
```

### Semantic Colors
```css
var(--color-success)  /* #4CAF50 - Green */
var(--color-danger)   /* #F44336 - Red */
var(--color-info)     /* #007BFF - Blue */
var(--color-warning)  /* #FF9800 - Orange */
```

### Background Colors
```css
var(--bg-primary)       /* #FFFFFF - Main light background */
var(--bg-secondary)     /* #FAFAFA - Off-white sections */
var(--bg-tertiary)      /* #F5F5F5 - Light gray sections */
var(--bg-dark)          /* #121212 - Jet Black */
var(--bg-dark-elevated) /* #1E1E1E - Charcoal (cards on dark) */
var(--bg-white)         /* #FFFFFF */
```

### Dark Surface Colors
```css
var(--color-dark)          /* #121212 - Jet Black base */
var(--color-dark-lighter)  /* #1E1E1E - Charcoal cards */
var(--color-dark-elevated) /* #1E1E1E - Elevated surfaces */
var(--color-dark-border)   /* #2A2A2A - Borders on dark */
```

### Text Colors
```css
var(--text-primary)         /* #121212 - Main text on light */
var(--text-secondary)       /* #757575 - Secondary text on light */
var(--text-muted)           /* #B0B0B0 - Metadata on light */
var(--text-white)           /* #FFFFFF - Text on dark backgrounds */
var(--text-white-secondary) /* #B0B0B0 - Secondary text on dark */
var(--text-yellow)          /* #FFC107 - Highlighted text */
```

### Gray Scale
```css
var(--color-gray-900) /* #1E1E1E */
var(--color-gray-800) /* #2A2A2A */
var(--color-gray-700) /* #505050 */
var(--color-gray-600) /* #757575 */
var(--color-gray-500) /* #9E9E9E */
var(--color-gray-400) /* #B0B0B0 */
var(--color-gray-300) /* #CCCCCC */
var(--color-gray-200) /* #E0E0E0 */
var(--color-gray-100) /* #F5F5F5 */
```

### Spacing
```css
var(--spacing-xs)   /* 0.25rem (4px) */
var(--spacing-sm)   /* 0.5rem  (8px) */
var(--spacing-md)   /* 1rem    (16px) */
var(--spacing-lg)   /* 1.5rem  (24px) */
var(--spacing-xl)   /* 2rem    (32px) */
var(--spacing-2xl)  /* 3rem    (48px) */
var(--spacing-3xl)  /* 4rem    (64px) */
```

### Border Radius
```css
var(--radius-sm)     /* 0.25rem (4px) */
var(--radius-md)     /* 0.5rem  (8px) */
var(--radius-lg)     /* 1rem    (16px) */
var(--radius-xl)     /* 1.5rem  (24px) */
var(--radius-pill)   /* 20px */
var(--radius-circle) /* 50% */
```

### Shadows
```css
var(--shadow-sm)  /* 0 2px 4px rgba(0,0,0,0.1) */
var(--shadow-md)  /* 0 4px 8px rgba(0,0,0,0.1) */
var(--shadow-lg)  /* 0 8px 16px rgba(0,0,0,0.15) */
var(--shadow-xl)  /* 0 12px 24px rgba(0,0,0,0.2) */
```

### Transitions
```css
var(--transition-fast)  /* 0.15s ease */
var(--transition-base)  /* 0.3s ease */
var(--transition-slow)  /* 0.5s ease */
```

---

## Utility Classes

### Text & Background
```html
<p class="text-primary">Goldenrod text</p>
<div class="bg-primary">Goldenrod background</div>
<div class="border-primary">Goldenrod border</div>
```

### Icons
```html
<i class="bi bi-heart icon-theme"></i>         <!-- Default size -->
<i class="bi bi-star icon-theme-lg"></i>       <!-- 2rem -->
<i class="bi bi-trophy icon-theme-xl"></i>     <!-- 3rem -->
```

---

## Component Classes

### Cards
```html
<div class="theme-card">
    <h3>Title</h3>
    <p>Content with hover effect</p>
</div>
```

### Buttons
```html
<button class="btn-theme-primary">Primary</button>
<button class="btn-theme-outline">Outline</button>
```

### Badges & Alerts
```html
<span class="badge-theme">New</span>
<div class="alert-theme"><strong>Info:</strong> Themed alert</div>
```

### Links & Dividers
```html
<a href="#" class="link-theme">Themed Link</a>
<div class="divider-theme"></div>
```

---

## Usage Examples

### Custom Component
```css
.my-component {
    background: var(--bg-dark-elevated);
    border: 1px solid var(--color-primary-border);
    border-radius: var(--radius-lg);
    padding: var(--spacing-lg);
    box-shadow: var(--shadow-md);
    transition: var(--transition-base);
}

.my-component:hover {
    border-color: var(--color-primary-dark);
    box-shadow: var(--shadow-lg);
}
```

### Text on Dark Background
```css
.dark-section h2 { color: var(--text-white); }
.dark-section p { color: var(--text-white-secondary); }
.dark-section .highlight { color: var(--text-yellow); }
```

---

## Bootstrap Override

The theme overrides Bootstrap's primary color to Goldenrod:
- `.btn-primary` uses Goldenrod
- `.text-primary` uses Goldenrod
- `.border-primary` uses Goldenrod
- Form focus states use Goldenrod borders

---

## Rules

1. **Never hardcode colors** - always use CSS variables
2. **Use the spacing scale** - `var(--spacing-md)` not `16px`
3. **Use shadows for depth** - cards and elevated elements need shadows
4. **Add transitions** - all interactive elements need `var(--transition-base)`
5. **Ensure contrast** - white/gold text on dark backgrounds, dark text on light backgrounds

---

**File location:** `wwwroot/css/theme.css`

**Last Updated:** April 15, 2026
