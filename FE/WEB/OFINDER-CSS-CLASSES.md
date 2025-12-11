# OFinder Custom CSS Classes

This document lists all custom CSS classes available in the OFinder design system, beyond the standard Bootstrap 4 and ArchitectUI theme classes.

## Table of Contents

1. [Color Variables](#color-variables)
2. [Typography Classes](#typography-classes)
3. [Utility Classes](#utility-classes)
4. [Component Classes](#component-classes)
5. [Filter Panel Classes](#filter-panel-classes)
6. [Badge Classes](#badge-classes)
7. [Button Classes](#button-classes)

---

## Color Variables

The OFinder design system uses a dark theme with vibrant accent colors:

| Variable | Color | Description |
|----------|-------|-------------|
| `$ofinder-dark-bg` | `#1a0a2e` | Deep dark purple (main background) |
| `$ofinder-dark-surface` | `#2d1b4e` | Lighter surface for cards/panels |
| `$ofinder-dark-surface-light` | `#3d2b5e` | Even lighter for hover states |
| `$ofinder-primary` | `#db2777` | Vibrant pink |
| `$ofinder-primary-dark` | `#be185d` | Darker pink |
| `$ofinder-primary-light` | `#ec4899` | Lighter pink |
| `$ofinder-secondary` | `#f59e0b` | Vibrant gold |
| `$ofinder-accent` | `#f59e0b` | Gold accent (same as secondary) |
| `$ofinder-success` | `#4CAF50` | Success green |
| `$ofinder-warning` | `#FF9800` | Warning orange |
| `$ofinder-danger` | `#F44336` | Danger red |
| `$ofinder-info` | `#2196F3` | Info blue |

---

## Typography Classes

### Heading Classes

| Class | Description | Font Size | Font Weight |
|-------|-------------|-----------|-------------|
| `.ofinder-heading-xl` | Extra large heading | 2rem | Bold (700) |
| `.ofinder-heading-lg` | Large heading | 1.5rem | Bold (700) |
| `.ofinder-heading-md` | Medium heading | 1.125rem | Medium (500) |

### Text Size Classes

| Class | Description | Font Size |
|-------|-------------|-----------|
| `.ofinder-text-xs` | Extra small text | 0.75rem |
| `.ofinder-text-sm` | Small text | 0.875rem |

### Text Color Classes

| Class | Description | Color |
|-------|-------------|-------|
| `.ofinder-text-primary` | Primary text color | Pink (`#db2777`) |
| `.ofinder-text-secondary` | Secondary text color | Gold (`#f59e0b`) |
| `.ofinder-text-accent` | Accent text color | Gold (`#f59e0b`) |
| `.ofinder-text-muted` | Muted text color | Gray (`#9ca3af`) |

**Example:**
```html
<h1 class="ofinder-heading-xl ofinder-text-primary">Welcome to OFinder</h1>
<p class="ofinder-text-sm ofinder-text-muted">Browse thousands of performers</p>
```

---

## Utility Classes

### Background Colors

| Class | Description | Background Color |
|-------|-------------|------------------|
| `.ofinder-bg-primary` | Primary background | Pink with white text |
| `.ofinder-bg-secondary` | Secondary background | Gold with dark text |
| `.ofinder-bg-accent` | Accent background | Gold with dark text |
| `.ofinder-bg-dark` | Dark background | Deep purple with light text |
| `.ofinder-bg-surface` | Surface background | Dark surface with light text |

### Background Gradients

| Class | Description |
|-------|-------------|
| `.ofinder-bg-gradient-primary` | Pink gradient background |
| `.ofinder-bg-gradient-secondary` | Gold gradient background |

### Shadow Classes

| Class | Description | Shadow |
|-------|-------------|--------|
| `.ofinder-shadow-sm` | Small shadow | `0 2px 4px rgba(0,0,0,0.1)` |
| `.ofinder-shadow` | Medium shadow | `0 4px 6px rgba(0,0,0,0.1)` |
| `.ofinder-shadow-lg` | Large shadow | `0 10px 20px rgba(0,0,0,0.15)` |

### Border Radius Classes

| Class | Description | Border Radius |
|-------|-------------|---------------|
| `.ofinder-rounded-sm` | Small rounded corners | 0.25rem |
| `.ofinder-rounded` | Medium rounded corners | 0.5rem |
| `.ofinder-rounded-lg` | Large rounded corners | 1rem |

**Example:**
```html
<div class="ofinder-bg-gradient-primary ofinder-shadow-lg ofinder-rounded">
  Premium Content
</div>
```

---

## Component Classes

### Performer Card Classes

| Class | Description |
|-------|-------------|
| `.ofinder-performer-avatar` | Performer avatar image (300px height, cover fit) |
| `.performer-bio-truncated` | Truncates bio text to 2 lines with ellipsis |
| `.ofinder-badge-overlay-tr` | Position badge at top-right of card |
| `.ofinder-badge-overlay-tl` | Position badge at top-left of card |
| `.performer-channels` | Container for channel badges with gap |

**Example:**
```html
<div class="card">
  <img src="..." class="ofinder-performer-avatar" />
  <span class="ofinder-badge-overlay-tr ofinder-badge-verified">✓</span>
  <div class="card-body">
    <p class="performer-bio-truncated">Long bio text...</p>
  </div>
</div>
```

### Rating Stars Classes

| Class | Description |
|-------|-------------|
| `.rating-stars` | Container for rating stars component |
| `.rating-stars-sm` | Small size rating stars |
| `.rating-stars-md` | Medium size rating stars (default) |
| `.rating-stars-lg` | Large size rating stars |
| `.star-filled` | Filled star (gold color) |
| `.star-empty` | Empty star (gray color) |
| `.star-half` | Half-filled star (50% opacity) |

---

## Filter Panel Classes

### Main Filter Panel

| Class | Description |
|-------|-------------|
| `.filter-panel` | Main container for the filter panel |
| `.filter-panel-header` | Collapsible header (mobile) with hover effects |
| `.ofinder-filter-panel-title` | Main title "Filtri" with gold color and bold weight |
| `.filter-panel-footer` | Footer container for action buttons |

### Filter Sections

| Class | Description |
|-------|-------------|
| `.filter-section` | Container for each filter section |
| `.ofinder-filter-section-primary` | Primary filter section (e.g., "Cosa cerchi?") with gold bottom border |
| `.ofinder-filter-section-heading-primary` | Heading for primary filter section with gold color |
| `.filter-section-two-columns` | 2-column grid layout for long filter lists (e.g., content types) |
| `.content-type-category` | Container for categorized content type groups |
| `.content-type-category-title` | Category title with gold color and bottom border |

### Platform Icons

| Class | Description |
|-------|-------------|
| `.platform-icon-badge` | Badge for platform icons in filter panel (24x24px) |
| `.platform-icon-badge-inline` | Inline platform icon badge for channel badges (20x20px) |

**Example:**
```html
<div class="filter-panel">
  <div class="filter-panel-header">
    <h5 class="ofinder-heading-md ofinder-filter-panel-title">Filtri</h5>
  </div>

  <div class="filter-section ofinder-filter-section-primary">
    <h6 class="ofinder-filter-section-heading-primary">Cosa cerchi?</h6>
    <!-- Filter content -->
  </div>

  <div class="filter-section">
    <h6>Posizione</h6>
    <!-- Filter content -->
  </div>
</div>
```

---

## Badge Classes

### Status Badges

| Class | Description | Color | Use Case |
|-------|-------------|-------|----------|
| `.ofinder-badge-verified` | Verified badge | Green (`#4CAF50`) | Verified performers |
| `.ofinder-badge-premium` | Premium badge | Gold (`#f59e0b`) | Premium content |
| `.ofinder-badge-new` | New badge | Blue (`#2196F3`) | New performers (last 30 days) |

**Example:**
```html
<span class="ofinder-badge-verified ofinder-rounded-sm">✓</span>
<span class="ofinder-badge-premium ofinder-rounded-sm">Premium</span>
<span class="ofinder-badge-new ofinder-rounded-sm">New</span>
```

### Channel Badges

| Class | Description | Background Color |
|-------|-------------|------------------|
| `.channel-badge` | Base channel badge class | - |
| `.channel-badge-sm` | Small channel badge | - |
| `.channel-badge-onlyfans` | OnlyFans badge | Blue (`#00aff0`) |
| `.channel-badge-fansly` | Fansly badge | Purple (`#7b68ee`) |
| `.channel-badge-instagram` | Instagram badge | Pink (`#e4405f`) |
| `.channel-badge-twitter` | Twitter badge | Blue (`#1da1f2`) |
| `.channel-badge-tiktok` | TikTok badge | Black (`#000000`) |
| `.channel-badge-youtube` | YouTube badge | Red (`#ff0000`) |
| `.channel-badge-snapchat` | Snapchat badge | Yellow (`#fffc00`) |
| `.channel-badge-other` | Other platform badge | Gray (`#6c757d`) |

**Example:**
```html
<span class="channel-badge channel-badge-onlyfans">
  <span class="platform-icon-badge-inline">🔵</span>
  OnlyFans
  <span class="channel-badge-check">✓</span>
</span>
```

---

## Button Classes

### Custom Button Styles

| Class | Description | Background | Text Color |
|-------|-------------|------------|------------|
| `.btn-ofinder-primary` | Primary button | Pink | White |
| `.btn-ofinder-secondary` | Secondary button | Gold | Dark |

**Example:**
```html
<button class="btn btn-ofinder-primary">Apply Filters</button>
<button class="btn btn-ofinder-secondary">Clear All</button>
```

---

## Search Page Classes

### Search Bar

| Class | Description |
|-------|-------------|
| `.search-bar-container` | Container for search bar with dark surface and shadow |
| `.quick-filters` | Container for quick filter buttons |

### Performer Grid

| Class | Description |
|-------|-------------|
| `.performer-grid` | Grid container with flex layout for equal height cards |

**Example:**
```html
<div class="search-bar-container">
  <div class="input-group">
    <input type="text" class="form-control" placeholder="Search..." />
  </div>

  <div class="quick-filters d-flex">
    <button class="btn btn-outline-secondary">Verified</button>
    <button class="btn btn-outline-secondary">Premium</button>
  </div>
</div>

<div class="row performer-grid">
  <div class="col-md-4">
    <div class="card">
      <!-- Performer card content -->
    </div>
  </div>
</div>
```

---

## Info Boxes

| Class | Description | Border Color |
|-------|-------------|--------------|
| `.info-box` | Base info box class with left border | - |
| `.info-box-primary` | Primary info box | Pink |
| `.info-box-secondary` | Secondary info box | Purple |

**Example:**
```html
<div class="info-box info-box-primary p-3 mb-3">
  <strong>Note:</strong> Premium members get access to exclusive content.
</div>
```

---

## Design Principles

When using these classes, follow these principles:

1. **Avoid Inline Styles**: Always use CSS classes instead of inline `style` attributes
2. **Consistency**: Use the predefined color variables and spacing utilities
3. **Semantic Naming**: Class names describe what the element is, not how it looks
4. **Mobile First**: All components are designed mobile-first with responsive behavior
5. **Dark Theme**: The design system is built for a dark cinema-mode experience

---

## Usage Guidelines

### DO ✓

```html
<!-- Good: Using CSS classes -->
<h5 class="ofinder-heading-md ofinder-filter-panel-title">Filtri</h5>

<!-- Good: Combining OFinder classes with Bootstrap utilities -->
<div class="ofinder-bg-surface p-4 mb-3 ofinder-rounded">Content</div>

<!-- Good: Using semantic class names -->
<div class="filter-section ofinder-filter-section-primary">
```

### DON'T ✗

```html
<!-- Bad: Using inline styles -->
<h5 style="color: #f39c12; font-weight: 700;">Filtri</h5>

<!-- Bad: Creating ad-hoc inline styles -->
<div style="padding-bottom: 1rem; border-bottom: 2px solid #f39c12;">

<!-- Bad: Using arbitrary colors not from the design system -->
<span style="color: #123456;">Custom color</span>
```

---

## File Locations

- **Variables**: `/app/styles/custom/ofinder/_variables.scss`
- **Typography**: `/app/styles/custom/ofinder/_typography.scss`
- **Utilities**: `/app/styles/custom/ofinder/_utilities.scss`
- **Components**: `/app/styles/custom/ofinder/ofinder-components.scss`
- **Base Styles**: `/app/styles/custom/ofinder/_base.scss`

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-12-11 | Initial documentation with all existing classes |
| 1.1.0 | 2025-12-11 | Added filter panel specific classes (`.ofinder-filter-panel-title`, `.ofinder-filter-section-primary`, `.ofinder-filter-section-heading-primary`) |
| 1.2.0 | 2025-12-11 | Added 2-column grid layout class (`.filter-section-two-columns`) for content types filter |
| 1.3.0 | 2025-12-11 | Added categorized content type classes (`.content-type-category`, `.content-type-category-title`) |

---

## Contributing

When adding new custom classes:

1. Follow the existing naming convention (`ofinder-*` prefix)
2. Add the class to the appropriate SCSS file
3. Document it in this file with description and example
4. Use existing color variables and spacing utilities
5. Test on mobile and desktop breakpoints
