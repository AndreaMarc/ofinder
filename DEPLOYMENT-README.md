# OFinder - Deployment Completo Frontend + Nginx

## Overview

Deployment completo dell'applicazione OFinder con frontend Ember.js su server OVH.

**Server**: ubuntu@51.210.6.193
**Dominio**: ofinder.it
**SSL**: Let's Encrypt (già configurato)
**Backend**: Porta 7002 (già funzionante)
**Frontend**: Ember.js 4.11.0

## Architettura Deployment

```
https://ofinder.it/
├── /                → Frontend Ember (Nginx static files)
├── /api             → Backend API (Nginx proxy → localhost:7002)
├── /swagger         → Swagger UI (Nginx proxy → localhost:7002)
└── /uploads         → File uploads (Nginx static)
```

## Script Disponibili

### 1. Deploy Completo (CONSIGLIATO)

Esegue tutti gli step automaticamente:

```powershell
.\deploy-complete.ps1
```

**Questo script esegue in sequenza**:
1. Backup configurazioni server
2. Build frontend Ember
3. Deploy frontend su server
4. Aggiornamento nginx
5. Test finale

### 2. Script Individuali

Se preferisci eseguire gli step manualmente:

#### 2.1 Backup Configurazioni Server

```powershell
.\backup-server-configs.ps1
```

Scarica e salva in locale:
- Configurazione nginx completa (`/etc/nginx`)
- Servizio systemd backend (`/etc/systemd/system/mitfwk-api.service`)
- Applicazione completa (`/opt/mitfwk/app/`)
- Certificati SSL (`/etc/letsencrypt`) - se presenti
- Snapshot stato server (nginx -t, services, disk, ports)

**Output**: `C:\Projects\ofinder\server-backups\backup-YYYYMMDD-HHMMSS\`

#### 2.2 Build Frontend Ember

```powershell
.\FE\build-frontend.ps1
```

Esegue:
1. Verifica `node_modules` (installa se mancante)
2. Pulisce `dist/` precedente
3. Build production: `npm run build`
4. Verifica output (index.html, assets/)

**Output**: `C:\Projects\ofinder\FE\WEB\dist\`

#### 2.3 Deploy Frontend su Server

```powershell
.\FE\deploy-frontend.ps1
```

Esegue:
1. Crea directory target su server (`/var/www/ofinder-frontend/html`)
2. Backup frontend esistente (se presente)
3. Package `dist/` → tar.gz
4. Upload su server via SCP (ottimizzato)
5. Estrazione e permessi corretti
6. Verifica deployment

**Target**: `/var/www/ofinder-frontend/html/`
**Backup remoti**: `/opt/mitfwk/backups/frontend-backup-*.tar.gz`

#### 2.4 Aggiornamento Nginx

```powershell
.\update-nginx.ps1
```

Esegue:
1. Verifica frontend deployato
2. Backup configurazione nginx corrente
3. Genera nuova configurazione con:
   - Frontend Ember su root (`/`)
   - API proxy su `/api`
   - Cache ottimizzata per asset statici
   - No-cache per `index.html` (forza reload app)
4. Test configurazione (`nginx -t`)
5. Reload nginx
6. Verifica servizio attivo

**Config nginx**: `/etc/nginx/sites-available/ofinder.it`
**Backup nginx**: `/opt/mitfwk/backups/ofinder.it-backup-*.txt`

## Rollback

### Rollback Frontend

```bash
# Su server
BACKUP=$(ls -t /opt/mitfwk/backups/frontend-backup-* | head -1)
sudo rm -rf /var/www/ofinder-frontend/html/*
sudo tar -xzf $BACKUP -C /var/www/ofinder-frontend/html/
```

### Rollback Nginx

```bash
# Su server
BACKUP=$(ls -t /opt/mitfwk/backups/ofinder.it-backup-* | head -1)
sudo cp $BACKUP /etc/nginx/sites-available/ofinder.it
sudo nginx -t && sudo systemctl reload nginx
```

O usa il comando PowerShell fornito alla fine di `update-nginx.ps1`.

### Rollback Backend

(Non modificato da questi script, ma per riferimento)

```bash
# Su server
sudo systemctl stop mitfwk-api
sudo rm -rf /opt/mitfwk/app/*
BACKUP=$(ls -t /opt/mitfwk/backups/app-backup-* | head -1)
sudo cp -r $BACKUP/* /opt/mitfwk/app/
sudo chown -R ubuntu:ubuntu /opt/mitfwk/app
sudo chmod +x /opt/mitfwk/app/MIT.Fwk.WebApi
sudo systemctl start mitfwk-api
```

## Verifica Deployment

### Frontend

```bash
curl -I https://ofinder.it
# Dovrebbe restituire: HTTP/2 200
```

Browser: https://ofinder.it

### API

```bash
curl -I https://ofinder.it/api
# Dovrebbe restituire: HTTP/2 401 (o 200)
```

Browser: https://ofinder.it/swagger

### Routing Ember

Testa il routing Ember:
1. Naviga: https://ofinder.it
2. Clicca link interni (cambiano route)
3. Premi F5 (refresh) su route `/something`
4. Verifica che NON dia 404 (nginx deve servire index.html per tutte le route)

## Log e Debugging

### Log Nginx

```bash
# Errori nginx
ssh ubuntu@51.210.6.193 'sudo tail -f /var/log/nginx/ofinder_error.log'

# Access nginx
ssh ubuntu@51.210.6.193 'sudo tail -f /var/log/nginx/ofinder_access.log'

# Status nginx
ssh ubuntu@51.210.6.193 'sudo systemctl status nginx'
```

### Log Backend

```bash
# Log API
ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -f'

# Status API
ssh ubuntu@51.210.6.193 'sudo systemctl status mitfwk-api'
```

### Test Nginx Config

```bash
ssh ubuntu@51.210.6.193 'sudo nginx -t'
```

## Troubleshooting

### Frontend 404 su Refresh

**Problema**: Route Ember danno 404 su refresh (F5)

**Soluzione**: Verifica `try_files` in nginx:

```nginx
location / {
    root /var/www/ofinder-frontend/html;
    index index.html;
    try_files $uri $uri/ /index.html;  # IMPORTANTE
}
```

### API 502 Bad Gateway

**Problema**: API non raggiungibile

**Soluzione**: Verifica backend:

```bash
ssh ubuntu@51.210.6.193 'sudo systemctl status mitfwk-api'
ssh ubuntu@51.210.6.193 'curl http://localhost:7002/api'
```

### Nginx Non si Riavvia

**Problema**: `nginx -t` fallisce

**Soluzione**: Controlla sintassi configurazione:

```bash
ssh ubuntu@51.210.6.193 'sudo nginx -t'
```

Se errore, ripristina backup (vedi sezione Rollback).

### Frontend Vecchio Dopo Deploy

**Problema**: Browser mostra vecchia versione

**Soluzione**: Hard refresh browser (Ctrl+F5 o Ctrl+Shift+R)

Verifica cache nginx su `index.html`:

```nginx
location = /index.html {
    expires -1;
    add_header Cache-Control "no-store, must-revalidate";
}
```

## Configurazione Nginx Completa

La configurazione nginx finale (dopo `update-nginx.ps1`):

```nginx
# HTTP → HTTPS redirect
server {
    listen 80;
    server_name ofinder.it www.ofinder.it;
    location /.well-known/acme-challenge/ { root /var/www/html; }
    location / { return 301 https://$server_name$request_uri; }
}

# HTTPS
server {
    listen 443 ssl http2;
    server_name ofinder.it www.ofinder.it;

    # SSL
    ssl_certificate /etc/letsencrypt/live/ofinder.it/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/ofinder.it/privkey.pem;

    # API
    location /api {
        proxy_pass http://localhost:7002;
        # ... headers CORS e proxy ...
    }

    # Frontend Ember
    location / {
        root /var/www/ofinder-frontend/html;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
}
```

## Prerequisiti

### Locale (Windows)

- PowerShell 5.1+
- SSH configurato per `ubuntu@51.210.6.193` (chiave pubblica)
- SCP disponibile (Git Bash o OpenSSH)
- Node.js e npm (per build frontend)

### Server (OVH)

- Ubuntu 24.04
- Nginx installato e running
- SSL Let's Encrypt configurato
- Backend MIT.Fwk.WebApi running su porta 7002
- User `ubuntu` con permessi sudo

## Note Importanti

1. **SSL già configurato**: Gli script assumono che Let's Encrypt sia già configurato. Se non lo è, esegui prima:

   ```bash
   sudo certbot certonly --webroot -w /var/www/html \
       -d ofinder.it -d www.ofinder.it \
       --email YOUR_EMAIL --agree-tos
   ```

2. **Backend non modificato**: Questi script deployano solo il frontend. Il backend resta invariato.

3. **Backup automatici**: Ogni script crea backup prima di modificare file. Backup mantenuti: ultimi 5-6.

4. **SMTP skippato**: La configurazione SMTP OVH è stata posticipata. Il backend usa ancora Mailgun (test).

## Prossimi Step (Opzionali)

1. **SMTP OVH**: Configurare email `noreply@ofinder.it` con server SMTP OVH
2. **Monitoraggio**: Setup monitoring (Uptime Robot, Prometheus, etc.)
3. **CI/CD**: Automatizzare deployment con GitHub Actions
4. **Backup schedulati**: Cron job per backup automatici periodici
5. **CDN**: CloudFlare o simili per cache statica frontend

## Support

Per problemi:
1. Controlla log nginx e backend (vedi sezione Log e Debugging)
2. Verifica backup disponibili: `ls -lh /opt/mitfwk/backups/`
3. Esegui rollback se necessario (vedi sezione Rollback)
