# OFinder - Project Guide

## Overview

**OFinder** è un motore di ricerca specializzato per **performer adulti** (OnlyFans, Fansly, CamGirl).

L'applicazione permette agli utenti di cercare e filtrare performer registrati utilizzando diversi parametri (nazione, regione, provincia, età, ecc.). La ricerca è disponibile sia per utenti registrati che per **utenti anonimi** (dopo accettazione disclaimer).

Questo documento contiene le direttive, funzionalità e caratteristiche **specifiche del progetto OFinder**, separate dalla documentazione generale del framework (README.md nella root).

## Framework Reference

Per informazioni sul framework MAE utilizzato, consultare:
- `/README.md` - Documentazione framework generale
- `/FE FWK DOCS v1.1 20240530.pdf` - Guida completa framework

---

## Project Information

**Nome Progetto:** OFinder
**Framework:** MAE (Ember.js + ArchitectUI)
**Target:** Mobile-first (Android/iOS via Cordova) + Web
**Tipo Contenuto:** Adult Content (18+)
**Created:** 2025-12-09

---

## Project Scope

### Obiettivo Principale
Creare un **motore di ricerca** per:
- Performer OnlyFans
- Performer Fansly
- CamGirl

### Funzionalità Core

#### Ricerca e Filtri

Gli utenti possono cercare performer utilizzando i seguenti filtri:

**Filtri Geografici:**
- **Nazione** (stato_id)
- **Regione** (regione_id)
- **Provincia** (provincia_id)
- **Città di residenza** (residenceCity - text search)

**Filtri Demografici:**
- **Sesso** (M/F)
- **Età** (range min/max, calcolato da birth_date)

**Filtri Piattaforme:**
- **Piattaforma** (OnlyFans, Fansly, Telegram, Instagram, Facebook, X)
- **Username/Handle** (text search)

**Filtri Servizi:**
- **Tipo servizio** (model, escort, ecc.)
- **Tipo contenuto** (foto erotiche, video gallery, live public/private, ecc.)

**Filtri Prezzi:**
- **Prezzo minimo/massimo** (range generico su tutti i pricing)
- **Abbonamento mensile** (range)
- **Live private** (range)
- **Live public** (range)

**Filtri Qualità/Popolarità:**
- **Rating minimo** (stelle, da performer_review)
- **Più votati** (ordinamento per avg_rating)
- **Più visti** (ordinamento per total_views)
- **Più recenti** (ordinamento per createdAt)

**Ordinamenti disponibili:**
- Nome (A-Z, Z-A)
- Data creazione (recenti, meno recenti)
- Rating medio (migliori, peggiori)
- Numero visualizzazioni (più visti, meno visti)
- Numero recensioni (più recensiti)

#### Accesso
- ✅ **Utenti anonimi**: possono effettuare ricerche senza registrazione
- ✅ **Utenti registrati**: accesso completo alle funzionalità
- ⚠️ **Age Gate obbligatorio**: Disclaimer e verifica età per contenuti adulti

---

## Architecture & Development

### Utenti Anonimi (Non-Logged Users)

**Challenge:** Il framework MAE è ottimizzato per utenti loggati, ma OFinder richiede funzionalità per anonimi.

**Soluzione:**
- Utilizzare le pagine non-logged di ArchitectUI theme:
  - `pages-login.html`
  - `pages-login-boxed.html`
  - Altre pagine pubbliche disponibili nel tema
- Consultare la guida framework per gestione utenti anonimi
- Implementare routing pubblico vs privato

**Flusso Utente Anonimo:**
1. Landing page con Age Gate/Disclaimer
2. Accettazione termini contenuti adulti
3. Accesso alla ricerca performer (read-only)
4. Optional: Registrazione per funzionalità avanzate

### Database

**Schema Database Completo:**

#### Tabella: `performer`
Dati anagrafici e generali del performer (univoci). **Estensione della tabella user-profile**.

```
id (guid, PK)
user_id (guid, FK → user)
stato_id (guid, FK → stato)
regione_id (guid, FK → regione)
provincia_id (guid, FK → provincia)
name (da user-profile)
surname (da user-profile)
email (da user)
birth_date (da user-profile)
sex (da user-profile)
description (da user-profile)
residenceCity (da user-profile)
createdAt (datetime)
updatedAt (datetime)
```

**Relazioni:**
- `performer` 1:N `channel`
- `performer` 1:N `performer_review`
- `performer` 1:N `performer_service`
- `performer` 1:N `performer_view`
- `performer` 1:N `user_favorite`

---

#### Tabella: `channel`
Ogni canale/social/piattaforma gestito dal performer.

```
id (guid, PK)
performer_id (guid, FK → performer.id)
platform (varchar 30, ENUM: OnlyFans, Fansly, Telegram, Instagram, Facebook, X, ecc.)
username_handle (nvarchar 50)
profile_link (nvarchar 150)
note (nvarchar max)
createdAt (datetime)
updatedAt (datetime)
```

**Relazioni:**
- `channel` 1:N `channel_schedule`
- `channel` 1:N `channel_content_types`
- `channel` 1:1 `channel_pricing`

**Piattaforme supportate:**
- OnlyFans
- Fansly
- Telegram
- Instagram
- Facebook
- X (Twitter)
- *(Altri da definire)*

---

#### Tabella: `channel_schedule`
Orari live o di attività, specifici per ciascun canale.

```
id (guid, PK)
channel_id (guid, FK → channel.id)
day_of_week (int, 0=domenica, 1=lunedì, ..., 6=sabato)
start_time (time)
end_time (time)
note (nvarchar max)
createdAt (datetime)
updatedAt (datetime)
```

---

#### Tabella: `channel_content_types`
Tipi di contenuti pubblicati su quel canale.

```
id (guid, PK)
channel_id (guid, FK → channel.id)
content_type (nvarchar 40)
description (nvarchar max)
createdAt (datetime)
updatedAt (datetime)
```

**Esempi content_type:**
- "foto erotiche"
- "video gallery"
- "live public"
- "live private"
- "vendita abbigliamento"
- "contenuti extra"

---

#### Tabella: `channel_pricing`
Tariffe e abbonamenti specifici per ciascun canale.

```
id (guid, PK)
channel_id (guid, FK → channel.id)
monthly_subscription_from (decimal)
monthly_subscription_to (decimal)
photo_sale_from (decimal)
photo_sale_to (decimal)
video_sale_from (decimal)
video_sale_to (decimal)
live_public_from (decimal)
live_public_to (decimal)
live_private_from (decimal)
live_private_to (decimal)
clothing_sales_from (decimal)
clothing_sales_to (decimal)
extra_content_from (decimal)
extra_content_to (decimal)
note (text)
createdAt (datetime)
updatedAt (datetime)
```

**Nota:** Range di prezzi (from/to) per ogni tipologia di servizio.

---

#### Tabella: `performer_review`
Raccoglie i voti e le recensioni dei performer espressi dagli utenti loggati.

```sql
CREATE TABLE performer_reviews (
  id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
  performer_id CHAR(36) NOT NULL,
  user_id CHAR(36) NOT NULL,
  rating INT NOT NULL,
  review_text TEXT NULL,
  createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT FK_PerformerReview_Performer FOREIGN KEY (performer_id) REFERENCES performer(id),
  CONSTRAINT FK_PerformerReview_User FOREIGN KEY (user_id) REFERENCES user(id),
  CONSTRAINT UQ_PerformerReview UNIQUE (performer_id, user_id)
);
```

**NOTA:** Vincolo UNIQUE su coppia `(performer_id, user_id)` - un utente può recensire un performer una sola volta.

**Query Media Recensioni:**
```sql
SELECT
  p.id, p.name, p.surname, p.createdAt,
  AVG(r.rating) AS avg_rating,
  COUNT(r.id) AS total_reviews
FROM performer p
LEFT JOIN performer_reviews r ON p.id = r.performer_id
GROUP BY p.id, p.name, p.surname, p.createdAt
ORDER BY avg_rating DESC, p.createdAt DESC;
```

---

#### Tabella: `performer_service`
Indica servizi opzionali offerti dai performer.

```
id (guid, PK)
performer_id (guid, FK → performer.id)
service_type (nvarchar 40, es. "model", "escort", ecc.)
link (nvarchar 400)
description (nvarchar max)
createdAt (datetime)
updatedAt (datetime)
```

**Esempi service_type:**
- "model" (modella/o)
- "escort"
- *(Altri da definire)*

---

#### Tabella: `performer_view`
Registra ogni click sul profilo del performer (tracking visualizzazioni).

```
id (guid, PK)
performer_id (guid, FK → performer.id)
user_id (guid, FK → user.id, NULLABLE per utenti anonimi)
viewedAt (datetime)
```

**NOTA:** Vincolo UNIQUE su coppia `(user_id, performer_id)` per evitare duplicati per utente.

**Utilizzo:**
- Statistiche visualizzazioni profilo
- "Performer più visti"
- Analytics

---

#### Tabella: `user_favorite`
Consente agli utenti loggati di crearsi una lista di performer preferiti.

```
id (guid, PK)
user_id (guid, FK → user.id)
performer_id (guid, FK → performer.id)
createdAt (datetime)
```

**NOTA:** Vincolo UNIQUE su coppia `(user_id, performer_id)`.

**Funzionalità:**
- Lista "I miei preferiti"
- Quick access a performer salvati
- Feature per utenti registrati

---

### Tabelle Geografiche (Riferimenti)

**Assunzione:** Esistono tabelle di lookup per dati geografici:
- `stato` (nazioni)
- `regione` (regioni per nazione)
- `provincia` (province per regione)

**Campi tipici:**
```
id (guid, PK)
name (nvarchar)
code (nvarchar, es. ISO code per stati)
```

---

### Schema Relazionale Completo

```
user (framework)
  └─→ user_profile (framework)
        └─→ performer (1:1)
              ├─→ channel (1:N)
              │     ├─→ channel_schedule (1:N)
              │     ├─→ channel_content_types (1:N)
              │     └─→ channel_pricing (1:1)
              ├─→ performer_review (1:N)
              ├─→ performer_service (1:N)
              ├─→ performer_view (1:N)
              └─→ user_favorite (1:N)

stato (lookup)
regione (lookup)
provincia (lookup)
```

---

## Legal & Compliance

### ⚠️ IMPORTANTE: Normativa Italiana Obbligatoria

**Dal 12 Novembre 2025**, in Italia è obbligatorio implementare sistemi di **age verification** per siti e piattaforme che distribuiscono contenuti pornografici, secondo:
- **Decreto Caivano** (DL 123/2023, convertito in Legge 159/2023)
- **Delibera AGCOM n. 96/25/CONS** (8 Aprile 2025)

**Sanzioni per non conformità:** Fino a **€250.000** di multa. Nei casi gravi: blocco dell'accesso al sito in Italia.

### Sistema di Age Verification Obbligatorio

#### Requisiti Tecnici AGCOM

**Sistema "Double Anonymity" (Doppia Anonimità):**
1. **Identificazione** - Verifica età tramite terze parti certificate indipendenti
2. **Autenticazione** - Verifica per ogni sessione di utilizzo del servizio

**Protezione Privacy:**
- Il provider di age verification **NON vede** quale sito l'utente sta visitando
- Il sito adulto riceve solo un **"proof of legal age"** senza dati personali identificativi
- **Privacy by design** e **privacy by default** (conformità GDPR)
- Principio di **data minimization** (solo verifica età, nessuna identità)

#### Opzioni di Implementazione

**Metodo 1: Terze Parti Certificate**
- Utilizzare provider certificati di age verification (raccomandato)
- Garantisce conformità legale e protezione dati
- Sistema già testato e approvato da AGCOM

**Metodo 2: EU Digital Wallet** (futuro)
- L'UE ha rilasciato un blueprint per age verification (Luglio 2025)
- Italia tra i primi paesi ad adottarlo
- App nazionali di verifica età in arrivo

**Metodo 3: App Personalizzate**
- Devono rispettare GDPR (data minimization)
- Devono essere sicure da cyber-attack
- Devono essere accurate nel fornire proof of age

### GDPR Compliance per Adult Content

#### Privacy Policy Obbligatoria

**Requisiti:**
- Linguaggio chiaro e semplice (non tecnico)
- Facile da trovare (link visibile in tutte le pagine)
- Spiega come vengono raccolti, processati, conservati e condivisi i dati
- Include diritti utenti: accesso dati, correzione, cancellazione (right to be forgotten)

**Contenuti minimi Privacy Policy:**
- Tipologie di dati raccolti
- Base legale del trattamento
- Finalità del trattamento
- Periodo di conservazione dati
- Condivisione dati con terze parti
- Misure di sicurezza implementate
- Diritti GDPR degli utenti
- Contatti Data Protection Officer (se applicabile)

#### Cookie Policy e Consent

**Requisiti:**
- **Cookie banner** conforme GDPR all'ingresso
- Consenso esplicito prima di cookie non essenziali
- Possibilità di rifiutare cookie non necessari
- Cookie preference center (gestione consensi)
- Cookie policy dettagliata

#### Misure di Sicurezza

**Obbligatorie per GDPR:**
- **Crittografia** dati sensibili (in transito e at rest)
- **Secure data storage** con accesso limitato
- **Backup** regolari e sicuri
- Protezione contro accessi non autorizzati
- Procedure per data breach notification (72 ore)

#### Diritti Utenti GDPR

**Gli utenti devono poter:**
- Accedere ai propri dati personali
- Richiedere correzioni dei dati
- Richiedere cancellazione dati (right to be forgotten)
- Esportare i dati (data portability)
- Opporsi al trattamento
- Limitare il trattamento

### Terms of Service (ToS)

**Requisiti specifici per adult content:**
- Conferma che il contenuto è legale
- Divieto assoluto contenuti che coinvolgono minori
- Regole su contenuti generati da utenti (se applicabile)
- Disclaimer responsabilità
- Legge applicabile e foro competente
- Procedura reclami e dispute

### Compliance Checklist OFinder

**Prima del lancio:**
- [ ] **Age verification system** implementato (terza parte certificata)
- [ ] **Privacy Policy** completa e conforme GDPR
- [ ] **Cookie Policy** e banner consenso
- [ ] **Terms of Service** per adult content
- [ ] **Disclaimer** contenuti adulti (pagina ingresso)
- [ ] **Exit button** per minori (richiesto da best practices)
- [ ] **Data encryption** (SSL/TLS per tutto il sito)
- [ ] **Secure storage** dati utenti
- [ ] **Data breach procedures** documentate
- [ ] **User rights interface** (accesso/modifica/cancellazione dati)
- [ ] Test age verification flow completo
- [ ] Legal review da avvocato specializzato (raccomandato)

### Sanzioni e Rischi

**GDPR (UE):**
- Fino a **€20 milioni** o **4% fatturato globale annuo** (la cifra maggiore)

**Age Verification Italia:**
- Fino a **€250.000** per non conformità
- Blocco accesso sito in Italia (casi gravi)

**Altri rischi:**
- Danni reputazionali
- Class action utenti
- Responsabilità civile e penale

---

## UI/UX Guidelines

### Design Principles

**OFinder deve essere:**
- 🎨 **Visivamente attraente** (professional adult platform aesthetic)
- 📱 **Mobile-first** (thumb-friendly navigation)
- 🔍 **Search-focused** (ricerca come elemento centrale)
- 🎯 **Intuitive filtering** (filtri facili da usare)
- ⚡ **Fast & responsive** (caricamento rapido cards/risultati)

### Best Practices Adult Content Platforms

1. **Professional Aesthetic**
   - Design elegante e moderno (non volgare)
   - Colori sofisticati (evitare rosa shocking, rosso acceso)
   - Typography professionale
   - Spaziatura generosa

2. **Search & Discovery**
   - Barra di ricerca prominente
   - Filtri visibili e intuitivi
   - Grid/card layout per risultati
   - Lazy loading per performance
   - Preview thumbnails (appropriate, non esplicite)

3. **Trust & Safety**
   - Badge verificati per performer autentici
   - Rating/review system (se applicabile)
   - Report button per contenuti inappropriati
   - Clear privacy indicators

4. **Mobile Optimization**
   - Cards ottimizzate per touch
   - Filtri collapsible su mobile
   - Swipe gestures (se appropriato)
   - Bottom navigation per funzioni principali

### ArchitectUI Theme - Pagine Non-Logged

**Pagine disponibili nel tema** (`/ARCHITECT/architectui-html-pro/`):

1. **pages-login.html** - Login standard con slider laterale
   - Layout split-screen: slider immagini (desktop) + form login
   - Responsive: mobile mostra solo form
   - Include: email, password, "keep me logged in", recover password

2. **pages-login-boxed.html** - Login boxed centrato
   - Background fullscreen con animazione gradient
   - Modal dialog centrato per form login
   - Più minimal e moderno
   - Mobile-friendly

3. **pages-register.html** - Registrazione standard
4. **pages-register-boxed.html** - Registrazione boxed
5. **pages-forgot-password.html** - Recupero password standard
6. **pages-forgot-password-boxed.html** - Recupero password boxed

**Caratteristiche comuni:**
- Bootstrap 4 based
- Responsive design mobile-first
- Form validation ready
- Gradient backgrounds con animazioni
- Clean, modern aesthetic
- Link tra le pagine (login ↔ register ↔ forgot password)

**Uso per OFinder:**

**Variante "Boxed" consigliata** per OFinder perché:
- Più moderna e visivamente impattante
- Background gradient personalizzabile
- Centrata e mobile-friendly
- Perfetta per Age Gate modal

**Pagine da creare per OFinder:**
- `age-gate.hbs` - Age verification (basata su boxed style)
- `landing.hbs` - Landing page pubblica con search
- `search-results.hbs` - Risultati ricerca (pubblica)
- `performer-detail.hbs` - Dettaglio performer (pubblica)
- `login.hbs` - Login (usa boxed style)
- `register.hbs` - Registrazione (usa boxed style)

---

## Features

### MVP (Minimum Viable Product)

**Phase 1 - Core Search:**
- [ ] Age Gate con disclaimer
- [ ] Landing page con ricerca
- [ ] Filtri base (nazione, regione, provincia, età)
- [ ] Grid risultati performer
- [ ] Dettaglio performer (public view)

**Phase 2 - User Registration:**
- [ ] Sistema registrazione
- [ ] Login/Logout
- [ ] User profile
- [ ] Funzionalità premium per utenti registrati

**Phase 3 - Advanced Features:**
- [ ] Favoriti/Saved performers
- [ ] Notifiche (nuovi performer, aggiornamenti)
- [ ] Rating/Review system
- [ ] Advanced filters

---

## Custom Components

### Componenti da Sviluppare

#### 1. `age-gate`
Age verification modal/page (prima schermata obbligatoria)

**Props:**
- `onAccept` (action) - Callback accettazione
- `onReject` (action) - Callback rifiuto (redirect a sito sicuro)

**Features:**
- Disclaimer legale contenuti adulti
- Checkbox "Ho 18+ anni"
- Button "Accetta" e "Esci"
- Link Privacy Policy e Terms of Service
- Cookie per ricordare consenso (session-based)
- Non bypassabile

---

#### 2. `performer-card`
Card performer per risultati ricerca (grid view)

**Props:**
- `@performer` (model) - Modello performer

**Features:**
- Foto profilo (thumbnail)
- Nome completo
- Età e localizzazione (città, provincia)
- Rating stelle (avg_rating + total_reviews)
- Badge piattaforme principali (icone OnlyFans/Fansly/ecc.)
- Badge "Verified" (se applicabile)
- Click → redirect a dettaglio performer
- Mobile-optimized (touch-friendly)

---

#### 3. `performer-grid`
Grid layout per lista risultati ricerca

**Props:**
- `@performers` (array) - Lista performer
- `@isLoading` (boolean) - Loading state

**Features:**
- Responsive grid (1 col mobile, 2 tablet, 3+ desktop)
- Lazy loading / infinite scroll
- Empty state quando nessun risultato
- Loading skeleton cards
- Pagination controls (optional)

---

#### 4. `search-filters`
Pannello filtri ricerca (sidebar/drawer)

**Props:**
- `@filters` (TrackedObject) - Filtri attivi
- `@onFilterChange` (action) - Callback cambio filtri

**Features:**
- Filtro geografico (stato → regione → provincia, cascading)
- Filtro sesso (M/F radio buttons)
- Filtro età (range slider min/max)
- Filtro piattaforme (checkbox multiple)
- Filtro servizi (checkbox multiple)
- Filtro prezzi (range slider)
- Filtro rating minimo (stelle)
- Button "Applica filtri" e "Reset"
- Collapsible su mobile (drawer)
- Count risultati in real-time

---

#### 5. `performer-profile`
Profilo performer dettagliato (pagina completa)

**Props:**
- `@performer` (model) - Modello performer con relationships loaded

**Features:**
- Header con foto, nome, età, localizzazione
- Rating e recensioni totali
- Descrizione personale
- Lista canali con link esterni
- Schedule orari live (tabella giorni/orari)
- Tipi contenuti offerti
- Tariffe (tabella prezzi)
- Servizi extra
- Lista recensioni (con rating, testo, utente, data)
- Button "Aggiungi ai preferiti" (solo logged users)
- Button "Lascia recensione" (solo logged users)
- View tracking automatico (incrementa performer_view)

---

#### 6. `channel-list`
Lista canali social/piattaforme del performer

**Props:**
- `@channels` (array) - Lista canali

**Features:**
- Card per ogni canale
- Icona piattaforma
- Username/handle
- Link esterno "Vai al profilo"
- Badge "verified" (se disponibile)
- Orari live (se presenti)
- Contenuti offerti
- Prezzi (se presenti)

---

#### 7. `review-list`
Lista recensioni con rating

**Props:**
- `@reviews` (array) - Lista recensioni
- `@canAddReview` (boolean) - Se utente può aggiungere recensione

**Features:**
- Card recensione (stelle, testo, utente, data)
- Ordinamento (recenti, rating alto/basso)
- Paginazione
- Form "Aggiungi recensione" (solo logged users)
- Empty state se nessuna recensione

---

#### 8. `review-form`
Form per creare/modificare recensione

**Props:**
- `@performer` (model) - Performer da recensire
- `@onSubmit` (action) - Callback submit
- `@review` (model, optional) - Recensione esistente (edit mode)

**Features:**
- Rating selector (1-5 stelle)
- Textarea per testo recensione
- Validazione (rating obbligatorio)
- Button "Invia recensione"
- Solo per utenti loggati
- Check se utente ha già recensito (UNIQUE constraint)

---

#### 9. `favorite-button`
Toggle button per aggiungere/rimuovere da preferiti

**Props:**
- `@performer` (model) - Performer
- `@isFavorite` (boolean) - Stato corrente

**Features:**
- Icon heart (filled/outlined)
- Toggle on click
- Loading state
- Solo per utenti loggati
- Tooltip "Aggiungi ai preferiti" / "Rimuovi dai preferiti"

---

#### 10. `search-bar`
Barra di ricerca principale

**Props:**
- `@onSearch` (action) - Callback ricerca
- `@placeholder` (string) - Placeholder text

**Features:**
- Input text con icona search
- Clear button (X)
- Enter to search
- Mobile-optimized
- Autofocus su desktop

---

#### 11. `filter-chip`
Chip per filtro attivo (removable)

**Props:**
- `@label` (string) - Label filtro
- `@onRemove` (action) - Callback rimozione

**Features:**
- Pill style con X button
- Click su X rimuove filtro
- Visual feedback hover
- Compact design

---

#### 12. `pricing-table`
Tabella prezzi servizi

**Props:**
- `@pricing` (model) - Channel pricing model

**Features:**
- Tabella con servizi e range prezzi
- Formato €XX - €YY
- Hide rows se prezzo non disponibile
- Mobile-responsive (stack verticale)

---

#### 13. `schedule-calendar`
Calendario/tabella orari live

**Props:**
- `@schedules` (array) - Lista channel_schedule

**Features:**
- Tabella giorni settimana
- Orari start-end per giorno
- Color coding per tipo live (public/private)
- Mobile-responsive
- Empty state se nessun orario

---

#### 14. `platform-badge`
Badge icona piattaforma

**Props:**
- `@platform` (string) - Nome piattaforma
- `@size` (string) - small/medium/large

**Features:**
- Icona SVG piattaforma
- Colori brand ufficiali
- Tooltip con nome piattaforma
- Responsive sizing

---

#### 15. `rating-stars`
Componente stelle rating (display o input)

**Props:**
- `@rating` (number) - Rating 1-5
- `@totalReviews` (number, optional) - Numero recensioni
- `@interactive` (boolean) - Se cliccabile per votare
- `@onChange` (action, optional) - Callback cambio rating

**Features:**
- Stelle piene/mezze/vuote
- Display decimale (es. 4.5 stelle)
- Click per votare (se interactive)
- Mostra "(X recensioni)" se totalReviews
- Accessible (ARIA labels)

---

## API Integration

### Backend Structure (JSON:API)

**Endpoints Performer:**

```
GET    /performers                    - Lista performer con filtri
GET    /performers/:id                - Dettaglio performer singolo
GET    /performers/:id/channels       - Canali del performer
GET    /performers/:id/reviews        - Recensioni del performer
GET    /performers/:id/services       - Servizi del performer
POST   /performers/:id/view           - Registra visualizzazione profilo
GET    /performers/top-rated          - Performer più votati (avg_rating)
GET    /performers/most-viewed        - Performer più visti
```

**Query Parameters per GET /performers:**
```
?filter[stato_id]=xxx
?filter[regione_id]=xxx
?filter[provincia_id]=xxx
?filter[sex]=M|F
?filter[age_min]=18
?filter[age_max]=99
?filter[platform]=OnlyFans|Fansly|...
?filter[service_type]=model|escort|...
?filter[content_type]=live-public|...
?filter[price_min]=10.00
?filter[price_max]=100.00
?sort=name|-name|createdAt|-createdAt|avg_rating|-avg_rating
?page[number]=1
?page[size]=20
?include=channels,reviews,stato,regione,provincia
```

**Endpoints Channel:**

```
GET    /channels                      - Lista canali (raramente usato diretto)
GET    /channels/:id                  - Dettaglio canale
GET    /channels/:id/schedule         - Schedule del canale
GET    /channels/:id/content-types    - Content types del canale
GET    /channels/:id/pricing          - Pricing del canale
```

**Endpoints Review (Utenti Loggati):**

```
GET    /performer-reviews             - Lista recensioni (admin)
POST   /performer-reviews             - Crea recensione
GET    /performer-reviews/:id         - Dettaglio recensione
PATCH  /performer-reviews/:id         - Aggiorna recensione
DELETE /performer-reviews/:id         - Elimina recensione
```

**Endpoints Favorite (Utenti Loggati):**

```
GET    /user-favorites                - I miei preferiti
POST   /user-favorites                - Aggiungi a preferiti
DELETE /user-favorites/:id            - Rimuovi da preferiti
```

**Endpoints Geografici (Lookup):**

```
GET    /stati                         - Lista nazioni
GET    /stati/:id/regioni             - Regioni per nazione
GET    /regioni/:id/province          - Province per regione
```

---

### EmberData Models

#### Model: `performer`
```javascript
// app/models/performer.js
import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class PerformerModel extends Model {
  // Relationships
  @belongsTo('user', { async: true, inverse: null }) user;
  @belongsTo('stato', { async: true }) stato;
  @belongsTo('regione', { async: true }) regione;
  @belongsTo('provincia', { async: true }) provincia;
  @hasMany('channel', { async: true }) channels;
  @hasMany('performer-review', { async: true }) reviews;
  @hasMany('performer-service', { async: true }) services;
  @hasMany('performer-view', { async: true }) views;

  // Attributes (from user-profile)
  @attr('string') name;
  @attr('string') surname;
  @attr('string') email;
  @attr('date-utc') birthDate;
  @attr('string') sex; // 'M' | 'F'
  @attr('string') description;
  @attr('string') residenceCity;

  // Timestamps
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // Computed (from backend aggregation)
  @attr('number') avgRating; // Calcolato lato backend
  @attr('number') totalReviews; // Calcolato lato backend
  @attr('number') totalViews; // Calcolato lato backend

  // Computed properties
  get fullName() {
    return `${this.name} ${this.surname}`;
  }

  get age() {
    if (!this.birthDate) return null;
    const today = new Date();
    const birth = new Date(this.birthDate);
    let age = today.getFullYear() - birth.getFullYear();
    const monthDiff = today.getMonth() - birth.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
      age--;
    }
    return age;
  }
}
```

---

#### Model: `channel`
```javascript
// app/models/channel.js
import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class ChannelModel extends Model {
  // Relationships
  @belongsTo('performer', { async: true }) performer;
  @hasMany('channel-schedule', { async: true }) schedules;
  @hasMany('channel-content-type', { async: true }) contentTypes;
  @belongsTo('channel-pricing', { async: true }) pricing;

  // Attributes
  @attr('string') platform; // ENUM: OnlyFans, Fansly, Telegram, Instagram, Facebook, X
  @attr('string') usernameHandle;
  @attr('string') profileLink;
  @attr('string') note;

  // Timestamps
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // Platform icons mapping
  get platformIcon() {
    const icons = {
      'OnlyFans': 'icon-onlyfans',
      'Fansly': 'icon-fansly',
      'Telegram': 'icon-telegram',
      'Instagram': 'icon-instagram',
      'Facebook': 'icon-facebook',
      'X': 'icon-x'
    };
    return icons[this.platform] || 'icon-link';
  }
}
```

---

#### Model: `channel-schedule`
```javascript
// app/models/channel-schedule.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class ChannelScheduleModel extends Model {
  @belongsTo('channel', { async: true }) channel;

  @attr('number') dayOfWeek; // 0=domenica, 1=lunedì, ..., 6=sabato
  @attr('string') startTime; // HH:mm format
  @attr('string') endTime;   // HH:mm format
  @attr('string') note;
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  get dayName() {
    const days = ['Domenica', 'Lunedì', 'Martedì', 'Mercoledì', 'Giovedì', 'Venerdì', 'Sabato'];
    return days[this.dayOfWeek];
  }
}
```

---

#### Model: `channel-content-type`
```javascript
// app/models/channel-content-type.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class ChannelContentTypeModel extends Model {
  @belongsTo('channel', { async: true }) channel;

  @attr('string') contentType; // "foto erotiche", "video gallery", "live public", etc.
  @attr('string') description;
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;
}
```

---

#### Model: `channel-pricing`
```javascript
// app/models/channel-pricing.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class ChannelPricingModel extends Model {
  @belongsTo('channel', { async: true }) channel;

  // Price ranges
  @attr('number') monthlySubscriptionFrom;
  @attr('number') monthlySubscriptionTo;
  @attr('number') photoSaleFrom;
  @attr('number') photoSaleTo;
  @attr('number') videoSaleFrom;
  @attr('number') videoSaleTo;
  @attr('number') livePublicFrom;
  @attr('number') livePublicTo;
  @attr('number') livePrivateFrom;
  @attr('number') livePrivateTo;
  @attr('number') clothingSalesFrom;
  @attr('number') clothingSalesTo;
  @attr('number') extraContentFrom;
  @attr('number') extraContentTo;
  @attr('string') note;

  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // Helper per mostrare range prezzi
  priceRange(from, to) {
    if (!from && !to) return null;
    if (from === to) return `€${from}`;
    if (!to) return `da €${from}`;
    if (!from) return `fino a €${to}`;
    return `€${from} - €${to}`;
  }
}
```

---

#### Model: `performer-review`
```javascript
// app/models/performer-review.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class PerformerReviewModel extends Model {
  @belongsTo('performer', { async: true }) performer;
  @belongsTo('user', { async: true }) user;

  @attr('number') rating; // 1-5 stelle
  @attr('string') reviewText;
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;
}
```

---

#### Model: `performer-service`
```javascript
// app/models/performer-service.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class PerformerServiceModel extends Model {
  @belongsTo('performer', { async: true }) performer;

  @attr('string') serviceType; // "model", "escort", etc.
  @attr('string') link;
  @attr('string') description;
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;
}
```

---

#### Model: `performer-view`
```javascript
// app/models/performer-view.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class PerformerViewModel extends Model {
  @belongsTo('performer', { async: true }) performer;
  @belongsTo('user', { async: true, inverse: null }) user; // nullable per anonimi

  @attr('date-utc') viewedAt;
}
```

---

#### Model: `user-favorite`
```javascript
// app/models/user-favorite.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class UserFavoriteModel extends Model {
  @belongsTo('user', { async: true }) user;
  @belongsTo('performer', { async: true }) performer;

  @attr('date-utc') createdAt;
}
```

---

#### Models Geografici

**Model: `stato` (Nazione)**
```javascript
// app/models/stato.js
import Model, { attr, hasMany } from '@ember-data/model';

export default class StatoModel extends Model {
  @attr('string') name;
  @attr('string') code; // ISO code (IT, FR, ES, etc.)
  @hasMany('regione', { async: true }) regioni;
}
```

**Model: `regione`**
```javascript
// app/models/regione.js
import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class RegioneModel extends Model {
  @belongsTo('stato', { async: true }) stato;
  @hasMany('provincia', { async: true }) province;

  @attr('string') name;
  @attr('string') code;
}
```

**Model: `provincia`**
```javascript
// app/models/provincia.js
import Model, { attr, belongsTo } from '@ember-data/model';

export default class ProvinciaModel extends Model {
  @belongsTo('regione', { async: true }) regione;

  @attr('string') name;
  @attr('string') code; // Sigla (MI, RM, NA, etc.)
}
```

---

### Custom Transforms

**Già disponibili nel framework:**
- `date-utc` - Date UTC format
- `array` - Array transform
- `object` - Object transform

**Eventuali custom transforms da aggiungere:** *(Nessuno necessario al momento)*

---

## Business Logic

### User Flow

**Utente Anonimo:**
```
1. Accesso sito/app
2. Age Gate (disclaimer 18+)
3. Accettazione → Landing page
4. Ricerca performer (filtri)
5. Visualizzazione risultati
6. Click su performer → Dettaglio
7. Optional: Registrazione per features avanzate
```

**Utente Registrato:**
```
1. Login
2. Dashboard personale
3. Ricerca avanzata
4. Salvataggio favoriti
5. Notifiche personalizzate
6. Gestione profilo
```

### Performer Data

**Informazioni pubbliche (visibili a tutti):**

**Dati anagrafici:**
- Nome e cognome
- Età (calcolata da birth_date)
- Sesso (M/F)
- Localizzazione (stato, regione, provincia, città)
- Descrizione personale
- Foto profilo (appropriate, non esplicite)

**Canali e piattaforme:**
- Lista canali social (OnlyFans, Fansly, Telegram, Instagram, Facebook, X)
- Username/handle per ogni canale
- Link diretti ai profili
- Badge piattaforma verificata (se disponibile)

**Contenuti offerti:**
- Tipi di contenuti per canale (foto, video, live public/private, ecc.)
- Schedule orari live (giorni/orari per canale)

**Tariffe:**
- Range prezzi per servizio (abbonamento mensile, foto, video, live, ecc.)
- Note su pricing speciale

**Servizi aggiuntivi:**
- Servizi extra (model, escort, ecc.) con descrizione e link

**Statistiche pubbliche:**
- Rating medio (stelle)
- Numero totale recensioni
- Numero visualizzazioni profilo
- Data registrazione

**Recensioni:**
- Lista recensioni con rating e testo
- Utente recensore (nome o anonimo)
- Data recensione

**Informazioni riservate (solo utenti registrati):**
- Email del performer (solo se performer vuole renderla visibile)
- Possibilità di aggiungere ai preferiti
- Possibilità di lasciare recensioni
- Possibilità di contattare direttamente (feature futura)

---

## Development Roadmap

### Roadmap Dettagliata - Step by Step

#### FASE 1: Setup e Configurazione Base
**Obiettivo:** Preparare l'ambiente di sviluppo e struttura progetto

**Step 1.1: Verifica Setup Progetto**
- [ ] Verificare struttura cartelle WEB/app
- [ ] Verificare package.json e dipendenze installate
- [ ] Verificare ember-cli-build.js
- [ ] Verificare configurazione environment.js
- [ ] Test avvio progetto: `ember serve`

**Step 1.2: Configurazione Backend API**
- [ ] Definire URL backend in environment.js
- [ ] Configurare adapter JSON:API
- [ ] Configurare serializer JSON:API
- [ ] Test connessione API (mock o reale)

**Step 1.3: Setup Routing Base**
- [ ] Definire router.js (route pubbliche vs private)
- [ ] Creare route application.js
- [ ] Configurare route guards per utenti loggati
- [ ] Implementare redirect logic

---

#### FASE 2: Modelli EmberData
**Obiettivo:** Creare tutti i modelli per gestire i dati

**Step 2.1: Modelli Geografici**
- [ ] Creare model `stato`
- [ ] Creare model `regione`
- [ ] Creare model `provincia`
- [ ] Test relationships tra modelli geografici

**Step 2.2: Modello Performer e Relations**
- [ ] Creare model `performer`
- [ ] Creare model `channel`
- [ ] Creare model `channel-schedule`
- [ ] Creare model `channel-content-type`
- [ ] Creare model `channel-pricing`
- [ ] Test relationships performer → channels

**Step 2.3: Modelli Interazioni Utente**
- [ ] Creare model `performer-review`
- [ ] Creare model `performer-service`
- [ ] Creare model `performer-view`
- [ ] Creare model `user-favorite`
- [ ] Test CRUD operations per ogni model

---

#### FASE 3: Age Gate (PRIORITÀ LEGALE)
**Obiettivo:** Implementare age verification obbligatoria

**Step 3.1: Route e Template Age Gate**
- [ ] Creare route `age-gate`
- [ ] Creare template `age-gate.hbs`
- [ ] Implementare layout boxed style (ArchitectUI)
- [ ] Aggiungere disclaimer legale (testo da definire)

**Step 3.2: Componente Age Gate**
- [ ] Creare component `age-gate`
- [ ] Implementare checkbox "Ho 18+ anni"
- [ ] Implementare button "Accetta" e "Esci"
- [ ] Aggiungere link Privacy Policy e ToS

**Step 3.3: Cookie e Session Management**
- [ ] Implementare cookie per consenso age gate
- [ ] Gestire session storage per accesso
- [ ] Implementare check all'ingresso app
- [ ] Redirect a age-gate se non accettato

**Step 3.4: Legal Documents**
- [ ] Creare pagina Privacy Policy (placeholder)
- [ ] Creare pagina Terms of Service (placeholder)
- [ ] Link da age gate

---

#### FASE 4: Landing Page e Ricerca Base
**Obiettivo:** Implementare ricerca performer per utenti anonimi

**Step 4.1: Route Landing**
- [ ] Creare route `index` (landing page)
- [ ] Creare route `search` (risultati ricerca)
- [ ] Implementare template base

**Step 4.2: Componente Search Bar**
- [ ] Creare component `search-bar`
- [ ] Implementare input text
- [ ] Implementare search action
- [ ] Test navigation a risultati

**Step 4.3: Componente Performer Card**
- [ ] Creare component `performer-card`
- [ ] Implementare layout card (foto, nome, età, rating)
- [ ] Aggiungere platform badges
- [ ] Implementare click → dettaglio
- [ ] Mobile-responsive

**Step 4.4: Componente Performer Grid**
- [ ] Creare component `performer-grid`
- [ ] Implementare grid responsive
- [ ] Aggiungere loading skeleton
- [ ] Implementare empty state
- [ ] Test con dati mock

---

#### FASE 5: Filtri Ricerca
**Obiettivo:** Implementare sistema filtri completo

**Step 5.1: Componente Search Filters**
- [ ] Creare component `search-filters`
- [ ] Implementare filtro geografico (cascading)
- [ ] Implementare filtro sesso
- [ ] Implementare filtro età (range)
- [ ] Implementare filtro piattaforme
- [ ] Implementare button "Applica" e "Reset"

**Step 5.2: Query Builder**
- [ ] Implementare logica query parameters
- [ ] Gestire filtri multipli
- [ ] Gestire ordinamento
- [ ] Gestire paginazione
- [ ] Test con API backend

**Step 5.3: Filter Chips**
- [ ] Creare component `filter-chip`
- [ ] Mostrare filtri attivi
- [ ] Implementare rimozione singolo filtro
- [ ] Update URL query params

**Step 5.4: Mobile Drawer Filters**
- [ ] Implementare drawer collapsible su mobile
- [ ] Aggiungere button "Filtri" su mobile
- [ ] Animazioni apertura/chiusura
- [ ] Test mobile UX

---

#### FASE 6: Profilo Performer Dettagliato
**Obiettivo:** Pagina dettaglio performer con tutte le info

**Step 6.1: Route Performer Detail**
- [ ] Creare route `performers/:id`
- [ ] Implementare model() hook con includes
- [ ] Gestire loading e error states
- [ ] Implementare tracking view (POST /performers/:id/view)

**Step 6.2: Componente Performer Profile Header**
- [ ] Creare section header profilo
- [ ] Mostrare foto, nome, età, localizzazione
- [ ] Mostrare rating e recensioni totali
- [ ] Mostrare descrizione

**Step 6.3: Componente Channel List**
- [ ] Creare component `channel-list`
- [ ] Mostrare lista canali con icone
- [ ] Link esterni a profili social
- [ ] Badge verified

**Step 6.4: Componenti Schedule e Pricing**
- [ ] Creare component `schedule-calendar`
- [ ] Creare component `pricing-table`
- [ ] Mostrare orari live per canale
- [ ] Mostrare tariffe servizi

---

#### FASE 7: Sistema Recensioni
**Obiettivo:** Permettere agli utenti di recensire performer

**Step 7.1: Componente Rating Stars**
- [ ] Creare component `rating-stars`
- [ ] Implementare display stelle (read-only)
- [ ] Implementare input stelle (interactive)
- [ ] Test accessibilità

**Step 7.2: Componente Review List**
- [ ] Creare component `review-list`
- [ ] Mostrare lista recensioni
- [ ] Implementare ordinamento
- [ ] Implementare paginazione
- [ ] Empty state

**Step 7.3: Componente Review Form**
- [ ] Creare component `review-form`
- [ ] Implementare rating selector
- [ ] Implementare textarea testo
- [ ] Validazione form
- [ ] Submit recensione (POST)
- [ ] Gestire UNIQUE constraint (utente già recensito)

**Step 7.4: Integrazione in Performer Profile**
- [ ] Aggiungere review-list in profile
- [ ] Mostrare review-form (solo logged users)
- [ ] Refresh dopo submit
- [ ] Error handling

---

#### FASE 8: Sistema Preferiti (Logged Users)
**Obiettivo:** Permettere salvataggio performer preferiti

**Step 8.1: Componente Favorite Button**
- [ ] Creare component `favorite-button`
- [ ] Implementare toggle add/remove
- [ ] Implementare loading state
- [ ] Icon heart animation
- [ ] Check user logged (hide se anonimo)

**Step 8.2: Route Favorites**
- [ ] Creare route `favorites` (private)
- [ ] Implementare lista preferiti
- [ ] Usare performer-grid per layout
- [ ] Empty state "Nessun preferito"

**Step 8.3: Integrazione**
- [ ] Aggiungere favorite-button in performer-card
- [ ] Aggiungere favorite-button in performer-profile
- [ ] Sincronizzare stato across components
- [ ] Test CRUD favorites

---

#### FASE 9: Autenticazione e Registrazione
**Obiettivo:** Sistema login/registrazione utenti

**Step 9.1: Login Page**
- [ ] Creare route `login`
- [ ] Implementare template boxed style
- [ ] Form email/password
- [ ] Integrazione ember-simple-auth
- [ ] Redirect dopo login

**Step 9.2: Register Page**
- [ ] Creare route `register`
- [ ] Implementare template boxed style
- [ ] Form registrazione
- [ ] Validazione campi
- [ ] Submit registrazione

**Step 9.3: Session Management**
- [ ] Configurare session service
- [ ] Implementare auto-login con token
- [ ] Implementare logout
- [ ] Protected routes (redirect se non loggato)

**Step 9.4: User Profile**
- [ ] Creare route `profile` (private)
- [ ] Mostrare dati utente
- [ ] Implementare edit profile
- [ ] Change password

---

#### FASE 10: UI/UX Polish e Ottimizzazioni
**Obiettivo:** Raffinare interfaccia e performance

**Step 10.1: Design System**
- [ ] Definire palette colori OFinder
- [ ] Customizzare variabili Bootstrap/ArchitectUI
- [ ] Creare SCSS custom (minimo necessario)
- [ ] Definire spacing, typography, shadows

**Step 10.2: Loading States**
- [ ] Skeleton loading per cards
- [ ] Loading spinners per actions
- [ ] Progress bar per ricerche
- [ ] Smooth transitions

**Step 10.3: Empty States**
- [ ] Nessun risultato ricerca
- [ ] Nessuna recensione
- [ ] Nessun preferito
- [ ] Error states

**Step 10.4: Mobile Optimization**
- [ ] Test su device mobile reali
- [ ] Ottimizzare touch targets
- [ ] Ottimizzare drawer/modal mobile
- [ ] Test performance mobile

**Step 10.5: Performance**
- [ ] Lazy loading immagini
- [ ] Infinite scroll risultati
- [ ] Debounce search input
- [ ] Minimize API calls
- [ ] Bundle size optimization

---

#### FASE 11: SEO e Metadata
**Obiettivo:** Ottimizzare per motori di ricerca

**Step 11.1: Meta Tags**
- [ ] Implementare ember-cli-head o ember-meta
- [ ] Dynamic title per ogni page
- [ ] Meta description per ogni page
- [ ] Open Graph tags
- [ ] Structured data (Schema.org)

**Step 11.2: Sitemap**
- [ ] Generare sitemap.xml
- [ ] Submit a Google Search Console

---

#### FASE 12: Testing
**Obiettivo:** Garantire qualità del codice

**Step 12.1: Unit Tests**
- [ ] Test models (computed properties)
- [ ] Test helpers
- [ ] Test utils

**Step 12.2: Integration Tests**
- [ ] Test componenti principali
- [ ] Test forms
- [ ] Test navigation

**Step 12.3: Acceptance Tests**
- [ ] Test user flow completo (anonimo)
- [ ] Test user flow completo (loggato)
- [ ] Test age gate
- [ ] Test ricerca e filtri
- [ ] Test recensioni
- [ ] Test preferiti

---

#### FASE 13: Deploy Preparation
**Obiettivo:** Preparare per produzione

**Step 13.1: Environment Configuration**
- [ ] Configurare production environment
- [ ] Configurare staging environment
- [ ] Environment variables
- [ ] API endpoints produzione

**Step 13.2: Build Optimization**
- [ ] Minification
- [ ] Asset optimization
- [ ] Source maps configuration
- [ ] Service Worker (PWA, optional)

**Step 13.3: Cordova App Preparation**
- [ ] Build Ember per produzione
- [ ] Copy in /APP/www/
- [ ] Configurare config.xml Cordova
- [ ] Configurare Firebase per push notifications
- [ ] Test su Android/iOS

**Step 13.4: Legal Compliance Final Check**
- [ ] Age verification funzionante
- [ ] Privacy Policy completa
- [ ] Terms of Service completi
- [ ] Cookie consent banner
- [ ] GDPR compliance verificata
- [ ] Legal review (raccomandato avvocato)

---

### Roadmap Frontend-First (Senza Backend)

**Durante lo sviluppo del backend, possiamo completare:**

#### PRIORITÀ 1: Foundation & Setup
- ✅ Verifica setup progetto (Step 1.1)
- ✅ Setup routing base (Step 1.3)
- ✅ Creare tutti i modelli EmberData (Step 2.1-2.3) - pronti per API
- ✅ Configurare adapter/serializer con URL mock (Step 1.2)

#### PRIORITÀ 2: Age Gate (Legal Compliance)
- ✅ Implementare Age Gate completo (Step 3.1-3.4)
- ✅ Cookie management
- ✅ Privacy Policy e ToS pages (placeholder)

#### PRIORITÀ 3: UI Components Base
- ✅ Search bar component (Step 4.2)
- ✅ Performer card component con dati mock (Step 4.3)
- ✅ Performer grid component (Step 4.4)
- ✅ Rating stars component (Step 7.1)
- ✅ Platform badge component
- ✅ Filter chip component (Step 5.3)

#### PRIORITÀ 4: Pages & Templates
- ✅ Landing page template (Step 4.1)
- ✅ Search results page template (Step 4.1)
- ✅ Performer profile template (Step 6.1-6.4)
- ✅ Login page template boxed style (Step 9.1)
- ✅ Register page template boxed style (Step 9.2)
- ✅ Favorites page template (Step 8.2)

#### PRIORITÀ 5: Advanced Components
- ✅ Search filters component con dati mock (Step 5.1)
- ✅ Channel list component (Step 6.3)
- ✅ Schedule calendar component (Step 6.4)
- ✅ Pricing table component (Step 6.4)
- ✅ Review list component con dati mock (Step 7.2)
- ✅ Review form component (Step 7.3)
- ✅ Favorite button component (Step 8.1)

#### PRIORITÀ 6: Design System
- ✅ Palette colori OFinder (Step 10.1)
- ✅ Customizzare variabili Bootstrap/ArchitectUI (Step 10.1)
- ✅ SCSS custom componenti (Step 10.1)
- ✅ Loading states e skeleton (Step 10.2)
- ✅ Empty states (Step 10.3)

#### PRIORITÀ 7: Mobile Optimization
- ✅ Mobile drawer filters (Step 5.4)
- ✅ Test responsive tutte le pagine (Step 10.4)
- ✅ Touch optimization (Step 10.4)

#### ⏸️ RICHIEDE BACKEND (Da fare dopo):
- ❌ Test API reali (Step 1.2)
- ❌ CRUD operations con backend (Step 2.3, 7.3, 8.3)
- ❌ Autenticazione reale ember-simple-auth (Step 9.3)
- ❌ Query builder con API reale (Step 5.2)
- ❌ View tracking (Step 6.1)
- ❌ Performance optimization API calls (Step 10.5)

---

### Piano di Lavoro Immediato (No Backend)

**STEP 1: Setup Progetto** ⬅️ INIZIAMO DA QUI
```
- Verificare struttura WEB/app
- Verificare package.json
- Verificare ember-cli-build.js
- Test ember serve
- Esplorare struttura ArchitectUI
```

**STEP 2: Modelli EmberData (con dati mock)**
```
- Creare tutti i 15 modelli
- Usare Mirage o fixtures per mock data
- Test relationships
```

**STEP 3: Age Gate (Priorità Legale)**
```
- Route age-gate
- Component age-gate boxed style
- Cookie management
- Privacy/ToS placeholder pages
```

**STEP 4: Design System**
```
- Definire palette colori
- Customizzare variabili SCSS
- Setup theme base
```

**STEP 5: Componenti Base**
```
- performer-card
- performer-grid
- search-bar
- rating-stars
- platform-badge
```

**STEP 6: Landing e Search Pages**
```
- Template landing
- Template search results
- Integrazione componenti
- Mock data per test
```

**STEP 7: Performer Profile**
```
- Template profile
- channel-list component
- schedule-calendar component
- pricing-table component
- review-list component (mock data)
```

**STEP 8: Filters**
```
- search-filters component
- filter-chip component
- Mobile drawer
```

**STEP 9: Auth Pages (solo UI)**
```
- Login page boxed
- Register page boxed
- (Senza integrazione backend)
```

**STEP 10: Polish**
```
- Loading states
- Empty states
- Mobile optimization
- Animations
```

---

### Iniziamo! 🚀

Vuoi che iniziamo con **STEP 1: Verifica Setup Progetto**?

Controlleremo:
1. Struttura cartelle WEB/app
2. package.json e dipendenze
3. ember-cli-build.js
4. Avvio progetto con `ember serve`
5. Esplorazione tema ArchitectUI

Procedo?

---

## Notes

- Il framework MAE è ottimizzato per utenti loggati, ma supporta utenti anonimi (consultare guida)
- Priorità assoluta: compliance legale per contenuti adulti
- UI/UX deve essere professionale e attraente
- Mobile-first approach (app Android/iOS primary target)
- Performance cruciali (ricerca e caricamento risultati)
