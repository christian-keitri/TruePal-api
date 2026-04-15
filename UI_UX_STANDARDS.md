# TruePal.Api - UI/UX Standards & Design Rules

## 🎯 Purpose
This document defines the mandatory UI/UX standards for TruePal.Api. Following these rules ensures a professional, consistent, and accessible user experience across the entire application.

---

## 📋 Table of Contents
1. [Design System Rules](#design-system-rules)
2. [CSS Variables Usage](#css-variables-usage)
3. [Component Design Standards](#component-design-standards)
4. [Typography Rules](#typography-rules)
5. [Color Usage Rules](#color-usage-rules)
6. [Spacing & Layout Rules](#spacing--layout-rules)
7. [Responsive Design Rules](#responsive-design-rules)
8. [Form Design Standards](#form-design-standards)
9. [Icon Usage Rules](#icon-usage-rules)
10. [Interactive Elements](#interactive-elements)
11. [Accessibility Standards](#accessibility-standards)
12. [Animation Guidelines](#animation-guidelines)

---

## 🎨 Design System Rules

### ✅ RULE 1: Always Use CSS Variables - Never Hardcode Values
**❌ BAD - Hardcoded values**
```css
.my-component {
    background-color: #FFC107; /* ❌ Hardcoded */
    padding: 16px;
    border-radius: 8px;
    color: #333;
}
```

**✅ GOOD - CSS variables**
```css
.my-component {
    background-color: var(--color-primary); /* ✅ Theme variable */
    padding: var(--spacing-md);
    border-radius: var(--radius-md);
    color: var(--text-primary);
}
```

**Why:** Consistency, easy theme changes, maintainability.

**Available Variables:** See [THEME_GUIDE.md](THEME_GUIDE.md)

---

### ✅ RULE 2: Use Bootstrap Grid System for All Layouts
**❌ BAD - Custom grid or floats**
```html
<div style="width: 33.33%; float: left;">
```

**✅ GOOD - Bootstrap grid**
```html
<div class="row">
    <div class="col-md-4">
        <!-- Content -->
    </div>
    <div class="col-md-8">
        <!-- Content -->
    </div>
</div>
```

**Why:** Responsive by default, tested, consistent.

---

### ✅ RULE 3: All New Components Must Be Reusable
**If a component is used 2+ times, create it as a reusable component:**

1. **Create CSS** in `wwwroot/css/components/component-name.css`
2. **Create Partial** in `Views/Shared/Components/_ComponentName.cshtml`
3. **Document** in CSS_ARCHITECTURE.md

**❌ BAD - Duplicate code**
```html
<!-- Page 1 -->
<div class="card shadow">...</div>

<!-- Page 2 -->
<div class="card shadow">...</div>
```

**✅ GOOD - Reusable partial**
```html
<!-- Both pages -->
<partial name="Components/_Card" model="@cardData" />
```

---

---

### ✅ RULE 4: All Features Must Support Dark Mode AND Light Mode
**Mandatory:** Every UI component, page, and feature must work correctly in both dark and light themes.

**❌ BAD - Hardcoded colors**
```css
.my-card {
    background: #1E1E1E;  /* ❌ Only works in dark mode */
    color: white;         /* ❌ Invisible in light mode */
}
```

**✅ GOOD - Theme-aware colors**
```css
.my-card {
    background: var(--color-dark-lighter); /* ✅ Adapts to theme */
    color: var(--text-white);              /* ✅ Adapts to theme */
}
```

**Theme Testing Checklist:**
1. Click the theme toggle in navbar (moon/sun icon)
2. Verify your component looks correct in BOTH dark and light modes
3. Check for text contrast and readability
4. Ensure no hardcoded colors that break in one theme

**Available themes:**
- `[data-theme="dark"]` - Default
- `[data-theme="light"]` - User toggleable

**See:** [THEME_GUIDE.md](THEME_GUIDE.md) for complete variable list and theme-specific overrides.

---

## 🔧 CSS Variables Usage

### ✅ RULE 4: Use Spacing Variables - No Hardcoded Pixel Values
**Available Spacing:**
```css
var(--spacing-xs)   /* 4px  - Tiny gaps */
var(--spacing-sm)   /* 8px  - Small gaps */
var(--spacing-md)   /* 16px - Standard gaps */
var(--spacing-lg)   /* 24px - Large gaps */
var(--spacing-xl)   /* 32px - Extra large */
var(--spacing-2xl)  /* 48px - Section spacing */
var(--spacing-3xl)  /* 64px - Major sections */
```

**❌ BAD**
```css
.card {
    padding: 20px; /* ❌ Random value */
    margin-bottom: 15px;
}
```

**✅ GOOD**
```css
.card {
    padding: var(--spacing-lg); /* ✅ Consistent */
    margin-bottom: var(--spacing-md);
}
```

---

### ✅ RULE 5: Use Border Radius Variables
**Available Radii:**
```css
var(--radius-sm)     /* 4px  - Small, subtle */
var(--radius-md)     /* 8px  - Standard cards */
var(--radius-lg)     /* 16px - Large cards */
var(--radius-xl)     /* 24px - Hero sections */
var(--radius-pill)   /* 20px - Pills/badges */
var(--radius-circle) /* 50%  - Avatars */
```

**Usage:**
```css
.card          { border-radius: var(--radius-md); }
.avatar        { border-radius: var(--radius-circle); }
.badge         { border-radius: var(--radius-pill); }
```

---

### ✅ RULE 6: Use Shadow Variables for Depth
**Available Shadows:**
```css
var(--shadow-sm)  /* Subtle lift */
var(--shadow-md)  /* Standard cards */
var(--shadow-lg)  /* Modals, overlays */
var(--shadow-xl)  /* Hero elements */
```

**Usage Hierarchy:**
- `.shadow-sm` - Hover states, minor elevation
- `.shadow-md` - Cards, panels (default)
- `.shadow-lg` - Modals, popovers
- `.shadow-xl` - Hero sections, main CTAs

---

## 🧩 Component Design Standards

### ✅ RULE 7: Follow Component Naming Convention
**Pattern:** `component-type-modifier`

**Examples:**
```css
.card-post           /* Post card variant */
.card-user           /* User card variant */
.btn-primary         /* Primary button */
.panel-posts         /* Posts panel */
.overlay-auth        /* Auth overlay */
```

**❌ BAD**
```css
.my-custom-thing
.stuff
.box123
```

---

### ✅ RULE 8: Components Must Be Self-Contained
**Each component should:**
- ✅ Work independently
- ✅ Not depend on parent styles
- ✅ Have clear boundaries
- ✅ Be testable in isolation

**❌ BAD - Dependent on parent**
```css
.page-container .my-card {
    /* Only works inside .page-container */
}
```

**✅ GOOD - Self-contained**
```css
.card-post {
    /* Works anywhere */
}
```

---

### ✅ RULE 9: Use Semantic HTML Elements
**❌ BAD - Non-semantic**
```html
<div class="header">
    <div class="nav">
```

**✅ GOOD - Semantic**
```html
<header>
    <nav>
```

**Required Semantic Elements:**
- `<header>`, `<nav>`, `<main>`, `<footer>`
- `<article>`, `<section>`, `<aside>`
- `<button>` for clicks (not `<div onclick>`)
- `<a>` for links (not `<span onclick>`)

---

## 📝 Typography Rules

### ✅ RULE 10: Use Bootstrap Typography Classes
**Headings:**
```html
<h1 class="display-1">Hero Title</h1>     <!-- Largest -->
<h1 class="display-4">Large Title</h1>
<h1>Page Title</h1>                       <!-- Standard h1 -->
<h2>Section Title</h2>
<h3>Subsection Title</h3>
```

**Text Utilities:**
```html
<p class="lead">Larger introductory text</p>
<p class="text-muted">Secondary information</p>
<small class="text-muted">Metadata, timestamps</small>
```

---

### ✅ RULE 11: Maintain Typography Hierarchy
**Required Scale:**
1. **Display** - Hero sections only
2. **H1** - Page titles (one per page)
3. **H2** - Main sections
4. **H3** - Subsections
5. **H4** - Minor headings
6. **Body** - Regular content
7. **Small** - Metadata, captions

**❌ BAD - No hierarchy**
```html
<h1>Welcome</h1>
<h1>Another Title</h1>  <!-- Multiple h1 -->
<h3>Section</h3>
<h1>Random</h1>         <!-- Out of order -->
```

**✅ GOOD - Clear hierarchy**
```html
<h1>Welcome to TruePal</h1>
<h2>Featured Posts</h2>
<h3>Post Title</h3>
<p>Post content...</p>
```

---

### ✅ RULE 12: Never Use Font Size Below 14px
**Minimum Sizes:**
- Body text: `16px` (1rem)
- Small text: `14px` (0.875rem)
- Metadata: `14px` minimum

**❌ BAD**
```css
.tiny-text { font-size: 10px; } /* ❌ Too small */
```

**✅ GOOD**
```html
<small class="text-muted">Posted 2 hours ago</small> <!-- 14px -->
```

---

## 🎨 Color Usage Rules

### ✅ RULE 13: Use Brand Colors Consistently
**Primary Goldenrod (`var(--color-primary)` / `#FFC107`):**
- ✅ Buttons (primary actions)
- ✅ Icons (brand elements)
- ✅ Links (hover states)
- ✅ Highlights and accents

**Jet Black (`var(--color-dark)` / `#121212`):**
- ✅ Page backgrounds (dark theme)
- ✅ Card backgrounds (`var(--color-dark-lighter)` / `#1E1E1E`)

**❌ Don't Use Primary Yellow For:**
- ❌ Large background areas (use dark tones instead)
- ❌ Body text (use `var(--text-white)` on dark or `var(--text-primary)` on light)
- ❌ Low-contrast text pairings

---

### ✅ RULE 14: Use Contextual Colors for States
**Bootstrap Contextual Classes:**
```html
<div class="alert alert-success">Success message</div>
<div class="alert alert-danger">Error message</div>
<div class="alert alert-warning">Warning message</div>
<div class="alert alert-info">Info message</div>
```

**Buttons:**
```html
<button class="btn btn-success">Save</button>
<button class="btn btn-danger">Delete</button>
<button class="btn btn-warning">Cancel</button>
<button class="btn btn-primary">Primary Action</button>
```

---

### ✅ RULE 15: Ensure Sufficient Color Contrast
**WCAG AA Standards:**
- Normal text: 4.5:1 contrast ratio
- Large text (18px+): 3:1 contrast ratio
- Interactive elements: 3:1 minimum

**❌ BAD - Poor contrast**
```css
.card {
    background: #FFC107; /* Yellow */
    color: #ffffff;      /* White on yellow - insufficient contrast */
}
```

**✅ GOOD - Good contrast**
```css
/* Dark theme (default) */
.card {
    background: var(--color-dark-lighter); /* Charcoal #1E1E1E */
    color: var(--text-white);              /* White on dark */
}

/* Light surfaces */
.card-light {
    background: var(--bg-white);
    color: var(--text-primary); /* Jet Black on white */
}

.btn-primary {
    background: var(--color-primary); /* Goldenrod */
    color: #000;                      /* Black on yellow (high contrast) */
}
```

**Tool:** Use browser DevTools or [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)

---

## 📏 Spacing & Layout Rules

### ✅ RULE 16: Use Consistent Spacing Scale
**Component Spacing:**
- Cards: `padding: var(--spacing-lg)` (24px)
- Buttons: `padding: var(--spacing-sm) var(--spacing-md)` (8px 16px)
- Forms: `margin-bottom: var(--spacing-md)` (16px)

**Section Spacing:**
- Between sections: `var(--spacing-2xl)` (48px)
- Between cards: `var(--spacing-md)` (16px)
- Between form fields: `var(--spacing-md)` (16px)

---

### ✅ RULE 17: Follow Container Width Standards
**Container Types:**
```html
<div class="container">      <!-- Max 1140px, responsive -->
<div class="container-fluid"> <!-- Full width -->
```

**❌ BAD - Custom widths**
```css
.my-container {
    max-width: 1050px; /* ❌ Non-standard */
}
```

**✅ GOOD - Bootstrap containers**
```html
<div class="container">
    <!-- Content fits standard breakpoints -->
</div>
```

---

### ✅ RULE 18: Use Proper Margin/Padding Direction
**Convention:**
- Use **margin-bottom** for vertical spacing (not top)
- Use **padding** inside components
- Use **gap** for flexbox/grid spacing

**❌ BAD - Inconsistent**
```css
.card:first-child { margin-top: 20px; }
.card:last-child  { margin-bottom: 20px; }
```

**✅ GOOD - Consistent bottom margins**
```css
.card {
    margin-bottom: var(--spacing-md);
}

.card:last-child {
    margin-bottom: 0;
}
```

---

## 📱 Responsive Design Rules

### ✅ RULE 19: Mobile-First Approach Required
**Write base styles for mobile, then enhance for desktop:**

**❌ BAD - Desktop-first**
```css
.card {
    width: 33.33%; /* Desktop */
}

@media (max-width: 768px) {
    .card { width: 100%; }
}
```

**✅ GOOD - Mobile-first**
```css
.card {
    width: 100%; /* Mobile */
}

@media (min-width: 768px) {
    .card { width: 50%; }
}

@media (min-width: 1200px) {
    .card { width: 33.33%; }
}
```

---

### ✅ RULE 20: Use Bootstrap Breakpoints
**Standard Breakpoints:**
```css
/* Mobile: < 576px (default) */
@media (min-width: 576px)  { /* sm - Small tablets */ }
@media (min-width: 768px)  { /* md - Tablets */ }
@media (min-width: 992px)  { /* lg - Desktops */ }
@media (min-width: 1200px) { /* xl - Large desktops */ }
@media (min-width: 1400px) { /* xxl - Extra large */ }
```

**❌ Don't Create Custom Breakpoints**

---

### ✅ RULE 21: Test All Layouts on Mobile
**Required Device Tests:**
- ✅ iPhone SE (375px)
- ✅ iPhone 12/13 (390px)
- ✅ iPad (768px)
- ✅ Desktop (1920px)

**All interfaces must be fully functional on mobile.**

---

## 📋 Form Design Standards

### ✅ RULE 22: Use Bootstrap Form Controls
**✅ REQUIRED Structure:**
```html
<div class="mb-3">
    <label for="email" class="form-label">Email Address</label>
    <input type="email" class="form-control" id="email" 
           placeholder="you@example.com" required>
    <div class="invalid-feedback">
        Please provide a valid email.
    </div>
</div>
```

**Required Elements:**
- `<label>` with `for` attribute
- `class="form-control"` on inputs
- `class="form-label"` on labels
- `.invalid-feedback` for errors
- Proper `type` attributes (email, password, etc.)

---

### ✅ RULE 23: Form Input States Must Be Clear
**Required States:**
```html
<input class="form-control">           <!-- Default -->
<input class="form-control" disabled>  <!-- Disabled -->
<input class="form-control is-valid">  <!-- Valid -->
<input class="form-control is-invalid"><!-- Invalid -->
```

**Visual Indicators:**
- ✅ Green border for valid
- ✅ Red border for invalid
- ✅ Gray/dimmed for disabled
- ✅ Focus ring on active

---

### ✅ RULE 24: All Forms Must Have Clear Submit Buttons
**❌ BAD - Unclear action**
```html
<button class="btn btn-secondary">OK</button>
```

**✅ GOOD - Clear action**
```html
<button type="submit" class="btn btn-primary">
    <i class="bi bi-check-circle me-2"></i>Save Changes
</button>
```

**Button Text Rules:**
- Use action verbs: "Save", "Delete", "Submit", "Create"
- Add icons for clarity
- Primary button for main action
- Secondary/Outline for cancel

---

## 🎯 Icon Usage Rules

### ✅ RULE 25: Use Bootstrap Icons Consistently
**❌ BAD - Mixed icon sets**
```html
<i class="fa fa-user"></i>      <!-- Font Awesome -->
<i class="bi bi-heart"></i>     <!-- Bootstrap Icons -->
```

**✅ GOOD - Consistent set (Bootstrap Icons)**
```html
<i class="bi bi-person-circle"></i>
<i class="bi bi-heart-fill"></i>
<i class="bi bi-geo-alt"></i>
```

**Why:** Consistency, smaller bundle size.

---

### ✅ RULE 26: Icons Must Have Proper Sizes
**Standard Sizes:**
```html
<!-- Default: 1em (inherits) -->
<i class="bi bi-heart"></i>

<!-- Specific sizes -->
<i class="bi bi-heart" style="font-size: 1.5rem;"></i>
<i class="bi bi-heart" style="font-size: 2rem;"></i>
<i class="bi bi-heart" style="font-size: 3rem;"></i>
```

**Theme Classes:**
```html
<i class="bi bi-heart icon-theme"></i>      <!-- Yellow, default -->
<i class="bi bi-star icon-theme-lg"></i>    <!-- Yellow, 2rem -->
<i class="bi bi-trophy icon-theme-xl"></i>  <!-- Yellow, 3rem -->
```

---

### ✅ RULE 27: Decorative Icons Must Have aria-hidden
**✅ REQUIRED:**
```html
<!-- Decorative icon (has adjacent text) -->
<button>
    <i class="bi bi-save" aria-hidden="true"></i>
    Save
</button>

<!-- Functional icon (no text) -->
<button aria-label="Save">
    <i class="bi bi-save"></i>
</button>
```

---

## 🖱️ Interactive Elements

### ✅ RULE 28: All Clickable Elements Must Show Hover State
**Required Hover Styles:**
```css
.btn:hover {
    transform: translateY(-1px);
    box-shadow: var(--shadow-md);
    transition: var(--transition-fast);
}

.card:hover {
    box-shadow: var(--shadow-lg);
    transition: var(--transition-base);
}

a:hover {
    color: var(--color-primary-dark);
    text-decoration: underline;
}
```

---

### ✅ RULE 29: Use Cursor Pointer for Interactive Elements
**✅ REQUIRED:**
```css
.card-clickable,
.btn,
button,
a,
[onclick] {
    cursor: pointer;
}
```

---

### ✅ RULE 30: Disable States Must Be Obvious
**✅ REQUIRED Visual Changes:**
```css
.btn:disabled,
.btn.disabled {
    opacity: 0.6;
    cursor: not-allowed;
    pointer-events: none;
}
```

---

## ♿ Accessibility Standards

### ✅ RULE 31: All Images Must Have Alt Text
**❌ BAD**
```html
<img src="photo.jpg">
```

**✅ GOOD**
```html
<img src="photo.jpg" alt="User profile photo">
```

**For decorative images:**
```html
<img src="decoration.jpg" alt="" role="presentation">
```

---

### ✅ RULE 32: Maintain Proper Focus Order
**Rules:**
- Tab order must be logical (left-to-right, top-to-bottom)
- Focus must be visible (outline or custom style)
- Skip links for keyboard users

**✅ REQUIRED Focus Styles:**
```css
a:focus,
button:focus,
input:focus {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
}
```

---

### ✅ RULE 33: Use ARIA Labels for Icon-Only Buttons
**✅ REQUIRED:**
```html
<button aria-label="Close">
    <i class="bi bi-x"></i>
</button>

<button aria-label="Edit profile">
    <i class="bi bi-pencil"></i>
</button>
```

---

## 🎬 Animation Guidelines

### ✅ RULE 34: Use Transition Variables
**Available Transitions:**
```css
var(--transition-fast)  /* 0.15s - Hovers, small changes */
var(--transition-base)  /* 0.3s  - Standard animations */
var(--transition-slow)  /* 0.5s  - Large transformations */
```

**Usage:**
```css
.card {
    transition: all var(--transition-base);
}

.btn:hover {
    transition: transform var(--transition-fast);
}
```

---

### ✅ RULE 35: Animations Must Be Subtle
**Allowed Animations:**
- ✅ Fade in/out
- ✅ Slide in/out
- ✅ Scale (small, max 1.05x)
- ✅ Hover lifts (max 2px)

**❌ Avoid:**
- ❌ Spinning elements (unless loading)
- ❌ Pulsing/flashing (accessibility issue)
- ❌ Automatic carousels
- ❌ Parallax effects

---

### ✅ RULE 36: Respect prefers-reduced-motion
**✅ REQUIRED:**
```css
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
    }
}
```

**Already included in theme.css**

---

## 🧪 Component Checklist

When creating a new component, verify:

- [ ] **CSS Variables** - No hardcoded colors, spacing, or radii
- [ ] **Responsive** - Works on mobile (375px) to desktop (1920px)
- [ ] **Reusable** - Self-contained, no parent dependencies
- [ ] **Accessible** - Proper ARIA labels, alt text, focus states
- [ ] **Semantic HTML** - Uses proper HTML5 elements
- [ ] **Hover States** - Clear visual feedback on interaction
- [ ] **Loading States** - Spinners or skeletons for async content
- [ ] **Error States** - Clear error messages and styling
- [ ] **Empty States** - Helpful messages when no content
- [ ] **Contrast** - Meets WCAG AA standards (4.5:1)
- [ ] **Icons** - Bootstrap Icons with proper sizing
- [ ] **Typography** - Follows hierarchy rules
- [ ] **Spacing** - Uses spacing variables consistently
- [ ] **Documentation** - Added to CSS_ARCHITECTURE.md

---

## 📊 Quality Standards

### Design Quality Metrics
- **Mobile Performance**: < 3s load time on 3G
- **Accessibility Score**: WCAG AA compliant
- **Consistency**: 100% use of design system variables
- **Browser Support**: Chrome, Firefox, Safari, Edge (latest 2 versions)

### Code Quality Metrics
- **CSS Validation**: No errors on W3C validator
- **No Inline Styles**: (Exceptions: dynamic values from backend)
- **No !important**: (Exceptions: utility overrides only)
- **Reusability**: Components used 2+ times must be in /components/

---

## 🚫 Common UI/UX Mistakes to Avoid

### ❌ Don't Do These:
1. **Fixed pixel values** instead of CSS variables
2. **Inline styles** instead of CSS classes
3. **Multiple icon libraries** (stick to Bootstrap Icons)
4. **Desktop-only designs** (mobile-first required)
5. **Poor color contrast** (must meet WCAG AA)
6. **Missing hover states** (all interactive elements need feedback)
7. **No loading states** (show spinners for async operations)
8. **Inconsistent spacing** (use spacing variables)
9. **Too many font sizes** (stick to typography scale)
10. **Inaccessible forms** (labels, ARIA, validation required)

---

## 📚 Related Documentation

- [THEME_GUIDE.md](THEME_GUIDE.md) - CSS variables reference
- [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) - File organization
- [CODING_STANDARDS.md](CODING_STANDARDS.md) - Code quality rules

---

## ⚖️ Enforcement

**These are MANDATORY standards.** All pull requests must:
1. ✅ Use CSS variables (no hardcoded values)
2. ✅ Be responsive (mobile-first)
3. ✅ Be accessible (WCAG AA)
4. ✅ Use Bootstrap components
5. ✅ Follow naming conventions
6. ✅ Include hover/focus states
7. ✅ Pass W3C validation
8. ✅ Be documented in CSS_ARCHITECTURE.md

**Design review will reject PRs that violate these standards.**

---

**Last Updated:** April 15, 2026
**Version:** 2.0
