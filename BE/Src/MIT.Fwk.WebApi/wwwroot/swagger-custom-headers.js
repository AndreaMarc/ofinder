/**
 * Swagger UI - Custom Headers & Auto-Login
 * Versione semplificata con debug esteso
 */

console.log('📦 swagger-custom-headers.js CARICATO!');

// Configurazione
const CONFIG = {
    STORAGE_KEY_FINGERPRINT: 'swagger_fingerPrint',
    STORAGE_KEY_TENANT: 'swagger_tenantId',
    STORAGE_KEY_TOKEN: 'swagger_bearer_token',

    DEFAULT_LOGIN: {
        email: "superadmin.seeders@maestrale.it",
        password: "28ec245edc1a1d9813acb28441ae4313",
        rememberMe: true,
        fingerPrint: "SwaggerFingerprint"
    },

    DEFAULT_FINGERPRINT: "SwaggerFingerprint", // FingerPrint di default per Swagger

    DEFAULT_VERSION: "6", // Versione API di default

    // Timing - MOLTO PIÙ LUNGHI
    DELAY_INITIAL: 3000,        // 3 secondi iniziali
    DELAY_COLLAPSE: 5000,       // 5 secondi per chiudere controller
    DELAY_PREFILL: 7000,        // 7 secondi per pre-compilare login
    RETRY_INTERVAL: 500,        // Controlla ogni 500ms
    MAX_RETRIES: 40             // Max 20 secondi di tentativi
};

console.log('⚙️ Configurazione:', CONFIG);

// Variabile globale per tenere traccia dell'endpoint login espanso
let activeLoginEndpoint = null;

// ==================== FUNZIONI UTILITY ====================

function log(emoji, message, data) {
    console.log(`${emoji} [CustomHeaders] ${message}`, data || '');
}

function waitForElement(selector, callback, maxRetries = CONFIG.MAX_RETRIES) {
    log('🔍', `Cercando elemento: ${selector}`);
    let retries = 0;

    const interval = setInterval(() => {
        const element = document.querySelector(selector);
        retries++;

        if (element) {
            clearInterval(interval);
            log('✅', `Elemento trovato: ${selector}`, element);
            callback(element);
        } else if (retries >= maxRetries) {
            clearInterval(interval);
            log('❌', `Elemento NON trovato dopo ${retries} tentativi: ${selector}`);
        } else if (retries % 5 === 0) {
            log('⏳', `Ancora in attesa (tentativo ${retries}/${maxRetries}): ${selector}`);
        }
    }, CONFIG.RETRY_INTERVAL);
}

// ==================== AUTO-POPOLA PARAMETRI ENDPOINT ====================

function setupEndpointParametersAutofill() {
    log('🔄', 'Setup auto-popolamento parametri endpoint...');

    // Observer per rilevare quando si espandono endpoint
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === 1) { // Element node
                    // Cerca input per fingerPrint e tenantId aggiunti al DOM
                    autofillEndpointParameters(node);
                }
            });
        });
    });

    // Osserva tutto il documento per nuovi nodi
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    log('✅', 'Auto-popolamento parametri attivato');
}

function autofillEndpointParameters(container) {
    // Cerca input per fingerPrint
    const fingerPrintInputs = container.querySelectorAll
        ? container.querySelectorAll('input[placeholder="fingerPrint"], tr[data-param-name="fingerPrint"] input')
        : [];

    fingerPrintInputs.forEach(input => {
        if (!input.value && !input.dataset.autofilled) {
            const globalValue = localStorage.getItem(CONFIG.STORAGE_KEY_FINGERPRINT) || CONFIG.DEFAULT_FINGERPRINT;
            input.value = globalValue;
            input.dataset.autofilled = 'true';
            input.dispatchEvent(new Event('input', { bubbles: true }));
            log('✅', 'fingerPrint auto-popolato:', globalValue);
        }
    });

    // Cerca input per tenantId
    const tenantIdInputs = container.querySelectorAll
        ? container.querySelectorAll('input[placeholder="tenantId"], tr[data-param-name="tenantId"] input')
        : [];

    tenantIdInputs.forEach(input => {
        if (!input.value && !input.dataset.autofilled) {
            const globalValue = localStorage.getItem(CONFIG.STORAGE_KEY_TENANT) || '';
            if (globalValue) {
                input.value = globalValue;
                input.dataset.autofilled = 'true';
                input.dispatchEvent(new Event('input', { bubbles: true }));
                log('✅', 'tenantId auto-popolato:', globalValue);
            }
        }
    });
}

// ==================== HEADERS GLOBALI ====================

function addCustomHeaderInputs() {
    log('🎨', 'Tentativo di aggiungere campi header...');

    if (document.getElementById('custom-headers-container')) {
        log('ℹ️', 'Campi header già presenti');
        return;
    }

    const savedFingerPrint = localStorage.getItem(CONFIG.STORAGE_KEY_FINGERPRINT) || CONFIG.DEFAULT_FINGERPRINT;
    const savedTenantId = localStorage.getItem(CONFIG.STORAGE_KEY_TENANT) || '';

    // Se non c'è fingerPrint salvato, salva il default
    if (!localStorage.getItem(CONFIG.STORAGE_KEY_FINGERPRINT)) {
        localStorage.setItem(CONFIG.STORAGE_KEY_FINGERPRINT, CONFIG.DEFAULT_FINGERPRINT);
    }

    const container = document.createElement('div');
    container.id = 'custom-headers-container';
    container.style.cssText = `
        padding: 20px;
        background: #fafafa;
        border: 2px solid #49cc90;
        border-radius: 8px;
        margin: 20px 0;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    `;

    container.innerHTML = `
        <div style="max-width: 1200px; margin: 0 auto;">
            <h4 style="margin: 0 0 15px 0; color: #49cc90; font-size: 18px;">
                🔧 Headers Globali (applicati automaticamente)
            </h4>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                <div>
                    <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #3b4151;">
                        fingerPrint
                        <span style="color: #888; font-weight: normal; font-size: 12px;">(Hash dispositivo)</span>
                    </label>
                    <input
                        type="text"
                        id="global-fingerprint"
                        placeholder="Inserisci fingerPrint"
                        value="${savedFingerPrint}"
                        style="width: 100%; padding: 10px; border: 1px solid #d9d9d9; border-radius: 4px; font-family: monospace; font-size: 14px;"
                    />
                </div>
                <div>
                    <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #3b4151;">
                        tenantId
                        <span style="color: #888; font-weight: normal; font-size: 12px;">(ID tenant)</span>
                    </label>
                    <input
                        type="text"
                        id="global-tenantid"
                        placeholder="Inserisci tenantId"
                        value="${savedTenantId}"
                        style="width: 100%; padding: 10px; border: 1px solid #d9d9d9; border-radius: 4px; font-family: monospace; font-size: 14px;"
                    />
                </div>
            </div>
            <div style="margin-top: 10px; font-size: 12px; color: #666;">
                💾 I valori vengono salvati automaticamente in localStorage
            </div>
        </div>
    `;

    // Inserisci dopo l'header di Swagger
    waitForElement('.information-container', (infoContainer) => {
        infoContainer.parentNode.insertBefore(container, infoContainer.nextSibling);
        log('✅', 'Campi header AGGIUNTI al DOM!');

        // Event listeners
        document.getElementById('global-fingerprint').addEventListener('input', (e) => {
            localStorage.setItem(CONFIG.STORAGE_KEY_FINGERPRINT, e.target.value);
            log('💾', 'fingerPrint salvato:', e.target.value);
        });

        document.getElementById('global-tenantid').addEventListener('input', (e) => {
            localStorage.setItem(CONFIG.STORAGE_KEY_TENANT, e.target.value);
            log('💾', 'tenantId salvato:', e.target.value);
        });
    });
}

// ==================== INTERCETTA RICHIESTE ====================

function setupRequestInterceptor() {
    log('🔌', 'Setup intercettazione richieste...');

    const originalFetch = window.fetch;

    window.fetch = function(...args) {
        const [url, options = {}] = args;

        // Aggiungi headers custom
        const fingerPrint = localStorage.getItem(CONFIG.STORAGE_KEY_FINGERPRINT);
        const tenantId = localStorage.getItem(CONFIG.STORAGE_KEY_TENANT);
        const token = localStorage.getItem(CONFIG.STORAGE_KEY_TOKEN);

        // Inizializza headers se non esistono
        options.headers = options.headers || {};

        // Aggiungi fingerPrint e tenantId
        if (fingerPrint) {
            options.headers['fingerPrint'] = fingerPrint;
        }
        if (tenantId) {
            options.headers['tenantId'] = tenantId;
        }

        // Aggiungi Authorization se il token esiste e non è già presente
        // Il token arriva già in formato "Bearer {jwt}"
        if (token && !options.headers['Authorization'] && !options.headers['authorization']) {
            options.headers['Authorization'] = token;
            log('🔐', 'Token Authorization aggiunto alla richiesta');
        }

        // Log degli header inviati (per debug)
        if (fingerPrint || tenantId || token) {
            log('📤', 'Headers aggiunti alla richiesta:', {
                fingerPrint: fingerPrint || 'N/A',
                tenantId: tenantId || 'N/A',
                authorization: token ? token.substring(0, 20) + '...' : 'N/A'
            });
        }

        const fetchPromise = originalFetch.apply(this, [url, options]);

        // Gestisci login - rileva tutte le varianti di URL
        const urlLower = url.toLowerCase();
        if (urlLower.includes('/account/login') && !urlLower.includes('loginotp')) {
            log('🔑', 'Richiesta di login rilevata!', url);

            fetchPromise.then(async response => {
                log('📡', 'Risposta ricevuta - Status:', response.status);

                if (response.ok && response.status === 200) {
                    try {
                        const clonedResponse = response.clone();
                        const data = await clonedResponse.json();

                        log('📥', 'Risposta login completa:', data);

                        // Estrai authorizationBearer e tenantId dalla risposta
                        const token = data.data?.authorizationBearer || data.authorizationBearer;
                        const tenantId = data.data?.tenantId || data.tenantId;

                        if (token) {
                            log('🎉', 'TOKEN TROVATO!', token.substring(0, 30) + '...');
                            localStorage.setItem(CONFIG.STORAGE_KEY_TOKEN, token);

                            // Salva anche tenantId se presente
                            if (tenantId) {
                                log('🏢', 'TENANT ID TROVATO:', tenantId);
                                localStorage.setItem(CONFIG.STORAGE_KEY_TENANT, tenantId);

                                // Aggiorna il campo tenantId nell'UI
                                const tenantIdInput = document.getElementById('global-tenantid');
                                if (tenantIdInput) {
                                    tenantIdInput.value = tenantId;
                                    log('✅', 'Campo tenantId aggiornato nell\'UI');
                                }
                            }

                            // Auto-autorizza dopo 1 secondo
                            setTimeout(() => {
                                log('🔐', 'Avvio auto-autorizzazione...');
                                autoAuthorize(token);
                            }, 1000);
                        } else {
                            log('⚠️', 'authorizationBearer NON trovato nella risposta');
                            log('🔍', 'Struttura dati:', Object.keys(data));
                            if (data.data) {
                                log('🔍', 'Chiavi in data.data:', Object.keys(data.data));
                            }
                        }
                    } catch (err) {
                        log('❌', 'Errore parsing risposta login:', err);
                    }
                } else {
                    log('❌', `Login fallito - Status: ${response.status}`);
                }
            }).catch(err => log('❌', 'Errore richiesta login:', err));
        }

        return fetchPromise;
    };

    log('✅', 'Intercettazione richieste ATTIVA!');
}

// ==================== AUTO-AUTORIZZAZIONE ====================

function autoAuthorize(token) {
    log('🔐', 'Tentativo auto-autorizzazione con token:', token.substring(0, 30) + '...');

    // Metodo 1: API Swagger UI (più affidabile)
    if (window.ui?.authActions) {
        try {
            log('🔌', 'Provo API Swagger UI...');

            window.ui.authActions.authorize({
                Bearer: {
                    name: "Bearer",
                    schema: { type: "http", scheme: "bearer" },
                    value: token
                }
            });

            log('✅', 'Auto-autorizzazione via API COMPLETATA!');
            showNotification('✅ Token inserito automaticamente! Sei autorizzato!', 'success');
            return;
        } catch (err) {
            log('⚠️', 'Metodo API fallito:', err.message);
        }
    } else {
        log('⚠️', 'window.ui.authActions non disponibile');
    }

    // Metodo 2: Simula click sul bottone Authorize
    log('🔄', 'Provo metodo manuale (click simulato)...');

    const authorizeBtn = document.querySelector('.authorize.unlocked') ||
                        document.querySelector('.btn.authorize') ||
                        document.querySelector('button.authorize');

    if (!authorizeBtn) {
        log('❌', 'Bottone Authorize NON trovato');
        showNotification('⚠️ Token salvato ma non inserito. Clicca "Authorize" manualmente', 'info');
        return;
    }

    log('🖱️', 'Click su bottone Authorize...');
    authorizeBtn.click();

    // Aspetta che si apra il dialog di autorizzazione
    setTimeout(() => {
        // Cerca il campo di input per il Bearer token
        const tokenInput = document.querySelector('input[name="Bearer"]') ||
                         document.querySelector('input[aria-label="auth-bearer-value"]') ||
                         document.querySelector('section input[type="text"]') ||
                         document.querySelector('.auth-container input[type="text"]');

        if (!tokenInput) {
            log('❌', 'Campo token NON trovato nel dialog');
            log('🔍', 'Input trovati:', document.querySelectorAll('input[type="text"]').length);
            return;
        }

        log('✍️', 'Compilo campo Bearer token...');
        tokenInput.value = token;
        tokenInput.dispatchEvent(new Event('input', { bubbles: true }));
        tokenInput.dispatchEvent(new Event('change', { bubbles: true }));

        // Cerca e clicca il bottone "Authorize" nel dialog
        setTimeout(() => {
            const authorizeDialogBtn = document.querySelector('.auth-btn-wrapper .btn.authorize') ||
                                      document.querySelector('.modal-ux .btn.authorize') ||
                                      document.querySelector('button.authorize:not(.unlocked)');

            if (authorizeDialogBtn) {
                log('🖱️', 'Click su Authorize nel dialog...');
                authorizeDialogBtn.click();

                setTimeout(() => {
                    // Chiudi il dialog
                    const closeBtn = document.querySelector('.close-modal') ||
                                   document.querySelector('.modal-ux button.close');
                    if (closeBtn) {
                        closeBtn.click();
                        log('✅', 'Dialog chiuso');
                    }

                    log('✅', 'Auto-autorizzazione COMPLETATA!');
                    showNotification('✅ Token inserito! Sei autorizzato!', 'success');
                }, 500);
            } else {
                log('❌', 'Bottone Authorize nel dialog NON trovato');
            }
        }, 500);
    }, 1000);
}

function showNotification(message, type = 'success') {
    const colors = {
        success: '#49cc90',
        error: '#f93e3e',
        info: '#3b99fc'
    };

    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 25px;
        background: ${colors[type]};
        color: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 9999;
        font-weight: 600;
        font-size: 14px;
        animation: slideIn 0.3s ease;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.transition = 'opacity 0.3s';
        notification.style.opacity = '0';
        setTimeout(() => notification.remove(), 300);
    }, 4000);
}

// ==================== CHIUDI CONTROLLER ====================

function collapseAllControllers() {
    log('📁', 'Chiudo controller non necessari...');

    const expandedSections = document.querySelectorAll('.opblock-tag-section.is-open');
    log('ℹ️', `Trovati ${expandedSections.length} controller aperti`);

    let closedCount = 0;

    expandedSections.forEach((section, index) => {
        const button = section.querySelector('.opblock-tag');
        if (button) {
            const tagText = button.textContent.toLowerCase();

            // NON chiudere il controller Account (deve rimanere aperto con il login)
            if (tagText.includes('account')) {
                log('ℹ️', 'Controller Account: lasciato aperto con login pre-compilato');
                return;
            }

            setTimeout(() => {
                button.click();
                closedCount++;
            }, index * 50); // Stagger ridotto a 50ms
        }
    });

    // IMPORTANTE: Non chiudere l'endpoint /account/login se è espanso
    if (activeLoginEndpoint && activeLoginEndpoint.classList.contains('is-open')) {
        log('🔒', 'Endpoint /account/login protetto - rimane espanso e pre-compilato');
    }

    if (closedCount > 0) {
        log('✅', `${closedCount} controller chiusi (Account e login lasciati aperti)`);
        showNotification(`${closedCount} controller chiusi`, 'info');
    }
}

// ==================== PRE-COMPILA LOGIN ====================

function expandAccountController() {
    log('📂', 'Cercando controller Account...');

    // Cerca il tag del controller Account
    const controllerTags = document.querySelectorAll('.opblock-tag');
    let accountSection = null;

    for (const tag of controllerTags) {
        const tagText = tag.textContent.toLowerCase();

        if (tagText.includes('account')) {
            log('✅', 'Controller Account trovato!');
            accountSection = tag.closest('.opblock-tag-section');

            // Se non è aperto, aprilo
            if (accountSection && !accountSection.classList.contains('is-open')) {
                log('🖱️', 'Apro controller Account...');
                tag.click();
            } else {
                log('ℹ️', 'Controller Account già aperto');
            }
            break;
        }
    }

    if (!accountSection) {
        log('❌', 'Controller Account NON trovato!');
        return;
    }

    // Ora cerca e espandi l'endpoint /account/login specifico
    log('🔍', 'Cercando endpoint /account/login...');

    setTimeout(() => {
        expandAndPrefillLoginEndpoint();
    }, 1500); // Aspetta che gli endpoint siano renderizzati nel controller
}

function expandAndPrefillLoginEndpoint() {
    log('🔑', 'Cercando riga endpoint /account/login...');

    // Cerca tutti gli opblock (endpoint) - sono le RIGHE degli endpoint
    const allEndpoints = document.querySelectorAll('.opblock');
    log('ℹ️', `Trovati ${allEndpoints.length} endpoint totali`);

    let loginEndpoint = null;

    // Cerca l'endpoint con path che include SOLO "login" (non loginotp, etc.)
    allEndpoints.forEach((endpoint) => {
        const pathSpan = endpoint.querySelector('.opblock-summary-path span') ||
                       endpoint.querySelector('.opblock-summary-path a') ||
                       endpoint.querySelector('.opblock-summary-path');

        const pathText = pathSpan ? pathSpan.textContent.toLowerCase() : '';

        const methodSpan = endpoint.querySelector('.opblock-summary-method');
        const method = methodSpan ? methodSpan.textContent.toLowerCase() : '';

        // Cerca POST /account/login (non loginotp, non altri)
        if (pathText.includes('account/login') &&
            !pathText.includes('loginotp') &&
            !pathText.includes('afterlogin') &&
            method === 'post') {
            loginEndpoint = endpoint;
            log('🎯', `Endpoint login ESATTO trovato: ${method.toUpperCase()} ${pathText.trim()}`);
            return;
        }
    });

    if (!loginEndpoint) {
        log('❌', 'Endpoint /account/login NON trovato!');
        return;
    }

    // Salva riferimento globale
    activeLoginEndpoint = loginEndpoint;

    // Aggiungi bottone "Copia JSON Login" vicino all'endpoint
    addCopyLoginButton(loginEndpoint);

    log('✅', 'Bottone copia login aggiunto all\'endpoint');
}

function addCopyLoginButton(loginEndpoint) {
    // Verifica che non esista già
    if (loginEndpoint.querySelector('.copy-login-btn')) {
        log('ℹ️', 'Bottone copia già presente');
        return;
    }

    const summary = loginEndpoint.querySelector('.opblock-summary');
    if (!summary) {
        log('❌', 'Summary non trovato per aggiungere bottone');
        return;
    }

    // Crea bottone con icona di copia
    const copyButton = document.createElement('button');
    copyButton.className = 'copy-login-btn';
    copyButton.innerHTML = '📋 Copia Login JSON';
    copyButton.style.cssText = `
        margin-left: 10px;
        padding: 6px 12px;
        background: #49cc90;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        font-weight: 600;
        transition: all 0.2s;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    `;

    // Hover effect
    copyButton.addEventListener('mouseenter', () => {
        copyButton.style.background = '#41b883';
        copyButton.style.transform = 'translateY(-1px)';
        copyButton.style.boxShadow = '0 4px 8px rgba(0,0,0,0.15)';
    });

    copyButton.addEventListener('mouseleave', () => {
        copyButton.style.background = '#49cc90';
        copyButton.style.transform = 'translateY(0)';
        copyButton.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
    });

    // Click handler - copia JSON negli appunti
    copyButton.addEventListener('click', async (e) => {
        e.stopPropagation(); // Non espandere/chiudere l'endpoint

        const loginData = {
            version: CONFIG.DEFAULT_VERSION,
            body: CONFIG.DEFAULT_LOGIN
        };

        const bodyJson = JSON.stringify(CONFIG.DEFAULT_LOGIN, null, 2);

        try {
            await navigator.clipboard.writeText(bodyJson);

            log('✅', 'JSON Login copiato negli appunti!', CONFIG.DEFAULT_LOGIN);

            // Feedback visivo
            copyButton.innerHTML = '✅ Copiato!';
            copyButton.style.background = '#41b883';

            showNotification(
                `JSON copiato! version=${CONFIG.DEFAULT_VERSION} | Incollalo nel body`,
                'success'
            );

            setTimeout(() => {
                copyButton.innerHTML = '📋 Copia Login JSON';
                copyButton.style.background = '#49cc90';
            }, 2000);
        } catch (err) {
            log('❌', 'Errore copia clipboard:', err);

            // Fallback: mostra il JSON in un alert
            copyButton.innerHTML = '❌ Errore';
            copyButton.style.background = '#f93e3e';

            alert(`Copia manualmente questo JSON:\n\n${bodyJson}\n\nversion: ${CONFIG.DEFAULT_VERSION}`);

            setTimeout(() => {
                copyButton.innerHTML = '📋 Copia Login JSON';
                copyButton.style.background = '#49cc90';
            }, 2000);
        }
    });

    // Aggiungi il bottone alla fine del summary
    summary.appendChild(copyButton);

    log('✅', 'Bottone "Copia Login JSON" aggiunto con successo');
    showNotification('Usa il bottone "📋 Copia Login JSON" per copiare le credenziali!', 'info');
}

// ==================== INIZIALIZZAZIONE PRINCIPALE ====================

function initialize() {
    log('🚀', '=== INIZIALIZZAZIONE SWAGGER CUSTOMIZATIONS ===');
    log('⏰', `Attesa iniziale: ${CONFIG.DELAY_INITIAL}ms`);

    // Aspetta che la pagina sia completamente caricata
    setTimeout(() => {
        log('1️⃣', 'STEP 1: Setup intercettazione richieste');
        setupRequestInterceptor();

        log('2️⃣', 'STEP 2: Aggiungi campi header globali');
        addCustomHeaderInputs();

        log('2️⃣b', 'STEP 2b: Setup auto-popolamento parametri endpoint');
        setupEndpointParametersAutofill();

        // Espandi Account dopo 3 secondi
        setTimeout(() => {
            log('3️⃣', 'STEP 3: Espandi controller Account e endpoint login');
            expandAccountController();
        }, 3000);

        // Chiudi altri controller dopo 15 secondi (dà tempo all'espansione e pre-compilazione)
        setTimeout(() => {
            log('4️⃣', 'STEP 4: Chiudi controller non necessari');
            collapseAllControllers();
        }, 15000);

    }, CONFIG.DELAY_INITIAL);
}

// ==================== AVVIO ====================

if (document.readyState === 'loading') {
    log('⏳', 'DOM in caricamento, attendo DOMContentLoaded...');
    document.addEventListener('DOMContentLoaded', initialize);
} else {
    log('✅', 'DOM già caricato, avvio immediato');
    initialize();
}

log('📝', 'Script completamente caricato e in attesa');
