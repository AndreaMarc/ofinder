# MAE Front-end Framework Ember based

## Overview

This is a custom front-end framework based on **Ember.js** integrated with the **ArchitectUI** theme (Bootstrap 4). The framework has been specifically designed and structured with custom conventions, services, and components that **take priority over standard Ember.js guidelines**.

The framework supports both web applications and mobile apps through Cordova integration (Android/iOS), and includes a comprehensive multi-tenant architecture with advanced features like real-time push notifications, internationalization, and custom authentication.

For detailed documentation, please refer to **FE FWK DOCS v1.1 20240530.pdf**.

## Prerequisites

You will need the following things properly installed on your computer:

* **nvm** min version 0.39.3 ([link](https://www.freecodecamp.org/news/node-version-manager-nvm-install-guide/))
* **npm** min version 9.5.1 (with nvm)
* **Node.js** min version v18.16.0 (with npm)
* **Git** ([link](https://git-scm.com/))
* [Ember CLI](https://cli.emberjs.com/release/)
* [Google Chrome](https://google.com/chrome/)
* [ember inspector for chrome](https://chrome.google.com/webstore/detail/ember-inspector/bmdblncegkenkacieihfhpjfppoconhi)
* [ember inspector for firefox](https://addons.mozilla.org/en-US/firefox/addon/ember-inspector/)

## Essential Ember Plugins

This framework requires the following Ember plugins (installed via npm):

* **ember-render-modifiers** - Provides element modifiers for lifecycle hooks
* **ember-truth-helpers** - Additional template helpers for logical operations
* **ember-concurrency** - Task management for handling async operations
* **ember-file-upload** - File upload handling with progress tracking
* **tracked-built-ins** - Tracked versions of JavaScript built-in classes (TrackedObject, TrackedArray)
* **ember-simple-auth** - Authentication and session management
* **ember-permissions** - Role-based access control and permissions

## ArchitectUI Theme

The framework uses the **ArchitectUI HTML Version** (Bootstrap 4 Admin Dashboard Template), which has been adapted from its original Handlebars/Webpack structure to work seamlessly with Ember.js as a Single Page Application (SPA).

Theme files are located in the `/ARCHITECT` directory.

### Visual Studio Code Plugin Recommended

* Ember JS Nome: Ember JS ([link](https://marketplace.visualstudio.com/items?itemName=hridoy.ember-snippets))
* Ember JS (ES6) and Handlebars code snippets ([link](https://marketplace.visualstudio.com/items?itemName=phanitejakomaravolu.EmberES6Snippets))
* Ember.js ([link](https://marketplace.visualstudio.com/items?itemName=EmberTooling.emberjs))
* ESLint ([link](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint))
* Glimmer Templates Syntax for VS Code ([link](https://marketplace.visualstudio.com/items?itemName=lifeart.vscode-glimmer-syntax))
* vscode-glimmer ([link](https://marketplace.visualstudio.com/items?itemName=chiragpat.vscode-glimmer))
* jsdoc ([link](https://marketplace.visualstudio.com/items?itemName=lllllllqw.jsdoc))
* auto rename tag ([link](https://marketplace.visualstudio.com/items?itemName=formulahendry.auto-rename-tag))
* Rainbow Brackets ([link](https://marketplace.visualstudio.com/items?itemName=2gua.rainbow-brackets))
* Region Marker ([link](https://marketplace.visualstudio.com/items?itemName=formulahendry.auto-rename-tag))

***IMPORTANT***: in the Visual Studio Code preferences set the 'Tab Size' to **2 spaces**

***RECOMMENDED***: in the Visual Studio Code settings, under "Search: Exclude", add the following criterion:

```
**/ARCHITECT (exclude Theme folder from search)
**/APP (exclude APP folder from search)
**/WEB/dist (excludes distribution folder from the search)
**/node_modules
**/bower_components
```

## Installation

* `git clone <repository-url>` this repository
* `cd poc-nuovo-fwk`
* `cd WEB`
* `npm install`

## Project Structure

```
/WEB
  /app
    /components     - Reusable UI components
    /controllers    - Route controllers
    /helpers        - Template helpers
    /models         - EmberData models
    /routes         - Route definitions
    /services       - Custom services (session, translation, push-notifications, etc.)
    /styles         - SCSS stylesheets
    /templates      - Handlebars templates
    /transforms     - Custom EmberData transforms
    /utils          - Utility functions
  /public          - Static assets
  /translations    - i18n JSON files
```

## Framework Features

### Custom Services

The framework includes several custom services that extend Ember.js functionality:

* **session** - Enhanced session management with authentication
* **translation** - Custom internationalization system (i18n)
* **push-notifications** - Real-time notifications via Firebase (mobile) and WebSocket (web)
* **web-socket** - WebSocket communication for real-time features
* **fetch** - Enhanced HTTP request handling with JSON:API support
* **audio** - Audio playback management
* **download** - File download utilities
* **headers** - Custom HTTP headers management
* **app** - Application-level state and configuration

### EmberData and Models

* Uses **EmberData** for data management with JSON:API backend
* Custom transforms: `array`, `object`, `date-utc`
* Custom query builder for flexible API queries
* Custom inflector rules for non-standard backend endpoints
* Support for complex model relationships and serialization

### Component Standards

* Components use **tracked properties** for reactivity
* **TrackedObject** and **TrackedArray** for mutable data structures
* **ember-concurrency** tasks for async operations
* Modifiers for DOM manipulation and lifecycle hooks
* Component naming follows kebab-case convention

### Internationalization (i18n)

The framework includes a **custom multi-language system**:

* Translation files located in `/public/translations/`
* Supported languages: Italian (it), English (en), French (fr), Spanish (es)
* Usage via the `translation` service: `this.translation.t('key.path')`
* Template helper: `{{t 'key.path'}}`
* Dynamic language switching without page reload

### File Upload and Management

* Integrated file upload system using **ember-file-upload**
* Support for multiple file uploads with progress tracking
* Files stored in MongoDB with metadata
* Download functionality for uploaded files
* Image preview and validation

### Push Notifications

* **Firebase Cloud Messaging (FCM)** for mobile apps (Cordova)
* **WebSocket** notifications for web applications
* Real-time notification delivery
* Notification history and management
* Badge counters and sound alerts

### Authentication and Permissions

* **ember-simple-auth** for authentication flow
* **ember-permissions** for role-based access control (RBAC)
* Session management with token refresh
* Route-level and component-level permission checks
* Multi-tenant support with tenant-specific data isolation

### Styling

* **SCSS** for stylesheets with Bootstrap 4 foundation
* ArchitectUI theme integration
* Custom component styles following BEM-like conventions
* Responsive design patterns

## Running / Development

* `ember serve`
* Visit your app at [http://localhost:4200](http://localhost:4200)
* Visit your tests at [http://localhost:4200/tests](http://localhost:4200/tests)

### Code Generators

Make use of the many generators for code, try `ember help generate` for more details

### Running Tests

* `ember test`
* `ember test --server`

### Linting

* `npm run lint`
* `npm run lint:fix`

### Building

* `ember build` (development)
* `ember build --environment production` (production)

### Cordova App Preparation

To prepare mobile apps for Android/iOS:

1. Ensure Cordova is installed: `npm install -g cordova`
2. Build the Ember app: `ember build --environment production`
3. Configure Cordova settings in `/APP` directory
4. Add platforms: `cordova platform add android` or `cordova platform add ios`
5. Configure Firebase for push notifications
6. Build the app: `cordova build android` or `cordova build ios`

Refer to the detailed PDF documentation for complete Cordova setup instructions.

## Code Standards and Conventions

This framework follows **specific conventions that take priority over standard Ember.js guidelines**:

* Use **tracked properties** instead of computed properties
* Prefer **ember-concurrency tasks** over promise chains
* Component organization: templates use `.hbs`, logic in `.js` with class syntax
* Service injection follows dependency injection pattern
* Custom query builder for EmberData queries
* Specific file naming and organization conventions

### Critical Framework Directives

**1. NO COOKIES - Use localStorage Instead**

**NEVER use browser cookies for client-side data storage.** Cookies are difficult to manage in Cordova mobile apps and can cause inconsistencies between web and mobile versions.

**Always use `localStorage` for:**
- User preferences
- Client-side flags (e.g., age verification, onboarding status)
- Cached data
- Session-related client state

**Example:**
```javascript
// ❌ WRONG - Do NOT use cookies
this.cookies.write('age-verified', 'true');

// ✅ CORRECT - Use localStorage
localStorage.setItem('age-verified', 'true');
const ageVerified = localStorage.getItem('age-verified');
```

**2. Legal Documents System**

The framework includes a built-in system for managing legal documents (Privacy Policy, Terms & Conditions):

* Model: `legal-term` with versioning, multi-language support, and activation dates
* Component: `<Standard::Core::TermsConditions @code="privacyPolicy" />` or `@code="termsEndConditions"`
* Route: `/terms/:legal_code` (e.g., `/terms/privacyPolicy`)
* Backend-managed content with HTML support

**Do NOT create separate privacy-policy or terms-of-service routes.** Use the existing `/terms/:legal_code` system.

**3. External Libraries**

* NO auto-import for external libraries
* All external libraries must be manually imported in `ember-cli-build.js`
* This policy discourages arbitrary library installation and ensures explicit dependency management

**4. Forms and Modals**

* NO forms inside modals (SweetAlert2 should only be used for confirmations)
* Use dedicated pages or components for forms
* Mobile-first approach: full-page forms work better on small screens

**5. Styling**

* Prefer ArchitectUI/Bootstrap classes over custom SCSS
* Only write custom SCSS when strictly necessary
* Maintain consistency with the existing theme

**6. Icons in Buttons**

**ALWAYS use Font Awesome icons inside buttons, NOT pe-7s-* icons.**

The pe-7s (PE-icon-7-stroke) icons are too small and difficult to read when used inside buttons. Font Awesome icons are already included in the framework and provide better visibility and consistency.

**Example:**
```handlebars
{{!-- ❌ WRONG - Do NOT use pe-7s icons in buttons --}}
<button class="btn btn-primary">
  <i class="pe-7s-check mr-2"></i>
  Conferma
</button>

{{!-- ✅ CORRECT - Use Font Awesome icons in buttons --}}
<button class="btn btn-primary">
  <i class="fa fa-check mr-2"></i>
  Conferma
</button>
```

**When to use each icon set:**
- **Font Awesome (`fa fa-*`)**: Inside buttons, important UI elements, mobile-friendly contexts
- **PE-icon-7-stroke (`pe-7s-*`)**: Page titles, section headers, decorative elements (where size is not critical)

**7. NO Dynamic Style/Class Bindings**

**NEVER bind dynamic values directly to `style` or `class` attributes.** This creates XSS (Cross-Site Scripting) vulnerabilities and generates Ember deprecation warnings.

**Always use:**
- CSS classes with conditional logic
- Predefined CSS classes based on component state
- Component modifiers for dynamic styling

**Example:**
```handlebars
{{!-- ❌ WRONG - Dynamic style binding (XSS vulnerability) --}}
<div style="height: {{this.dynamicHeight}}; color: {{this.userColor}};">
  Content
</div>

<i style="font-size: {{this.fontSize}};" class="fa fa-star"></i>

{{!-- ✅ CORRECT - Use CSS classes instead --}}
<div class="content-box content-box-{{this.size}}">
  Content
</div>

<i class="fa fa-star star-{{@size}}"></i>
```

**In SCSS:**
```scss
// Define classes for different states/sizes
.content-box {
  &.content-box-sm { height: 100px; }
  &.content-box-md { height: 200px; }
  &.content-box-lg { height: 300px; }
}

.star-sm { font-size: 14px; }
.star-md { font-size: 16px; }
.star-lg { font-size: 20px; }
```

**Why this matters:**
- Prevents XSS attacks from malicious user input
- Avoids Ember deprecation warnings
- Better performance (CSS is faster than inline styles)
- Easier to maintain and theme

**Please refer to FE FWK DOCS v1.1 20240530.pdf for complete code standards and conventions.**

## Deploying

The application can be deployed as:

1. **Web Application** - Standard Ember.js deployment to web servers
2. **Mobile App** - Cordova-based Android/iOS applications via app stores

Deployment configurations and environment-specific settings are managed through Ember CLI build system.

## Further Reading / Useful Links

* [ember.js](https://emberjs.com/)
* [ember-cli](https://cli.emberjs.com/release/)
* [Ember Guides](https://guides.emberjs.com/)
* [ember-concurrency](http://ember-concurrency.com/)
* [ember-simple-auth](https://ember-simple-auth.com/)
* **FE FWK DOCS v1.1 20240530.pdf** - Complete framework documentation (PRIORITY reference)
* Development Browser Extensions (Ember Inspector)

## Important Notes

**This framework has been customized extensively beyond standard Ember.js patterns. Always refer to the internal framework documentation (FE FWK DOCS v1.1 20240530.pdf) as the primary reference, as it takes priority over standard Ember.js documentation.**

For questions or issues specific to this framework, consult the PDF guide or contact the framework maintainers.
