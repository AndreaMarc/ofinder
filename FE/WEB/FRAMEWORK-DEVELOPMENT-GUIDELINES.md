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

**✅ CORRETTO - Query Semplici (1 solo filtro):**
```javascript
// Per query semplici con un solo filtro, scrivi direttamente la stringa
// IMPORTANTE: quando filtri per relazioni, usa gli attributi *Id (es. geoCountryId), NON la relazione (geoCountry)
// I valori vanno sempre messi tra quotes, sia numerici che stringhe
const data = await this.store.query('model-name', {
  filter: `equals(relationshipNameId,'${value}')`,  // Usa attributo ID con quotes
  sort: 'name',
});
```

**✅ CORRETTO - Query Complesse (2+ filtri):**
```javascript
// Per query con più filtri o complesse, usa il servizio json-api
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'relationshipName',
      value: valueToFilter,
    },
    {
      function: 'contains',
      column: 'name',
      value: 'searchText',
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

#### Filtro Singolo (Query Diretta)
```javascript
// Carica tutte le regioni dell'Italia (ID: 107)
// Query semplice -> scrivi direttamente la stringa
// IMPORTANTE: usa l'attributo geoCountryId, NON la relazione geoCountry
// I valori vanno sempre tra quotes
const regions = await this.store.query('geo-first-division', {
  filter: `equals(geoCountryId,'107')`,  // ✓ Corretto: attributo ID con quotes
  sort: 'name',
});

// Esempio con stringa
const users = await this.store.query('user', {
  filter: `equals(username,'mario.rossi')`,  // ✓ Corretto: stringa con quotes
});
```

#### Filtri Multipli (AND automatico)
```javascript
// Filtra per paese E per status attivo
// Con json-api service per query con 2+ filtri
const queryParams = this.jsonApi.queryBuilder({
  filter: [
    {
      function: 'equals',
      column: 'geoCountryId',  // Usa attributo ID, non relazione
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
      value: "and(equals(geoCountryId,'107'),greaterThan(population,'100000'))",
    },
  ],
});
```

---

## Servizio json-api

### Quando Usare il Servizio json-api

**Usare il servizio `json-api` quando:**
1. Hai **2 o più filtri** da combinare (AND/OR automatico)
2. Hai query con **molti parametri complessi** (filtri + paginazione + sort + include)
3. Hai query con **operatori logici annidati** (AND/OR/NOT complessi)

**NON usare il servizio per:**
1. **Query semplici con 1 solo filtro** - scrivi direttamente la stringa
2. **Solo ordinamento o paginazione** senza filtri - usa parametri diretti

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

### 5. Conversione RecordArray in Array
**DEPRECATO**: Non usare `.toArray()` su RecordArray o altri oggetti array-like di EmberData.

**✅ CORRETTO**: Usare il metodo nativo `.slice()` per convertire in array:
```javascript
// ❌ ERRATO
const regions = await this.store.query('geo-first-division', {...});
this.regions = regions.toArray(); // Deprecato!

// ✅ CORRETTO
const regions = await this.store.query('geo-first-division', {...});
this.regions = regions.slice(); // Usa metodo nativo
```

**Motivazione**: Il metodo `.toArray()` è deprecato in EmberData 4.x e sarà rimosso nella versione 5.0. Usare sempre i metodi nativi degli array quando disponibili.

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
| 1.1.0 | 2025-12-11 | Added JSON:API filter syntax rules (use attributeId instead of relationships) |
| 1.2.0 | 2025-12-11 | Added deprecation warning for .toArray() - use .slice() instead |

---

## Contributing

Quando aggiungi nuove direttive o best practices:
1. Mantieni il formato esistente
2. Aggiungi esempi pratici
3. Documenta sia cosa fare che cosa NON fare
4. Aggiorna il version history
