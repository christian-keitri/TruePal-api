# TruePal CSS & Component Architecture

## Overview
This document describes the scalable CSS and component architecture for TruePal.

## Directory Structure

```
TruePal.Api/
├── wwwroot/css/
│   ├── theme.css                      # Global theme variables & utilities
│   ├── components/                     # Reusable component styles
│   │   ├── cards.css                  # Card components (flex-card, post-card)
│   │   ├── panels.css                 # Panel components (posts-panel, sidebars)
│   │   └── overlays.css               # Overlay components (auth, modals)
│   └── pages/                          # Page-specific styles
│       ├── home.css                   # Home page styles (map, pins)
│       └── dashboard.css              # Dashboard page styles (future)
│
└── Pages/
    └── Shared/
        └── Components/                 # Reusable HTML partials
            ├── _AuthOverlay.cshtml    # Auth call-to-action overlay
            ├── _MapOverlay.cshtml     # Map header overlay
            └── _PostsPanel.cshtml     # Posts slide-out panel
```

## CSS Layer Hierarchy

### 1. Theme Layer (`theme.css`)
**Purpose**: Global design system  
**Contains**:
- CSS custom properties (--primary-yellow, --spacing-md, etc.)
- Utility classes (.text-primary, .m-lg, etc.)
- Global component overrides (cards, buttons, forms)
- Shared avatar and flex-card base styles

**Loading**: Loaded globally in `_Layout.cshtml`

### 2. Component Layer (`css/components/`)
**Purpose**: Reusable UI components  
**Contains**:
- Component-specific styles that can be used across multiple pages
- Self-contained, single-responsibility stylesheets

**Files**:
- `cards.css` - Card components (post cards, user cards)
- `panels.css` - Sliding panels and sidebars
- `overlays.css` - Modals, auth overlays, floating UI

**Loading**: Load per-page as needed via `@section Styles`

### 3. Page Layer (`css/pages/`)
**Purpose**: Page-specific layouts and overrides  
**Contains**:
- Layout styles unique to specific pages
- Page-specific component arrangements
- Custom animations and interactions for that page

**Loading**: Load on specific pages via `@section Styles`

## Component Usage

### Including CSS Components

```cshtml
@section Styles {
    <!-- Include only what you need -->
    <link rel="stylesheet" href="~/css/components/cards.css" asp-append-version="true">
    <link rel="stylesheet" href="~/css/components/panels.css" asp-append-version="true">
    <link rel="stylesheet" href="~/css/pages/your-page.css" asp-append-version="true">
}
```

### Using HTML Partials

```cshtml
<!-- Simple partial -->
<partial name="Shared/Components/_AuthOverlay" />

<!-- Partial with model (future) -->
<partial name="Shared/Components/_PostCard" model="@postData" />
```

## Best Practices

### CSS Organization

1. **Single Responsibility**: Each CSS file should have one clear purpose
2. **Component Isolation**: Components should not depend on each other
3. **Use CSS Variables**: Reference theme variables instead of hardcoded values
4. **Mobile-First**: Write base styles for mobile, use `@media` for desktop

### Creating New Components

#### 1. Create Component CSS
```
wwwroot/css/components/your-component.css
```

#### 2. Create Component Partial
```
Pages/Shared/Components/_YourComponent.cshtml
```

#### 3. Use in Pages
```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/components/your-component.css" asp-append-version="true">
}

<partial name="Shared/Components/_YourComponent" />
```

### Naming Conventions

- **CSS Files**: kebab-case (e.g., `post-card.css`)
- **CSS Classes**: kebab-case with BEM-inspired structure (e.g., `.flex-card__header`)
- **Partials**: PascalCase with underscore prefix (e.g., `_PostCard.cshtml`)
- **Components**: Descriptive and purpose-driven names

## Example: Creating a New Feature

Let's say you want to add a "comments section" component:

### Step 1: Create CSS Component
**File**: `wwwroot/css/components/comments.css`
```css
.comment-section {
    padding: var(--spacing-lg);
    background: var(--bg-white);
}

.comment-item {
    padding: var(--spacing-md);
    border-bottom: 1px solid #eee;
}
```

### Step 2: Create HTML Partial
**File**: `Pages/Shared/Components/_Comments.cshtml`
```cshtml
<div class="comment-section">
    <h3>Comments</h3>
    <!-- Comment items -->
</div>
```

### Step 3: Use in Page
**File**: `Pages/Post/Details.cshtml`
```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/components/comments.css" asp-append-version="true">
}

<partial name="Shared/Components/_Comments" />
```

## Maintenance Guidelines

### Adding New Styles

1. ✅ **DO** add to component CSS if reusable across pages
2. ✅ **DO** add to page CSS if specific to one page
3. ✅ **DO** add to theme CSS if it's a global utility or variable
4. ❌ **DON'T** add inline styles in .cshtml files
5. ❌ **DON'T** duplicate styles across files

### Refactoring Existing Code

1. Identify duplicated styles across pages
2. Extract to component CSS file
3. Create reusable partial if HTML is duplicated
4. Update pages to use new components
5. Remove old duplicated code

### Performance Considerations

- Only load CSS you need on each page
- Use `asp-append-version="true"` for cache busting
- Minify CSS in production
- Consider critical CSS for above-the-fold content

## Future Enhancements

- [ ] Add dashboard-specific styles to `css/pages/dashboard.css`
- [ ] Create profile card component
- [ ] Add animation utilities to theme
- [ ] Implement dark mode via CSS variables
- [ ] Create component library documentation

## Questions?

For questions about the architecture or to propose changes, please:
1. Review this document first
2. Check existing components for similar patterns
3. Discuss in team meetings before major changes

---
**Last Updated**: April 13, 2026  
**Maintainers**: TruePal Development Team
