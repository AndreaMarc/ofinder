# STEP 3: Age Gate Implementation (REVISED)

## Obiettivo
Sistema di verifica età obbligatorio (AGCOM - Delibera 96/25/CONS) usando **localStorage** e il sistema legale già esistente nel framework.

---

## ✅ Convenzioni Framework Applicate

- ✅ **NO COOKIES**: Usa localStorage
- ✅ **Legal System Esistente**: Usa `/terms/privacyPolicy` e `/terms/termsEndConditions`
- ✅ **NO Forms in Modals**: Age Gate è una pagina dedicata
- ✅ **ArchitectUI Classes**: Usa boxed style esistente
- ✅ **Mobile-First**: Compatible con Cordova

---

## 📁 File da Creare

### 1. Route Age Gate
**File:** `FE/WEB/app/routes/age-gate.js`

```javascript
import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class AgeGateRoute extends Route {
  @service router;

  beforeModel() {
    // Controlla localStorage (funziona con Cordova)
    const ageVerified = localStorage.getItem('ofinder-age-verified');

    if (ageVerified === 'true') {
      this.router.transitionTo('index');
    }
  }
}
```

---

### 2. Template Age Gate
**File:** `FE/WEB/app/templates/age-gate.hbs`

```handlebars
<div class="app-container">
  {{!-- Boxed style ArchitectUI (mobile-first) --}}
  <div class="h-100 bg-plum-plate">
    <div class="d-flex h-100 justify-content-center align-items-center">
      <div class="mx-auto app-login-box col-md-8 col-lg-6">
        <AgeGate />
      </div>
    </div>
  </div>
</div>
```

---

### 3. Component Age Gate Logic
**File:** `FE/WEB/app/components/age-gate.js`

```javascript
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class AgeGateComponent extends Component {
  @service router;

  @tracked accepted = false;
  @tracked errorMessage = '';

  @action
  toggleAccepted() {
    this.accepted = !this.accepted;
    this.errorMessage = '';
  }

  @action
  confirmAge() {
    if (!this.accepted) {
      this.errorMessage = 'Devi confermare di avere almeno 18 anni per continuare.';
      return;
    }

    // Salva in localStorage (persiste anche con Cordova)
    localStorage.setItem('ofinder-age-verified', 'true');
    localStorage.setItem('ofinder-age-verified-date', new Date().toISOString());

    // Redirect alla home
    this.router.transitionTo('index');
  }

  @action
  exitSite() {
    // Redirect a sito esterno
    window.location.href = 'https://www.google.com';
  }

  @action
  goToPrivacyPolicy() {
    // Usa la route esistente del framework
    this.router.transitionTo('terms', 'privacyPolicy');
  }

  @action
  goToTerms() {
    // Usa la route esistente del framework
    this.router.transitionTo('terms', 'termsEndConditions');
  }
}
```

---

### 4. Template Component Age Gate
**File:** `FE/WEB/app/components/age-gate.hbs`

```handlebars
<div class="modal-dialog w-100 mx-auto">
  <div class="modal-content">
    {{!-- Header --}}
    <div class="modal-header">
      <div class="h5 modal-title">
        <h4 class="mt-2">
          <div>Verifica Età</div>
          <span class="text-muted">Accesso riservato a maggiorenni</span>
        </h4>
      </div>
    </div>

    {{!-- Body --}}
    <div class="modal-body">
      <div class="text-center mb-4">
        <i class="pe-7s-attention icon-gradient bg-sunny-morning" style="font-size: 80px;"></i>
      </div>

      <div class="alert alert-warning" role="alert">
        <h5 class="alert-heading">⚠️ Contenuto per Adulti</h5>
        <p>
          Questo sito contiene materiale per adulti e non è adatto a minori di 18 anni.
          Per accedere, devi confermare di aver compiuto la maggiore età.
        </p>
        <hr>
        <p class="mb-0">
          <strong>Delibera AGCOM 96/25/CONS (Decreto Caivano)</strong> -
          L'accesso a piattaforme con contenuti per adulti è riservato esclusivamente
          a utenti maggiorenni in conformità alla normativa italiana.
          <br>
          Obbligatorio dal 12 Novembre 2025.
        </p>
      </div>

      {{!-- Checkbox Accettazione --}}
      <div class="position-relative form-check mt-4">
        <label class="form-check-label">
          <input
            type="checkbox"
            class="form-check-input"
            checked={{this.accepted}}
            {{on "change" this.toggleAccepted}}
          >
          <strong>Confermo di avere almeno 18 anni</strong> e di voler accedere
          a contenuti destinati a un pubblico adulto.
        </label>
      </div>

      {{#if this.errorMessage}}
        <div class="alert alert-danger mt-3" role="alert">
          {{this.errorMessage}}
        </div>
      {{/if}}

      {{!-- Link Privacy e Terms (route esistenti del framework) --}}
      <div class="mt-4 text-center">
        <small class="text-muted">
          Accedendo al sito accetti i nostri
          <a href="#" class="text-primary" {{on "click" (prevent-default this.goToTerms)}}>Termini di Servizio</a>
          e la
          <a href="#" class="text-primary" {{on "click" (prevent-default this.goToPrivacyPolicy)}}>Privacy Policy</a>.
        </small>
      </div>
    </div>

    {{!-- Footer con Bottoni --}}
    <div class="modal-footer d-flex justify-content-between">
      <button
        type="button"
        class="btn btn-secondary btn-lg"
        {{on "click" this.exitSite}}
      >
        <i class="pe-7s-close-circle mr-2"></i>
        Esci
      </button>
      <button
        type="button"
        class="btn btn-primary btn-lg"
        disabled={{not this.accepted}}
        {{on "click" this.confirmAge}}
      >
        <i class="pe-7s-check mr-2"></i>
        Entra (18+)
      </button>
    </div>
  </div>
</div>
```

---

## 🔧 File da Modificare

### 5. Application Route - Aggiungere Age Gate Check
**File:** `FE/WEB/app/routes/application.js`

**MODIFICA 1: Aggiungere age-gate e terms alla lista unloggedEnabledPages** (linea 30-42)

```javascript
// pagine che gli utenti non loggati possono visualizzare
unloggedEnabledPages = [
  'registration',
  'access-permissions',
  'confirm-registration',
  'google-login',
  'google-error',
  'recovery-password',
  'welcome-slider',
  'help-desk-unlog',
  'maintenance',
  'user-profile',
  'age-gate',      // <-- AGGIUNGI
  'terms',         // <-- AGGIUNGI
];
```

**MODIFICA 2: Aggiungere controllo Age Gate all'inizio di execBeforeModel** (dopo linea 63, PRIMA di tutto il resto)

```javascript
execBeforeModel = task({ enqueue: true }, async (transition) => {
  try {
    let self = this;
    if (window.cordova) console.log('EXEC DEVICE READY');

    // ============================================
    // AGE GATE CHECK (AGCOM Compliance)
    // ============================================
    const ageVerified = localStorage.getItem('ofinder-age-verified');
    const destinationRoute = transition.to.name;
    const publicRoutes = ['age-gate', 'terms']; // Route accessibili senza age verification

    if (ageVerified !== 'true' && !publicRoutes.includes(destinationRoute)) {
      this.router.transitionTo('age-gate');
      return false; // Stop execution
    }
    // ============================================

    await this.session.setup();

    // ... resto del codice esistente ...
```

---

### 6. Router Configuration
**File:** `FE/WEB/app/router.js`

**AGGIUNGI:**

```javascript
Router.map(function () {
  // ... altre route esistenti ...

  this.route('age-gate');

  // NOTA: terms esiste già con parametro: this.route('terms', { path: '/terms/:legal_code' });
});
```

---

## 🎨 SCSS Personalizzato (Opzionale)

### 7. Age Gate Styles
**File:** `FE/WEB/app/styles/components/_age-gate.scss`

```scss
// Background gradient (opzionale, ArchitectUI ha già bg-plum-plate)
.bg-age-gate {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  background-size: 200% 200%;
  animation: gradientShift 15s ease infinite;
}

@keyframes gradientShift {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

// Icona warning con gradient
.icon-gradient {
  background: linear-gradient(45deg, #f093fb 0%, #f5576c 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
```

**IMPORTA nel file principale:**

`FE/WEB/app/styles/app.scss`

```scss
@import 'components/age-gate';
```

---

## ✅ Test Checklist

- [ ] Creare route `age-gate.js`
- [ ] Creare template `age-gate.hbs`
- [ ] Creare component `age-gate.js`
- [ ] Creare template component `age-gate.hbs`
- [ ] Modificare `application.js` (2 modifiche)
- [ ] Aggiornare `router.js`
- [ ] (Opzionale) Aggiungere SCSS personalizzato
- [ ] Test: accesso iniziale → age-gate
- [ ] Test: checkbox non selezionata → errore
- [ ] Test: checkbox selezionata → localStorage salvato
- [ ] Test: refresh page → no age-gate (già verificato)
- [ ] Test: link Privacy Policy → `/terms/privacyPolicy`
- [ ] Test: link Terms → `/terms/termsEndConditions`
- [ ] Test: bottone Esci → redirect Google
- [ ] Test: Cordova app → localStorage persiste

---

## 🔒 Compliance AGCOM

Questa implementazione rispetta:

✅ **Delibera 96/25/CONS (Decreto Caivano)**
- Verifica età obbligatoria
- Disclaimer chiaro su contenuto adulti
- Link a Privacy Policy e ToS
- Obbligatorio dal 12 Novembre 2025

✅ **GDPR**
- Privacy Policy accessibile
- Terms & Conditions accessibili
- Consenso esplicito tramite checkbox
- Tracciamento data accettazione (localStorage)

✅ **Technical Requirements**
- localStorage (funziona con Cordova)
- Mobile-first design (ArchitectUI boxed)
- No cookies (compatibilità mobile)
- Sistema legale versionato (framework)

---

## 💡 Note Implementative

1. **localStorage vs Cookies**: localStorage persiste anche nelle app Cordova, i cookies possono avere problemi
2. **Legal System**: Il framework ha già `/terms/:legal_code` con:
   - `privacyPolicy` per Privacy Policy
   - `termsEndConditions` per Terms & Conditions
   - Versionamento automatico
   - Multi-lingua
   - Backend-managed content
3. **Route Pubbliche**: `age-gate` e `terms` devono essere accessibili senza autenticazione
4. **Application Route**: Il controllo age-gate deve avvenire PRIMA di qualsiasi altra verifica
5. **Exit Button**: Redirect a Google (o altro sito esterno) per chi non accetta

---

## 🚀 Prossimi Step

Dopo aver completato STEP 3:

- **STEP 4**: Design System (colori, tipografia OFinder)
- **STEP 5**: Componenti Base (performer-card, search-bar, rating-stars)
- **STEP 6**: Landing & Search Pages
- **STEP 7**: Performer Profile Page

---

## 📞 Backend Requirements (Futuro)

Per il sistema legale, il backend deve esporre:

- **GET** `/legal-terms?filter=and(equals(language,'it'),equals(code,'privacyPolicy'),equals(active,'true'))&sort=-version`
- **GET** `/legal-terms?filter=and(equals(language,'it'),equals(code,'termsEndConditions'),equals(active,'true'))&sort=-version`

Risposta JSON:API:
```json
{
  "data": {
    "id": "1",
    "type": "legal-terms",
    "attributes": {
      "title": "Privacy Policy",
      "content": "<html content>",
      "code": "privacyPolicy",
      "language": "it",
      "version": "1.0",
      "active": true,
      "dataActivation": "2025-01-01T00:00:00.000Z"
    }
  }
}
```
