# UI/UX Quick Reference Card

> 📖 **Full Documentation:** [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)

## 🎨 CSS Variables (Always Use These!)

### Colors
```css
var(--primary-yellow)       /* #c9a961 - Brand color */
var(--primary-yellow-dark)  /* Hover states */

var(--text-primary)         /* #333 - Main text */
var(--text-secondary)       /* #666 - Secondary text */
var(--text-muted)           /* #999 - Metadata */

var(--bg-primary)           /* #f5f5f5 - Light gray */
var(--bg-white)             /* White backgrounds */
```

### Spacing
```css
var(--spacing-xs)   /* 4px  */
var(--spacing-sm)   /* 8px  */
var(--spacing-md)   /* 16px - Most common */
var(--spacing-lg)   /* 24px */
var(--spacing-xl)   /* 32px */
var(--spacing-2xl)  /* 48px - Section gaps */
```

### Border Radius
```css
var(--radius-sm)     /* 4px  - Subtle */
var(--radius-md)     /* 8px  - Standard */
var(--radius-lg)     /* 16px - Large cards */
var(--radius-circle) /* 50%  - Avatars */
```

### Shadows
```css
var(--shadow-sm)  /* Hover states */
var(--shadow-md)  /* Cards (default) */
var(--shadow-lg)  /* Modals */
```

### Transitions
```css
var(--transition-fast)  /* 0.15s - Hovers */
var(--transition-base)  /* 0.3s  - Standard */
var(--transition-slow)  /* 0.5s  - Large changes */
```

---

## 📱 Responsive Breakpoints

```css
/* Mobile-first (write base styles for mobile first!) */

/* Default: < 576px (Mobile) */

@media (min-width: 576px)  { /* sm - Small tablets */ }
@media (min-width: 768px)  { /* md - Tablets */ }
@media (min-width: 992px)  { /* lg - Desktops */ }
@media (min-width: 1200px) { /* xl - Large desktops */ }
```

---

## 🧩 Component Naming

```
component-type-modifier

✅ Examples:
.card-post           ← Post card variant
.card-user           ← User card variant
.btn-primary         ← Primary button
.panel-posts         ← Posts panel
```

---

## 📝 Typography Scale

```html
<h1 class="display-1">Hero Title</h1>     <!-- Largest -->
<h1>Page Title</h1>                       <!-- One per page -->
<h2>Section Title</h2>
<h3>Subsection</h3>
<p>Body text (16px minimum)</p>
<small>Metadata (14px minimum)</small>
```

---

## 🎯 Bootstrap Icons

```html
<!-- Standard sizes -->
<i class="bi bi-heart"></i>                           <!-- Default -->
<i class="bi bi-star" style="font-size: 2rem;"></i>  <!-- 2rem -->

<!-- Theme colors -->
<i class="bi bi-heart icon-theme"></i>      <!-- Yellow -->
<i class="bi bi-star icon-theme-lg"></i>    <!-- Yellow, 2rem -->

<!-- Accessibility -->
<button aria-label="Save">                  <!-- Icon-only -->
    <i class="bi bi-save"></i>
</button>

<button>                                    <!-- Icon + Text -->
    <i class="bi bi-save" aria-hidden="true"></i>
    Save
</button>
```

---

## 📋 Form Structure

```html
<div class="mb-3">
    <label for="email" class="form-label">Email</label>
    <input type="email" 
           class="form-control" 
           id="email" 
           placeholder="you@example.com"
           required>
    <div class="invalid-feedback">
        Please provide a valid email.
    </div>
</div>
```

---

## 🎨 Bootstrap Contextual Colors

```html
<!-- Alerts -->
<div class="alert alert-success">Success!</div>
<div class="alert alert-danger">Error!</div>
<div class="alert alert-warning">Warning!</div>
<div class="alert alert-info">Info</div>

<!-- Buttons -->
<button class="btn btn-primary">Primary</button>
<button class="btn btn-success">Save</button>
<button class="btn btn-danger">Delete</button>
<button class="btn btn-secondary">Cancel</button>
```

---

## ✅ Always Do

- ✅ Use CSS variables (no hardcoded values)
- ✅ Mobile-first responsive design
- ✅ Bootstrap grid system (`row`, `col-md-*`)
- ✅ Semantic HTML (`<header>`, `<nav>`, `<main>`)
- ✅ Hover states on all interactive elements
- ✅ Alt text on all images
- ✅ ARIA labels on icon-only buttons
- ✅ Proper spacing scale (`var(--spacing-*)`)
- ✅ Bootstrap Icons only
- ✅ Focus visible (outline or custom style)

---

## ❌ Never Do

- ❌ Hardcode colors: `#c9a961` → Use `var(--primary-yellow)`
- ❌ Hardcode spacing: `padding: 20px` → Use `var(--spacing-lg)`
- ❌ Inline styles (except dynamic backend values)
- ❌ Desktop-first media queries
- ❌ Mix icon libraries (Bootstrap Icons only)
- ❌ Text smaller than 14px
- ❌ Poor contrast (must meet WCAG AA: 4.5:1)
- ❌ Multiple `<h1>` tags per page
- ❌ Images without `alt` text
- ❌ Icon-only buttons without `aria-label`

---

## 🖱️ Interactive States

```css
/* Required for all clickable elements */
.clickable-element {
    cursor: pointer;
    transition: var(--transition-fast);
}

.clickable-element:hover {
    transform: translateY(-1px);
    box-shadow: var(--shadow-md);
}

.clickable-element:focus {
    outline: 2px solid var(--primary-yellow);
    outline-offset: 2px;
}

.clickable-element:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}
```

---

## 📏 Spacing Guidelines

```
Card Padding:        var(--spacing-lg)      24px
Button Padding:      var(--spacing-sm) var(--spacing-md)  8px 16px
Form Field Margin:   var(--spacing-md)      16px
Section Gaps:        var(--spacing-2xl)     48px
Between Cards:       var(--spacing-md)      16px
```

---

## ♿ Accessibility Checklist

- [ ] All images have `alt` text or `alt=""` + `role="presentation"`
- [ ] Form labels have `for` attribute matching input `id`
- [ ] Icon-only buttons have `aria-label`
- [ ] Decorative icons have `aria-hidden="true"`
- [ ] Focus states are visible
- [ ] Color contrast meets WCAG AA (4.5:1)
- [ ] Keyboard navigation works (Tab order logical)

---

## 🔍 Testing Checklist

- [ ] Mobile: iPhone SE (375px)
- [ ] Tablet: iPad (768px)
- [ ] Desktop: 1920px
- [ ] All interactive elements have hover state
- [ ] All buttons have clear action text
- [ ] Forms show validation errors
- [ ] Loading states for async operations
- [ ] Empty states when no content

---

## 📚 Quick Links

- **Full UI/UX Standards:** [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)
- **CSS Variables Reference:** [THEME_GUIDE.md](THEME_GUIDE.md)
- **CSS File Organization:** [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md)
- **Coding Standards:** [CODING_STANDARDS.md](CODING_STANDARDS.md)

---

**💡 Tip:** Keep this file open while developing!

**🔗 W3C Contrast Checker:** https://webaim.org/resources/contrastchecker/
