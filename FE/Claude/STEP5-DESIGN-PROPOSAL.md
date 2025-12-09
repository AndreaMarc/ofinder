# STEP 5: Base Components - Design Proposal

## Panoramica

Questo documento presenta il design completo dei componenti base per OFinder, con mockup grafici testuali, struttura HTML e preview del comportamento.

---

## 1. PerformerCard Component

### Descrizione
Card responsiva che mostra le informazioni principali di un performer. Utilizzata in listing, search results, homepage.

### Mockup Grafico (Desktop)

```
┌─────────────────────────────────────────┐
│  ╔═══════════════════╗                  │
│  ║                   ║                  │
│  ║   [AVATAR IMG]    ║  ✓ Verified      │
│  ║     300x300       ║                  │
│  ║                   ║                  │
│  ╚═══════════════════╝                  │
│                                         │
│  Sofia Martinez            Premium     │
│  ★★★★★ 4.8 (127 reviews)              │
│                                         │
│  🔵 OnlyFans  🟣 Fansly  📸 Instagram  │
│                                         │
│  "Fitness & Lifestyle creator..."      │
│                                         │
│  💰 $9.99/month                        │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │     VISUALIZZA PROFILO    →     │   │
│  └─────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

### Mockup Grafico (Mobile)

```
┌──────────────────────┐
│  ╔════════════════╗  │
│  ║                ║  │
│  ║  [AVATAR IMG]  ║  │
│  ║                ║  │
│  ╚════════════════╝  │
│                      │
│  Sofia Martinez      │
│  ✓ Verified          │
│  ★★★★★ 4.8          │
│                      │
│  🔵 🟣 📸           │
│                      │
│  $9.99/mo            │
│                      │
│  ┌──────────────┐    │
│  │   VISUALIZZA │    │
│  └──────────────┘    │
└──────────────────────┘
```

### Struttura HTML Completa

```handlebars
{{!-- components/performer-card.hbs --}}
<div class="card ofinder-shadow ofinder-rounded overflow-hidden h-100">
  {{!-- Avatar Container --}}
  <div class="position-relative">
    <img
      src={{@performer.avatarUrl}}
      alt={{@performer.displayName}}
      class="card-img-top"
      style="height: 300px; object-fit: cover;"
    />

    {{!-- Badge Verified (top-right overlay) --}}
    {{#if @performer.verified}}
      <span class="ofinder-badge-verified position-absolute" style="top: 10px; right: 10px;">
        <i class="pe-7s-check mr-1"></i>
        Verified
      </span>
    {{/if}}

    {{!-- Badge Premium (top-left overlay) --}}
    {{#if @performer.isPremium}}
      <span class="ofinder-badge-premium position-absolute" style="top: 10px; left: 10px;">
        <i class="pe-7s-star mr-1"></i>
        Premium
      </span>
    {{/if}}
  </div>

  {{!-- Card Body --}}
  <div class="card-body">
    {{!-- Nome Performer --}}
    <h5 class="ofinder-heading-md mb-2">
      {{@performer.displayName}}
    </h5>

    {{!-- Rating Stars --}}
    <div class="mb-3">
      <RatingStars
        @rating={{@performer.averageRating}}
        @reviewCount={{@performer.reviewCount}}
        @size="sm"
      />
    </div>

    {{!-- Channel Badges (Social Platforms) --}}
    <div class="mb-3 d-flex flex-wrap gap-2">
      {{#each @performer.channels as |channel|}}
        <ChannelBadge @channel={{channel}} @size="sm" />
      {{/each}}
    </div>

    {{!-- Descrizione breve (troncata) --}}
    {{#if @performer.bio}}
      <p class="ofinder-text-sm ofinder-text-muted mb-3" style="
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
      ">
        {{@performer.bio}}
      </p>
    {{/if}}

    {{!-- Pricing --}}
    {{#if @performer.minPrice}}
      <div class="mb-3 d-flex align-items-center">
        <i class="pe-7s-wallet ofinder-text-accent mr-2"></i>
        <span class="ofinder-text-accent font-weight-bold">
          Da ${{@performer.minPrice}}/mese
        </span>
      </div>
    {{/if}}
  </div>

  {{!-- Card Footer con CTA --}}
  <div class="card-footer bg-transparent border-0 p-3">
    <button
      class="btn btn-block btn-ofinder-primary"
      {{on "click" (fn @onViewProfile @performer)}}
    >
      Visualizza Profilo
      <i class="pe-7s-angle-right ml-2"></i>
    </button>
  </div>
</div>
```

### Varianti

**1. Compact Mode** (per listing con tante card):
- Avatar 200x200px
- No descrizione
- Badge più piccoli

**2. Featured Mode** (homepage):
- Avatar 400x400px
- Gradiente overlay sul footer
- Animazione hover (scale + shadow)

---

## 2. SearchBar Component

### Descrizione
Barra di ricerca principale con autocomplete, filtri rapidi e bottone ricerca avanzata.

### Mockup Grafico (Desktop)

```
┌───────────────────────────────────────────────────────────────────┐
│  🔍  Cerca performer per nome, categoria, tag...    [🎯 Filtri]  │
└───────────────────────────────────────────────────────────────────┘
     ┌──────────────────────────────────────────────────────────┐
     │  Filtri rapidi:                                          │
     │  [ OnlyFans ]  [ Fansly ]  [ Fitness ]  [ 18-25 ]       │
     └──────────────────────────────────────────────────────────┘
```

### Mockup Grafico (Mobile)

```
┌────────────────────────┐
│  🔍  Cerca...    [🎯]  │
└────────────────────────┘
┌────────────────────────┐
│  [ OnlyFans ]          │
│  [ Fitness ]  [ 18-25 ]│
└────────────────────────┘
```

### Struttura HTML Completa

```handlebars
{{!-- components/search-bar.hbs --}}
<div class="search-bar-container mb-4">
  {{!-- Input Group principale --}}
  <div class="input-group input-group-lg ofinder-shadow-sm ofinder-rounded">
    {{!-- Icona Search --}}
    <div class="input-group-prepend">
      <span class="input-group-text bg-white border-0">
        <i class="pe-7s-search ofinder-text-primary" style="font-size: 24px;"></i>
      </span>
    </div>

    {{!-- Input field con autocomplete --}}
    <input
      type="text"
      class="form-control border-0"
      placeholder="Cerca performer per nome, categoria, tag..."
      value={{this.searchQuery}}
      {{on "input" this.handleInput}}
      {{on "keydown" this.handleKeydown}}
    />

    {{!-- Bottone Filtri Avanzati --}}
    <div class="input-group-append">
      <button
        class="btn btn-ofinder-secondary"
        {{on "click" this.toggleFilters}}
      >
        <i class="pe-7s-filter mr-2"></i>
        Filtri
        {{#if this.activeFiltersCount}}
          <span class="badge badge-light ml-2">{{this.activeFiltersCount}}</span>
        {{/if}}
      </button>
    </div>
  </div>

  {{!-- Autocomplete Dropdown (se ci sono risultati) --}}
  {{#if this.showAutocomplete}}
    <div class="dropdown-menu show w-100 ofinder-shadow mt-2">
      {{#each this.autocompleteResults as |result|}}
        <a
          href="#"
          class="dropdown-item d-flex align-items-center"
          {{on "click" (fn this.selectResult result)}}
        >
          <img
            src={{result.avatarUrl}}
            alt={{result.name}}
            class="rounded-circle mr-3"
            style="width: 40px; height: 40px; object-fit: cover;"
          />
          <div>
            <div class="font-weight-bold">{{result.name}}</div>
            <small class="ofinder-text-muted">
              {{result.category}} • {{result.platform}}
            </small>
          </div>
        </a>
      {{/each}}
    </div>
  {{/if}}

  {{!-- Filtri Rapidi (chips) --}}
  <div class="mt-3 d-flex flex-wrap gap-2">
    <span class="text-muted mr-2">Filtri rapidi:</span>

    {{#each this.quickFilters as |filter|}}
      <button
        class="btn btn-sm {{if filter.active 'btn-ofinder-primary' 'btn-outline-secondary'}} ofinder-rounded-sm"
        {{on "click" (fn this.toggleQuickFilter filter)}}
      >
        {{filter.icon}} {{filter.label}}
        {{#if filter.active}}
          <i class="pe-7s-close-circle ml-1"></i>
        {{/if}}
      </button>
    {{/each}}
  </div>
</div>
```

### Quick Filters Examples

```javascript
quickFilters = [
  { id: 'onlyfans', label: 'OnlyFans', icon: '🔵', active: false },
  { id: 'fansly', label: 'Fansly', icon: '🟣', active: false },
  { id: 'fitness', label: 'Fitness', icon: '💪', active: false },
  { id: 'gaming', label: 'Gaming', icon: '🎮', active: false },
  { id: 'verified', label: 'Verificati', icon: '✓', active: false },
  { id: 'new', label: 'Nuovi', icon: '🆕', active: false },
];
```

---

## 3. RatingStars Component

### Descrizione
Visualizzazione rating con stelle, voto numerico e conteggio recensioni.

### Mockup Grafico

```
★★★★☆ 4.3 (127 recensioni)
```

### Struttura HTML Completa

```handlebars
{{!-- components/rating-stars.hbs --}}
<div class="rating-stars d-inline-flex align-items-center {{if (eq @size 'sm') 'rating-stars-sm'}}">
  {{!-- Stelle (piene e vuote) --}}
  <div class="stars-container mr-2">
    {{#each this.starsArray as |star|}}
      <i class="{{star.iconClass}} {{star.colorClass}}"
         style="font-size: {{if (eq @size 'sm') '16px' '20px'}};"></i>
    {{/each}}
  </div>

  {{!-- Voto numerico --}}
  <span class="font-weight-bold mr-1" style="font-size: {{if (eq @size 'sm') '14px' '16px'}};">
    {{this.formattedRating}}
  </span>

  {{!-- Conteggio recensioni (opzionale) --}}
  {{#if @reviewCount}}
    <span class="ofinder-text-muted ofinder-text-xs">
      ({{@reviewCount}} {{if (eq @reviewCount 1) 'recensione' 'recensioni'}})
    </span>
  {{/if}}
</div>
```

### Logic (JavaScript)

```javascript
// components/rating-stars.js
import Component from '@glimmer/component';

export default class RatingStarsComponent extends Component {
  get formattedRating() {
    return this.args.rating?.toFixed(1) || '0.0';
  }

  get starsArray() {
    const rating = this.args.rating || 0;
    const stars = [];

    for (let i = 1; i <= 5; i++) {
      if (i <= Math.floor(rating)) {
        // Stella piena
        stars.push({
          iconClass: 'pe-7s-star',
          colorClass: 'ofinder-text-accent',
        });
      } else if (i === Math.ceil(rating) && rating % 1 !== 0) {
        // Mezza stella (o usa icona diversa)
        stars.push({
          iconClass: 'pe-7s-star',
          colorClass: 'ofinder-text-accent',
          style: 'opacity: 0.5;',
        });
      } else {
        // Stella vuota
        stars.push({
          iconClass: 'pe-7s-star',
          colorClass: 'text-muted',
        });
      }
    }

    return stars;
  }
}
```

### Varianti

**1. Interactive Mode** (per lasciare recensione):
- Stelle cliccabili
- Hover effect
- `@onChange` callback

**2. Size Variants**:
- `sm`: 16px (per card)
- `md`: 20px (default)
- `lg`: 28px (per dettaglio)

---

## 4. ChannelBadge Component

### Descrizione
Badge colorato per indicare la piattaforma social del performer (OnlyFans, Fansly, Instagram, ecc.)

### Mockup Grafico

```
[🔵 OnlyFans]  [🟣 Fansly]  [📸 Instagram]  [🐦 Twitter/X]
```

### Struttura HTML Completa

```handlebars
{{!-- components/channel-badge.hbs --}}
<span
  class="channel-badge ofinder-rounded-sm d-inline-flex align-items-center px-2 py-1"
  style="
    background-color: {{this.channelColor}};
    color: white;
    font-size: {{if (eq @size 'sm') '11px' '13px'}};
    font-weight: 500;
  "
>
  {{!-- Icona piattaforma --}}
  {{#if this.channelIcon}}
    <i class="{{this.channelIcon}} mr-1"></i>
  {{else}}
    <span class="mr-1">{{this.channelEmoji}}</span>
  {{/if}}

  {{!-- Nome piattaforma --}}
  {{this.channelName}}

  {{!-- Verified badge (se verificato) --}}
  {{#if @channel.verified}}
    <i class="pe-7s-check ml-1" style="font-size: 10px;"></i>
  {{/if}}
</span>
```

### Logic (JavaScript)

```javascript
// components/channel-badge.js
import Component from '@glimmer/component';

const CHANNEL_CONFIG = {
  onlyfans: {
    name: 'OnlyFans',
    color: '#00AFF0',
    emoji: '🔵',
    icon: null,
  },
  fansly: {
    name: 'Fansly',
    color: '#7B68EE',
    emoji: '🟣',
    icon: null,
  },
  instagram: {
    name: 'Instagram',
    color: '#E4405F',
    emoji: '📸',
    icon: null,
  },
  twitter: {
    name: 'Twitter/X',
    color: '#1DA1F2',
    emoji: '🐦',
    icon: null,
  },
  tiktok: {
    name: 'TikTok',
    color: '#000000',
    emoji: '🎵',
    icon: null,
  },
  youtube: {
    name: 'YouTube',
    color: '#FF0000',
    emoji: '📺',
    icon: null,
  },
};

export default class ChannelBadgeComponent extends Component {
  get channelConfig() {
    return CHANNEL_CONFIG[this.args.channel.platformType] || {
      name: 'Other',
      color: '#6C757D',
      emoji: '🌐',
    };
  }

  get channelName() {
    return this.channelConfig.name;
  }

  get channelColor() {
    return this.channelConfig.color;
  }

  get channelEmoji() {
    return this.channelConfig.emoji;
  }

  get channelIcon() {
    return this.channelConfig.icon;
  }
}
```

### Varianti

**1. Clickable Mode**:
- Href al profilo social
- Hover effect
- Target `_blank`

**2. Count Mode** (per statistiche):
```
[🔵 OnlyFans • 12.5K followers]
```

---

## 5. FilterPanel Component

### Descrizione
Pannello laterale (sidebar) con filtri avanzati per la ricerca. Collassabile su mobile.

### Mockup Grafico (Desktop)

```
┌─────────────────────────┐
│  FILTRI                 │
│  ─────────────────────  │
│                         │
│  PIATTAFORME            │
│  ☐ OnlyFans (1,234)    │
│  ☐ Fansly (567)        │
│  ☐ Instagram (890)     │
│                         │
│  CATEGORIE              │
│  ☐ Fitness (234)       │
│  ☐ Gaming (156)        │
│  ☐ Lifestyle (345)     │
│  ☐ Cosplay (123)       │
│                         │
│  PREZZO ($/mese)        │
│  [====•========] $25    │
│  Free - $50+            │
│                         │
│  RATING                 │
│  ☐ ★★★★★ (5 stelle)    │
│  ☐ ★★★★☆ (4+)          │
│  ☐ ★★★☆☆ (3+)          │
│                         │
│  STATO                  │
│  ☑ Solo Verificati     │
│  ☐ Nuovi (ultimi 30gg) │
│  ☐ Attivi oggi         │
│                         │
│  ┌─────────────────┐    │
│  │  APPLICA FILTRI │    │
│  └─────────────────┘    │
│  [ Cancella tutto ]     │
└─────────────────────────┘
```

### Mockup Grafico (Mobile - Collapsed)

```
┌────────────────────────────┐
│ [🎯 Filtri (3 attivi)] ▼  │
└────────────────────────────┘
```

### Struttura HTML Completa

```handlebars
{{!-- components/filter-panel.hbs --}}
<div class="filter-panel {{if this.isMobile 'filter-panel-mobile'}}">
  {{!-- Header --}}
  <div class="filter-panel-header d-flex justify-content-between align-items-center mb-4">
    <h5 class="ofinder-heading-md mb-0">Filtri</h5>

    {{#if this.hasActiveFilters}}
      <button
        class="btn btn-link btn-sm ofinder-text-primary p-0"
        {{on "click" this.clearAllFilters}}
      >
        Cancella tutto
      </button>
    {{/if}}
  </div>

  {{!-- Piattaforme --}}
  <div class="filter-section mb-4">
    <h6 class="ofinder-text-sm font-weight-bold mb-3">PIATTAFORME</h6>
    {{#each this.platformFilters as |platform|}}
      <div class="custom-control custom-checkbox mb-2">
        <input
          type="checkbox"
          class="custom-control-input"
          id="platform-{{platform.id}}"
          checked={{platform.selected}}
          {{on "change" (fn this.togglePlatform platform)}}
        />
        <label class="custom-control-label ofinder-text-sm" for="platform-{{platform.id}}">
          {{platform.emoji}} {{platform.name}}
          <span class="ofinder-text-muted">({{platform.count}})</span>
        </label>
      </div>
    {{/each}}
  </div>

  {{!-- Categorie --}}
  <div class="filter-section mb-4">
    <h6 class="ofinder-text-sm font-weight-bold mb-3">CATEGORIE</h6>
    {{#each this.categoryFilters as |category|}}
      <div class="custom-control custom-checkbox mb-2">
        <input
          type="checkbox"
          class="custom-control-input"
          id="category-{{category.id}}"
          checked={{category.selected}}
          {{on "change" (fn this.toggleCategory category)}}
        />
        <label class="custom-control-label ofinder-text-sm" for="category-{{category.id}}">
          {{category.name}}
          <span class="ofinder-text-muted">({{category.count}})</span>
        </label>
      </div>
    {{/each}}
  </div>

  {{!-- Range Slider Prezzo --}}
  <div class="filter-section mb-4">
    <h6 class="ofinder-text-sm font-weight-bold mb-3">
      PREZZO ($/mese)
      <span class="ofinder-text-primary float-right">${{this.maxPrice}}</span>
    </h6>
    <input
      type="range"
      class="custom-range"
      min="0"
      max="50"
      step="5"
      value={{this.maxPrice}}
      {{on "input" this.updatePriceFilter}}
    />
    <div class="d-flex justify-content-between ofinder-text-xs ofinder-text-muted mt-2">
      <span>Free</span>
      <span>$50+</span>
    </div>
  </div>

  {{!-- Rating --}}
  <div class="filter-section mb-4">
    <h6 class="ofinder-text-sm font-weight-bold mb-3">RATING MINIMO</h6>
    {{#each this.ratingFilters as |rating|}}
      <div class="custom-control custom-radio mb-2">
        <input
          type="radio"
          class="custom-control-input"
          id="rating-{{rating.value}}"
          name="ratingFilter"
          checked={{eq this.selectedRating rating.value}}
          {{on "change" (fn this.selectRating rating.value)}}
        />
        <label class="custom-control-label ofinder-text-sm" for="rating-{{rating.value}}">
          <RatingStars @rating={{rating.value}} @size="sm" />
          ({{rating.label}})
        </label>
      </div>
    {{/each}}
  </div>

  {{!-- Stato --}}
  <div class="filter-section mb-4">
    <h6 class="ofinder-text-sm font-weight-bold mb-3">STATO</h6>

    <div class="custom-control custom-checkbox mb-2">
      <input
        type="checkbox"
        class="custom-control-input"
        id="filter-verified"
        checked={{this.onlyVerified}}
        {{on "change" this.toggleVerified}}
      />
      <label class="custom-control-label ofinder-text-sm" for="filter-verified">
        <span class="ofinder-badge-verified ofinder-rounded-sm">✓</span>
        Solo Verificati
      </label>
    </div>

    <div class="custom-control custom-checkbox mb-2">
      <input
        type="checkbox"
        class="custom-control-input"
        id="filter-new"
        checked={{this.onlyNew}}
        {{on "change" this.toggleNew}}
      />
      <label class="custom-control-label ofinder-text-sm" for="filter-new">
        <span class="ofinder-badge-new ofinder-rounded-sm">New</span>
        Nuovi (ultimi 30gg)
      </label>
    </div>

    <div class="custom-control custom-checkbox mb-2">
      <input
        type="checkbox"
        class="custom-control-input"
        id="filter-active"
        checked={{this.onlyActiveToday}}
        {{on "change" this.toggleActiveToday}}
      />
      <label class="custom-control-label ofinder-text-sm" for="filter-active">
        <i class="pe-7s-signal ofinder-text-success"></i>
        Attivi oggi
      </label>
    </div>
  </div>

  {{!-- Bottoni Azione --}}
  <div class="filter-panel-footer">
    <button
      class="btn btn-block btn-ofinder-primary mb-2"
      {{on "click" this.applyFilters}}
    >
      Applica Filtri
      {{#if this.activeFiltersCount}}
        ({{this.activeFiltersCount}})
      {{/if}}
    </button>

    {{#if this.hasActiveFilters}}
      <button
        class="btn btn-block btn-outline-secondary"
        {{on "click" this.clearAllFilters}}
      >
        Cancella tutto
      </button>
    {{/if}}
  </div>
</div>
```

---

## Pagina Showcase Proposta

Per vedere tutti i componenti in azione, propongo di creare:

### Route: `/design-showcase`

**Layout della pagina:**

```
┌─────────────────────────────────────────────────────────┐
│  OFINDER - DESIGN SYSTEM SHOWCASE                       │
└─────────────────────────────────────────────────────────┘

  [1. PERFORMER CARDS]
  ═══════════════════════════════════════════════════

  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐
  │ Card 1  │  │ Card 2  │  │ Card 3  │  │ Card 4  │
  │         │  │         │  │         │  │         │
  └─────────┘  └─────────┘  └─────────┘  └─────────┘


  [2. SEARCH BAR]
  ═══════════════════════════════════════════════════

  🔍  [Search bar completa con filtri rapidi]


  [3. RATING STARS]
  ═══════════════════════════════════════════════════

  ★★★★★ 5.0    ★★★★☆ 4.3    ★★★☆☆ 3.2    ★☆☆☆☆ 1.5


  [4. CHANNEL BADGES]
  ═══════════════════════════════════════════════════

  [🔵 OnlyFans]  [🟣 Fansly]  [📸 Instagram]  [🎵 TikTok]


  [5. FILTER PANEL]
  ═══════════════════════════════════════════════════

  ┌─────────────────┐
  │ [Pannello filtri│
  │  completo]      │
  └─────────────────┘
```

---

## Struttura File da Creare

```
app/
├── components/
│   ├── performer-card.js
│   ├── performer-card.hbs
│   ├── search-bar.js
│   ├── search-bar.hbs
│   ├── rating-stars.js
│   ├── rating-stars.hbs
│   ├── channel-badge.js
│   ├── channel-badge.hbs
│   ├── filter-panel.js
│   └── filter-panel.hbs
│
├── routes/
│   └── design-showcase.js
│
└── templates/
    └── design-showcase.hbs
```

---

## Props API (Interfacce Componenti)

### PerformerCard
```javascript
<PerformerCard
  @performer={{performer}}      // Object: dati performer
  @variant="default"            // String: "default" | "compact" | "featured"
  @onViewProfile={{this.viewProfile}}  // Function: callback click
/>
```

### SearchBar
```javascript
<SearchBar
  @placeholder="Cerca..."       // String: placeholder input
  @onSearch={{this.handleSearch}}  // Function: callback ricerca
  @quickFilters={{this.filters}}   // Array: filtri rapidi
/>
```

### RatingStars
```javascript
<RatingStars
  @rating={{4.5}}               // Number: voto (0-5)
  @reviewCount={{127}}          // Number: numero recensioni
  @size="md"                    // String: "sm" | "md" | "lg"
  @interactive={{false}}        // Boolean: stelle cliccabili
  @onChange={{this.updateRating}}  // Function: callback (se interactive)
/>
```

### ChannelBadge
```javascript
<ChannelBadge
  @channel={{channel}}          // Object: dati canale
  @size="sm"                    // String: "sm" | "md"
  @clickable={{true}}           // Boolean: badge cliccabile
  @onClick={{this.openChannel}} // Function: callback click
/>
```

### FilterPanel
```javascript
<FilterPanel
  @filters={{this.currentFilters}}  // Object: filtri attivi
  @onApply={{this.applyFilters}}    // Function: callback applica
  @onClear={{this.clearFilters}}    // Function: callback cancella
  @isMobile={{this.isMobile}}       // Boolean: layout mobile
/>
```

---

## Design Tokens Utilizzati

Tutti i componenti usano il Design System creato in STEP 4:

- **Colori**: `ofinder-primary`, `ofinder-secondary`, `ofinder-accent`
- **Shadows**: `ofinder-shadow`, `ofinder-shadow-sm`, `ofinder-shadow-lg`
- **Border Radius**: `ofinder-rounded`, `ofinder-rounded-sm`, `ofinder-rounded-lg`
- **Typography**: `ofinder-heading-md`, `ofinder-text-sm`, `ofinder-text-muted`
- **Badges**: `ofinder-badge-verified`, `ofinder-badge-premium`, `ofinder-badge-new`
- **Buttons**: `btn-ofinder-primary`, `btn-ofinder-secondary`

---

## Responsive Breakpoints

- **Mobile (< 768px)**: Stack verticale, card full-width, filtri collapsati
- **Tablet (768px - 1024px)**: Grid 2 colonne, filtri sidebar
- **Desktop (> 1024px)**: Grid 3-4 colonne, filtri sidebar fixed

---

## Prossimi Step - Opzioni

**OPZIONE A**: Creo subito la pagina `/design-showcase` con tutti i componenti mockati, così puoi vedere il risultato live nel browser

**OPZIONE B**: Procedo con l'implementazione completa dei componenti partendo da PerformerCard

**OPZIONE C**: Faccio modifiche al design basandomi sui tuoi feedback

Quale opzione preferisci?
