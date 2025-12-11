# OFinder Framework Development Guidelines

Questo documento contiene le direttive essenziali di sviluppo per il framework OFinder basato su Ember.js 4.11.0.

## Indice

1. [JSON:API Query e Filtri](#jsonapi-query-e-filtri)
2. [Servizio json-api](#servizio-json-api)
3. [CSS e Styling](#css-e-styling)
4. [Componenti Standard](#componenti-standard)

---

## JSON:API Query e Filtri

### Sintassi Corretta per i Filtri

**❌ ERRATO:**
```javascript
const data = await this.store.query('model-name', {
  filter: { attributeId: value }  // NON usare questa sintassi
});
```

**✅ CORRETTO - Query Semplici:**
```javascript
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'relationshipName',  // Nome della relazione, NON l'attributo ID
      value: valueToFilter,
    },
  ],
  sort: 'name',
});

const data = await this.store.query('model-name', queryParams);
```

### Funzioni di Filtraggio Disponibili

Secondo lo standard JSON:API implementato da JsonApiDotNetCore ([documentazione](https://www.jsonapi.net/usage/reading/filtering.html)), le funzioni disponibili sono:

- `equals` - Uguaglianza
- `contains` - Contiene substring
- `startsWith` - Inizia con
- `endsWith` - Finisce con
- `any` - Qualsiasi valore nell'array
- `has` - Ha relazione
- `greaterThan`, `lessThan`, `greaterOrEqual`, `lessOrEqual` - Confronti numerici
- `and`, `or` - Operatori logici
- `not` - Negazione

### Esempi

#### Filtro Singolo
```javascript
// Carica tutte le regioni dell'Italia (ID: 107)
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'geoCountry',
      value: 107,
    },
  ],
  sort: 'name',
});

const regions = await this.store.query('geo-first-division', queryParams);
```

#### Filtri Multipli (AND automatico)
```javascript
// Filtra per paese E per status attivo
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'geoCountry',
      value: 107,
    },
    {
      function: 'equals',
      column: 'isActive',
      value: true,
    },
  ],
  sort: 'name',
});
```
*Nota: quando si passano più filtri nell'array, vengono automaticamente combinati con AND.*

#### Filtro con OR
```javascript
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'or',
      value: "equals(status,'active')",
      value2: "equals(status,'pending')",
    },
  ],
});
```

#### Filtro con Contains
```javascript
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'contains',
      column: 'name',
      value: 'Milano',
    },
  ],
});
```

#### Filtro con Negazione (NOT)
```javascript
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'status',
      value: 'deleted',
      negation: true,  // NOT equals
    },
  ],
});
```

#### Filtro Extra (Query Complesse Custom)
```javascript
// Per query molto complesse, usa function: null
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: null,  // Ignora il meccanismo automatico
      value: "and(equals(geoCountry,'107'),greaterThan(population,100000))",
    },
  ],
});
```

---

## Servizio json-api

### Quando Usare il Servizio json-api

**SEMPRE usare il servizio `json-api` quando:**
1. Hai filtri JSON:API da applicare
2. Hai query con più di 2-3 parametri
3. Vuoi rendere le query più leggibili e manutenibili

### Struttura Completa dei Parametri

```javascript
const queryParams = this.jsonApi.queryBuilder({
  page: {
    size: 20,      // Numero di record per pagina
    number: 1,     // Numero di pagina
  },
  filter: [
    {
      function: 'equals',
      column: 'attribute',
      value: 'value',
      value2: null,      // Opzionale, per AND/OR
      negation: false,   // Opzionale, per NOT
    },
  ],
  sort: 'name',         // Ordinamento: 'name' o '-name' (desc)
  fields: [],           // TODO: non ancora implementato
  include: [],          // TODO: non ancora implementato
});
```

### Iniettare il Servizio

```javascript
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';

export default class MyComponent extends Component {
  @service store;
  @service jsonApi;  // ← Sempre iniettare insieme a store

  async loadData() {
    const queryParams = this.jsonApi.queryBuilder({
      filter: [
        {
          function: 'equals',
          column: 'category',
          value: 'active',
        },
      ],
      sort: 'createdAt',
    });

    const data = await this.store.query('model-name', queryParams);
    return data.toArray();
  }
}
```

---

## CSS e Styling

### Regole Fondamentali

1. **MAI usare inline styles** - Sempre usare classi CSS
2. **Usare classi OFinder custom** quando disponibili (vedi `OFINDER-CSS-CLASSES.md`)
3. **Documentare nuove classi** nel file `OFINDER-CSS-CLASSES.md`
4. **Seguire la nomenclatura** `ofinder-*` per nuove classi custom

### Esempio

**❌ ERRATO:**
```handlebars
<div style="color: #f39c12; font-weight: 700;">Titolo</div>
```

**✅ CORRETTO:**
```handlebars
<div class="ofinder-heading-md ofinder-text-secondary">Titolo</div>
```

### File SCSS Organizzazione

```
app/styles/custom/ofinder/
  ├── _variables.scss      - Variabili colori, font, spacing
  ├── _typography.scss     - Classi tipografiche
  ├── _utilities.scss      - Utility classes (background, shadow, border)
  ├── _base.scss          - Stili base globali
  └── ofinder-components.scss - Stili specifici per componenti
```

---

## Componenti Standard

### Componenti Disponibili

I componenti standard sono nella directory `app/components/standard/` e includono:

- `Standard::ToolTip` - Tooltip Bootstrap
- `Standard::SelectTwo` - Select2 con ricerca
- `Standard::BackButton` - Bottone back
- `Standard::LoadingSpinner` - Spinner di caricamento
- `Standard::SliderCarousel` - Carousel immagini
- Altri...

### Esempio: Uso di ToolTip

```handlebars
<Standard::ToolTip @title="Testo del tooltip" @color="black">
  <i class="fa fa-question-circle"></i>
</Standard::ToolTip>
```

### Esempio: Uso di SelectTwo

```handlebars
<Standard::SelectTwo
  @addEmptyOption="1"
  @options={{this.optionsJSON}}
  @select2Options={{this.select2ConfigJSON}}
  @changeCB={{this.onChangeCallback}}
/>
```

**Nota:** `@options` e `@select2Options` devono essere stringhe JSON, non oggetti!

---

## Best Practices Generali

### 1. Injection dei Servizi
Sempre iniettare i servizi necessari all'inizio del component/controller:
```javascript
@service store;
@service jsonApi;
@service session;
```

### 2. Error Handling
Sempre gestire gli errori nelle chiamate async:
```javascript
try {
  const data = await this.store.query(...);
  // success handling
} catch (error) {
  console.error('Descrizione errore:', error);
  // fallback behavior
}
```

### 3. Computed Properties
Usare getter per dati derivati:
```javascript
get filteredData() {
  return this.data.filter(item => item.isActive);
}
```

### 4. Actions
Decorare i metodi event handler con `@action`:
```javascript
@action
handleClick(event) {
  // logic
}
```

---

## Risorse

- [JSON:API Filtering (JsonApiDotNetCore)](https://www.jsonapi.net/usage/reading/filtering.html)
- [JSON:API Sorting (JsonApiDotNetCore)](https://www.jsonapi.net/usage/reading/sorting.html)
- [Ember.js Guides](https://guides.emberjs.com/)
- [Bootstrap 4 Documentation](https://getbootstrap.com/docs/4.6/)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-12-11 | Initial guidelines document |

---

## Contributing

Quando aggiungi nuove direttive o best practices:
1. Mantieni il formato esistente
2. Aggiungi esempi pratici
3. Documenta sia cosa fare che cosa NON fare
4. Aggiorna il version history
