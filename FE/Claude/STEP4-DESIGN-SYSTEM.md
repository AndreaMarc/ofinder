# STEP 4: Design System OFinder

## Panoramica

Il Design System OFinder definisce la palette colori, tipografia e utility classes utilizzate nell'applicazione. Si integra perfettamente con ArchitectUI, estendendolo con stili specifici per il brand OFinder.

## Struttura File

```
app/styles/custom/ofinder/
├── _variables.scss   # Design tokens (colori, spacing, shadows, ecc.)
├── _typography.scss  # Classi utility per tipografia
└── _utilities.scss   # Utility classes (backgrounds, badges, buttons)
```

Tutti i file vengono importati in `app/styles/custom/custom.scss`.

---

## Palette Colori

### Primary Colors (Rosa/Magenta)

Colori principali per CTA, link, elementi interattivi.

- **`$ofinder-primary`**: `#E91E63` (Material Pink 500)
- **`$ofinder-primary-dark`**: `#C2185B` (Hover/Active states)
- **`$ofinder-primary-light`**: `#F8BBD0` (Backgrounds leggeri)
- **`$ofinder-primary-gradient`**: Gradiente da primary a primary-dark

**Esempio uso:**
```html
<button class="btn-ofinder-primary">Cerca Performer</button>
<span class="ofinder-text-primary">Link importante</span>
<div class="ofinder-bg-primary p-4">Banner</div>
```

### Secondary Colors (Viola Profondo)

Per elementi secondari, header, footer.

- **`$ofinder-secondary`**: `#673AB7` (Material Deep Purple)
- **`$ofinder-secondary-dark`**: `#512DA8`
- **`$ofinder-secondary-light`**: `#D1C4E9`

**Esempio uso:**
```html
<button class="btn-ofinder-secondary">Filtri</button>
<div class="ofinder-bg-gradient-secondary">Header Section</div>
```

### Accent Colors (Oro - Premium)

Per feature premium, highlight, badge speciali.

- **`$ofinder-accent`**: `#FFC107` (Oro)
- **`$ofinder-accent-dark`**: `#FFA000`

**Esempio uso:**
```html
<span class="ofinder-badge-premium">Premium</span>
<div class="ofinder-bg-accent p-3">Sezione Premium</div>
```

### Neutral Grays

- **`$ofinder-gray-100`**: `#F5F5F5` (Sfondi leggeri)
- **`$ofinder-gray-200`**: `#EEEEEE`
- **`$ofinder-gray-300`**: `#E0E0E0` (Bordi)
- **`$ofinder-gray-700`**: `#616161` (Testo secondario)
- **`$ofinder-gray-900`**: `#212121` (Testo principale)

**Esempio uso:**
```html
<div class="ofinder-bg-light p-4">
  <p class="ofinder-text-muted">Testo secondario</p>
</div>
```

### Semantic Colors

- **`$ofinder-success`**: `#4CAF50` (Successo, verifica)
- **`$ofinder-warning`**: `#FF9800` (Attenzione)
- **`$ofinder-danger`**: `#F44336` (Errore, eliminazione)
- **`$ofinder-info`**: `#2196F3` (Informazioni)

**Esempio uso:**
```html
<span class="ofinder-badge-verified">Verified</span>
<span class="ofinder-badge-new">New</span>
```

---

## Tipografia

### Font Sizes

- **`$ofinder-font-size-xs`**: `0.75rem` (12px) - Label piccole
- **`$ofinder-font-size-sm`**: `0.875rem` (14px) - Testo secondario
- **`$ofinder-font-size-base`**: `1rem` (16px) - Testo principale
- **`$ofinder-font-size-lg`**: `1.125rem` (18px) - Sottotitoli
- **`$ofinder-font-size-xl`**: `1.5rem` (24px) - Titoli H2/H3
- **`$ofinder-font-size-2xl`**: `2rem` (32px) - Titoli H1

### Font Weights

- **`$ofinder-font-weight-normal`**: `400`
- **`$ofinder-font-weight-medium`**: `500`
- **`$ofinder-font-weight-bold`**: `700`

### Classi Utility

**Heading Classes:**
```html
<h1 class="ofinder-heading-xl">Trova il tuo Performer Ideale</h1>
<h2 class="ofinder-heading-lg">Categoria OnlyFans</h2>
<h3 class="ofinder-heading-md">Performer in Evidenza</h3>
```

**Text Classes:**
```html
<p class="ofinder-text-sm">Descrizione breve</p>
<span class="ofinder-text-xs">Label o tag</span>
```

**Color Classes:**
```html
<p class="ofinder-text-primary">Testo rosa</p>
<p class="ofinder-text-secondary">Testo viola</p>
<p class="ofinder-text-accent">Testo oro</p>
<p class="ofinder-text-muted">Testo grigio</p>
```

---

## Spacing System

Basato su multipli di `0.25rem` (4px):

- **`$ofinder-spacing-xs`**: `0.25rem` (4px)
- **`$ofinder-spacing-sm`**: `0.5rem` (8px)
- **`$ofinder-spacing-md`**: `1rem` (16px)
- **`$ofinder-spacing-lg`**: `1.5rem` (24px)
- **`$ofinder-spacing-xl`**: `2rem` (32px)
- **`$ofinder-spacing-2xl`**: `3rem` (48px)

**Nota:** Usa principalmente le classi Bootstrap (`p-3`, `m-4`, ecc.). Le variabili OFinder servono per SCSS custom.

---

## Borders & Border Radius

- **`$ofinder-border-radius-sm`**: `0.25rem` (4px) - Badge, piccoli elementi
- **`$ofinder-border-radius`**: `0.5rem` (8px) - Card, bottoni
- **`$ofinder-border-radius-lg`**: `1rem` (16px) - Immagini, modali

**Classi Utility:**
```html
<div class="ofinder-rounded-sm">Badge</div>
<div class="ofinder-rounded">Card normale</div>
<div class="ofinder-rounded-lg">Card hero</div>
```

---

## Shadows

- **`$ofinder-shadow-sm`**: `0 2px 4px rgba(0, 0, 0, 0.1)` - Card leggere
- **`$ofinder-shadow`**: `0 4px 6px rgba(0, 0, 0, 0.1)` - Card standard
- **`$ofinder-shadow-lg`**: `0 10px 20px rgba(0, 0, 0, 0.15)` - Modali, elementi in primo piano

**Classi Utility:**
```html
<div class="card ofinder-shadow">Card con shadow standard</div>
<div class="modal-content ofinder-shadow-lg">Modal</div>
```

---

## Utility Classes

### Backgrounds

```html
<!-- Colori solidi -->
<div class="ofinder-bg-primary">Background rosa</div>
<div class="ofinder-bg-secondary">Background viola</div>
<div class="ofinder-bg-accent">Background oro</div>
<div class="ofinder-bg-light">Background grigio chiaro</div>

<!-- Gradienti -->
<div class="ofinder-bg-gradient-primary">Gradiente rosa</div>
<div class="ofinder-bg-gradient-secondary">Gradiente viola</div>
```

### Badges

```html
<span class="ofinder-badge-verified">Verified</span>
<span class="ofinder-badge-premium">Premium</span>
<span class="ofinder-badge-new">New</span>
```

**Stili:**
- **Verified**: Verde, font medium, per performer verificati
- **Premium**: Oro, font bold, per feature a pagamento
- **New**: Blu, font medium, per nuovi contenuti

### Buttons

```html
<button class="btn btn-ofinder-primary">Primary Action</button>
<button class="btn btn-ofinder-secondary">Secondary Action</button>
```

---

## Esempi di Componenti

### Performer Card

```html
<div class="card ofinder-shadow ofinder-rounded">
  <img src="avatar.jpg" class="card-img-top ofinder-rounded-lg" alt="Performer">
  <div class="card-body">
    <h5 class="ofinder-heading-md">
      Nome Performer
      <span class="ofinder-badge-verified ml-2">Verified</span>
    </h5>
    <p class="ofinder-text-sm ofinder-text-muted">OnlyFans Creator</p>
    <div class="d-flex justify-content-between align-items-center mt-3">
      <span class="ofinder-text-primary">4.8 ⭐</span>
      <button class="btn btn-sm btn-ofinder-primary">Visualizza</button>
    </div>
  </div>
</div>
```

### Hero Section

```html
<div class="ofinder-bg-gradient-primary p-5 text-center">
  <h1 class="ofinder-heading-xl text-white mb-3">
    Trova il tuo Performer Ideale
  </h1>
  <p class="ofinder-text-lg text-white mb-4">
    Cerca tra migliaia di creator verificati
  </p>
  <button class="btn btn-lg btn-ofinder-secondary">Inizia Ora</button>
</div>
```

### Premium Banner

```html
<div class="alert ofinder-bg-accent ofinder-rounded p-4">
  <span class="ofinder-badge-premium mb-2">Premium</span>
  <h4 class="ofinder-heading-md">Sblocca tutte le funzionalità</h4>
  <p class="ofinder-text-sm">Accesso illimitato a filtri avanzati e notifiche in tempo reale</p>
  <button class="btn btn-ofinder-primary mt-2">Upgrade Ora</button>
</div>
```

---

## Best Practices

### 1. Preferisci Classi ArchitectUI

Usa sempre le classi Bootstrap/ArchitectUI quando disponibili:

```html
<!-- ✅ CORRETTO -->
<div class="p-4 mb-3 text-center">...</div>

<!-- ❌ EVITARE -->
<div style="padding: 1rem; margin-bottom: 1rem; text-align: center;">...</div>
```

### 2. Usa Variabili SCSS per Custom Components

Quando crei componenti custom, usa le variabili OFinder:

```scss
// ✅ CORRETTO
.my-custom-card {
  background-color: $ofinder-primary;
  border-radius: $ofinder-border-radius;
  box-shadow: $ofinder-shadow;
}

// ❌ EVITARE
.my-custom-card {
  background-color: #E91E63;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
```

### 3. Combina Utility Classes

Combina utility OFinder con Bootstrap per flessibilità:

```html
<div class="card ofinder-shadow ofinder-rounded p-4 mb-3">
  <h5 class="ofinder-heading-md mb-2">Titolo</h5>
  <p class="ofinder-text-sm ofinder-text-muted">Descrizione</p>
</div>
```

### 4. Mantieni Consistenza nei Colori

- **Primary (Rosa)**: CTA, link principali, azioni primarie
- **Secondary (Viola)**: Header, footer, azioni secondarie
- **Accent (Oro)**: Premium, highlight, offerte speciali
- **Semantic**: Usa solo per stati (success, warning, danger, info)

---

## Checklist Implementazione

- [x] Creato `_variables.scss` con design tokens
- [x] Creato `_typography.scss` con utility tipografiche
- [x] Creato `_utilities.scss` con classi utility
- [x] Aggiornato `custom.scss` con import OFinder
- [x] Documentate tutte le classi e variabili

### Testing

Per testare il Design System:

1. Compilare SCSS e verificare assenza errori
2. Creare una pagina demo con esempi di tutti i componenti
3. Verificare responsiveness su mobile (Cordova compatibility)
4. Validare contrasto colori (WCAG AA compliance)

---

## Prossimi Step

**STEP 5**: Creare componenti base riutilizzabili:
- `<PerformerCard>` - Card performer con avatar, badge, rating
- `<SearchBar>` - Barra ricerca con filtri
- `<RatingStars>` - Visualizzazione rating stelle
- `<ChannelBadge>` - Badge per piattaforme (OnlyFans, Fansly, ecc.)

---

**Data Implementazione**: 2025-12-09
**Framework**: MAE (Ember 4.11 + ArchitectUI)
**Design System Version**: 1.0
