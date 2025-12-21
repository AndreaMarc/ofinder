# OFinder Frontend - Script di Deploy

Questa cartella contiene gli script per build e deploy del frontend Ember.js.

## Script Disponibili

### Windows (PowerShell)

```powershell
# Build frontend
.\build-frontend.ps1

# Deploy su server
.\deploy-frontend.ps1
```

### Mac / Linux (Bash)

```bash
# Build frontend
./build-frontend.sh

# Deploy su server
./deploy-frontend.sh
```

## Funzionalità

Entrambe le versioni (PowerShell e Bash) eseguono le stesse operazioni:

### build-frontend (.ps1 / .sh)

1. Verifica dipendenze npm (esegue `npm install` se necessario)
2. Pulisce build precedente (`dist/`)
3. Esegue build production: `npm run build`
4. Verifica output (index.html, assets/)

**Output**: `WEB/dist/` (433 file, ~47 MB)

### deploy-frontend (.ps1 / .sh)

1. Verifica build locale (dist/ deve esistere)
2. Prepara directory su server (`/var/www/ofinder-frontend/html`)
3. Crea backup frontend esistente (se presente)
4. Crea pacchetto tar.gz (~28 MB compresso)
5. Upload su server via SCP (ottimizzato)
6. Estrazione e configurazione permessi
7. Verifica deployment
8. Pulizia vecchi backup (mantiene ultimi 5)

**Target**: `/var/www/ofinder-frontend/html/` su `ubuntu@51.210.6.193`

## Prerequisiti

### Comune (Windows/Mac/Linux)

- Node.js e npm installati
- SSH configurato per `ubuntu@51.210.6.193` (chiave pubblica)
- SCP disponibile

### Solo Mac/Linux

```bash
# Rendi eseguibili gli script (una volta)
chmod +x build-frontend.sh deploy-frontend.sh
```

## Esempio di Utilizzo

### Windows

```powershell
# Dalla cartella FE
cd C:\Projects\ofinder\FE

# Build + Deploy
.\build-frontend.ps1
.\deploy-frontend.ps1
```

### Mac/Linux

```bash
# Dalla cartella FE
cd /path/to/ofinder/FE

# Build + Deploy
./build-frontend.sh
./deploy-frontend.sh
```

## Verifica Deployment

Dopo il deploy:

```bash
# Verifica frontend
curl -I https://ofinder.it
# Dovrebbe restituire: HTTP/2 200, Content-Type: text/html

# Browser
open https://ofinder.it  # Mac
start https://ofinder.it # Windows
```

## Rollback

I backup frontend sono salvati automaticamente su server:

```bash
# Lista backup disponibili
ssh ubuntu@51.210.6.193 'ls -lh /opt/mitfwk/backups/frontend-backup-*'

# Ripristina ultimo backup
ssh ubuntu@51.210.6.193 'BACKUP=$(ls -t /opt/mitfwk/backups/frontend-backup-*.tar.gz | head -1) && \
  sudo rm -rf /var/www/ofinder-frontend/html/* && \
  sudo tar -xzf $BACKUP -C /var/www/ofinder-frontend/html/ && \
  sudo chown -R ubuntu:ubuntu /var/www/ofinder-frontend/html'
```

## Note

- **Build locale**: La build crea ~47 MB di file (433 file)
- **Package compresso**: Upload ~28 MB via SCP
- **Backup automatici**: Ogni deploy crea backup del frontend precedente
- **Pulizia automatica**: Mantiene solo ultimi 5 backup per risparmiare spazio
- **Permessi**: Gli script impostano automaticamente `ubuntu:ubuntu` con `755`

## Troubleshooting

### Errore "node_modules non trovato"

```bash
cd WEB
npm install
```

### Errore "SSH permission denied"

Configura chiave SSH:

```bash
ssh-copy-id ubuntu@51.210.6.193
```

### Errore "dist/ non trovata"

Esegui prima il build:

```bash
./build-frontend.sh  # Mac/Linux
.\build-frontend.ps1 # Windows
```

### Frontend non si aggiorna nel browser

Hard refresh:
- Windows/Linux: `Ctrl+F5` o `Ctrl+Shift+R`
- Mac: `Cmd+Shift+R`

Oppure verifica cache nginx (dovrebbe essere configurata correttamente).

## Struttura File

```
FE/
├── build-frontend.ps1    # Build (Windows)
├── build-frontend.sh     # Build (Mac/Linux)
├── deploy-frontend.ps1   # Deploy (Windows)
├── deploy-frontend.sh    # Deploy (Mac/Linux)
├── README-DEPLOY.md      # Questa guida
└── WEB/
    ├── app/              # Codice sorgente Ember
    ├── config/           # Configurazione
    ├── dist/             # Output build (generato)
    ├── node_modules/     # Dipendenze npm
    └── package.json      # Configurazione npm
```

## Link Utili

- **Applicazione**: https://ofinder.it
- **API**: https://ofinder.it/api
- **Swagger**: https://ofinder.it/swagger
- **Documentazione completa**: `../DEPLOYMENT-README.md`
