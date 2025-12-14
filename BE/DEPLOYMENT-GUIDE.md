# Guida al Deployment su OVH - Ofinder

> **IMPORTANTE**: Questa guida si trova nella cartella `BE/` del progetto.
> Tutti i percorsi e comandi sono relativi alla cartella `BE/`.
> Prima di iniziare, assicurati di essere nella cartella BE: `cd BE`

## Informazioni Server
- **IP Server OVH**: 51.210.6.193
- **Dominio**: ofinder.it
- **Utente SSH**: ubuntu
- **Directory applicazione**: /opt/mitfwk/app
- **Servizio systemd**: mitfwk-api
- **Porta API**: 7002

## Processo di Deployment Completo

### 0. BACKUP LOCALE (IMPORTANTE - FARE SEMPRE PRIMA DEL DEPLOYMENT!)

```bash
# Scarica un backup completo dal server in locale
# Il nome del file include: Anno-Mese-Giorno-Ora-Minuto
BACKUP_NAME="ofinder-backup-$(date +%Y%m%d-%H%M%S).tar.gz"

# Crea il backup sul server e scaricalo in locale
ssh ubuntu@51.210.6.193 "cd /opt/mitfwk && sudo tar -czf /tmp/$BACKUP_NAME app/ && sudo chown ubuntu:ubuntu /tmp/$BACKUP_NAME"
scp ubuntu@51.210.6.193:/tmp/$BACKUP_NAME ./backups/
ssh ubuntu@51.210.6.193 "sudo rm /tmp/$BACKUP_NAME"

echo "Backup salvato in: ./backups/$BACKUP_NAME"
```

**Perché è importante**:
- Backup locale completo della versione funzionante
- Permette rollback veloce anche senza accesso al server
- Mantiene storico delle versioni deployate
- Nome file con timestamp per facile identificazione

### 1. Build del Backend (.NET)

```bash
# Dalla cartella BE (tutti i comandi partono da qui)
cd Src/MIT.Fwk.WebApi

# Build in modalità Release per Linux (SELF-CONTAINED)
# IMPORTANTE: Usare SEMPRE --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true -o ../../publish/linux-x64-selfcontained

# Torna alla cartella BE
cd ../..
```

**Note IMPORTANTI**:
- **Tutti i percorsi sono relativi alla cartella BE/**
- **SEMPRE usare `--self-contained true`**: include il .NET Runtime nel pacchetto (circa 67MB)
- Sul server OVH NON è installato .NET Runtime, quindi self-contained è obbligatorio
- `--self-contained false` NON FUNZIONA e causa errore "You must install .NET to run this application"
- Output in `BE/publish/linux-x64-selfcontained/`

### 2. Creazione del Pacchetto di Deployment

```bash
# Dalla cartella BE, vai in publish/linux-x64-selfcontained
cd publish/linux-x64-selfcontained

# Crea archivio tar.gz con timestamp
tar -czf ofinder-api-$(date +%Y%m%d-%H%M%S).tar.gz *
```

**Contenuto del pacchetto**:
- Binari .NET compilati (self-contained con runtime incluso)
- File di configurazione (appsettings.json, customsettings.json, dbconnections.json)
- Librerie dipendenze
- Runtime .NET completo (circa 60MB)
- **NOTA**: File di licenza (license.lic) deve essere copiato manualmente sul server se non presente

### 3. Upload sul Server OVH

```bash
# Upload tramite SCP (dalla directory BE/publish/linux-x64-selfcontained)
scp ofinder-api-*.tar.gz ubuntu@51.210.6.193:/opt/mitfwk/

# O con path completo dalla cartella BE
scp publish/linux-x64-selfcontained/ofinder-api-*.tar.gz ubuntu@51.210.6.193:/opt/mitfwk/
```

**Credenziali**:
- Richiede chiave SSH o password per utente ubuntu
- Assicurati di avere accesso SSH configurato
- Il pacchetto è circa 67MB, quindi l'upload richiede qualche minuto

### 4. Deployment sul Server

Connettiti al server SSH:
```bash
ssh ubuntu@51.210.6.193
```

Esegui i comandi di deployment:
```bash
# 5.1 - Ferma il servizio
sudo systemctl stop mitfwk-api

# 5.2 - Backup della versione corrente (importante!)
sudo cp -r /opt/mitfwk/app /opt/mitfwk/backups/app-backup-$(date +%Y%m%d-%H%M%S)

# 5.3 - Pulisci la directory app
sudo rm -rf /opt/mitfwk/app/*

# 5.4 - Estrai il nuovo pacchetto
sudo tar -xzf /opt/mitfwk/ofinder-api-*.tar.gz -C /opt/mitfwk/app

# 5.5 - IMPORTANTE: Copia il file di licenza (se non presente nel pacchetto)
# Questo file è necessario per il funzionamento dell'applicazione
if [ ! -f /opt/mitfwk/app/license.lic ]; then
    sudo cp /opt/mitfwk/backups/license.lic /opt/mitfwk/app/license.lic
fi

# 5.6 - Imposta i permessi corretti
sudo chown -R ubuntu:ubuntu /opt/mitfwk/app
sudo chmod +x /opt/mitfwk/app/MIT.Fwk.WebApi

# 5.7 - Riavvia il servizio
sudo systemctl start mitfwk-api

# 5.8 - Attendi qualche secondo per l'avvio
sleep 5

# 5.9 - Verifica lo stato del servizio
sudo systemctl status mitfwk-api

# 5.10 - Controlla i log per verificare che non ci siano errori
sudo journalctl -u mitfwk-api -n 50 --no-pager

# 5.11 - Controlla i log in tempo reale (opzionale)
# sudo journalctl -u mitfwk-api -f
```

**Errori Comuni**:
- **"No License: NoLicense"**: Manca il file license.lic → copiarlo dai backups
- **"You must install .NET"**: Build fatto con --self-contained false → rifare con true
- **Exit code 131**: Runtime .NET mancante → usare self-contained deployment

### 5. Verifica del Deployment

```bash
# Esegui lo script di test automatico
bash /opt/mitfwk/test-deployment.sh

# O test manuali:

# Test endpoint locale
curl http://localhost:7002/api

# Test endpoint pubblico
curl https://ofinder.it/api

# Verifica servizi attivi
sudo systemctl status mitfwk-api
sudo systemctl status nginx
sudo systemctl status mysql
sudo systemctl status mongod
```

### 6. Rollback (in caso di problemi)

```bash
# Ferma il servizio
sudo systemctl stop mitfwk-api

# Ripristina il backup
sudo rm -rf /opt/mitfwk/app/*
sudo cp -r /opt/mitfwk/backups/app-backup-YYYYMMDD-HHMMSS/* /opt/mitfwk/app/

# Riavvia
sudo systemctl start mitfwk-api
```

## Configurazione File

### File da configurare sul server (se necessario)

**dbconnections.json** - Connessioni database:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ofinder_main;User=ofinder_user;Password=...",
    "JsonApiConnection": "Server=localhost;Database=ofinder_main;User=ofinder_user;Password=...",
    "OtherDbContext": "Server=localhost;Database=ofinder_other;User=ofinder_user;Password=...",
    "NoSQLConnection": "mongodb://localhost:27017/ofinder_logs"
  }
}
```

**customsettings.json** - Impostazioni framework:
```json
{
  "JsonApiSqlProvider": "MySql",
  "OtherDbContext": "MySql",
  "EnableSwagger": false,
  "EnableSSL": true,
  "EnableJobs": true,
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext"
  ],
  "AllowedCorsOrigin": "https://ofinder.it"
}
```

## Comandi Utili

### Gestione Servizio
```bash
# Start/Stop/Restart
sudo systemctl start mitfwk-api
sudo systemctl stop mitfwk-api
sudo systemctl restart mitfwk-api

# Abilita/Disabilita all'avvio
sudo systemctl enable mitfwk-api
sudo systemctl disable mitfwk-api

# Verifica stato
sudo systemctl status mitfwk-api
```

### Log e Debugging
```bash
# Log dell'applicazione
sudo journalctl -u mitfwk-api -n 100

# Log in tempo reale
sudo journalctl -u mitfwk-api -f

# Log Nginx
sudo tail -f /var/log/nginx/ofinder_access.log
sudo tail -f /var/log/nginx/ofinder_error.log

# Log applicazione (se configurati)
tail -f /opt/mitfwk/logs/*.log
```

### Database
```bash
# Connessione MySQL
mysql -u ofinder_user -p ofinder_main

# Backup database
mysqldump -u ofinder_user -p ofinder_main > backup-$(date +%Y%m%d).sql
mysqldump -u ofinder_user -p ofinder_other > backup-other-$(date +%Y%m%d).sql

# Connessione MongoDB
mongosh ofinder_logs
```

### Certificati SSL
```bash
# Rinnovo certificato Let's Encrypt
sudo certbot renew

# Verifica scadenza
sudo certbot certificates

# Test rinnovo
sudo certbot renew --dry-run
```

## Checklist Pre-Deployment

- [ ] **BACKUP LOCALE scaricato dal server** (formato: ofinder-backup-annomesegiornooraminuto.tar.gz)
- [ ] Backup del database eseguito (opzionale, per sicurezza extra)
- [ ] Build testato in locale
- [ ] **Verificato che il build sia con --self-contained true**
- [ ] Variabili ambiente/configurazione verificate
- [ ] File license.lic presente (o disponibile per copia dai backups)
- [ ] Notifica utenti del downtime (se necessario)

## Checklist Post-Deployment

- [ ] Servizio mitfwk-api running
- [ ] API risponde su https://ofinder.it/api
- [ ] Test funzionalità critiche
- [ ] Verifica log per errori
- [ ] Monitoring attivo

## Troubleshooting

### Servizio non si avvia
```bash
# Controlla log dettagliati
sudo journalctl -u mitfwk-api -n 200 --no-pager

# Verifica permessi
ls -la /opt/mitfwk/app/MIT.Fwk.WebApi

# Test esecuzione manuale
cd /opt/mitfwk/app
./MIT.Fwk.WebApi
```

### Errori di connessione database
```bash
# Verifica MySQL
sudo systemctl status mysql
mysql -u ofinder_user -p -e "SHOW DATABASES;"

# Verifica MongoDB
sudo systemctl status mongod
mongosh --eval "db.adminCommand('listDatabases')"
```

### Problemi SSL/Nginx
```bash
# Verifica configurazione Nginx
sudo nginx -t

# Ricarica configurazione
sudo systemctl reload nginx

# Verifica certificato
sudo certbot certificates
```

## Script di Deployment Automatico

Per automatizzare il processo, puoi creare uno script:

```bash
#!/bin/bash
# deploy.sh - Deployment automatico

set -e  # Exit on error

echo "========================================="
echo "=== Ofinder Deployment Automatico ==="
echo "========================================="
echo ""

# 0. Backup locale
echo "STEP 0: Scaricando backup dal server..."
BACKUP_NAME="ofinder-backup-$(date +%Y%m%d-%H%M%S).tar.gz"
mkdir -p ./backups
ssh ubuntu@51.210.6.193 "cd /opt/mitfwk && sudo tar -czf /tmp/$BACKUP_NAME app/ && sudo chown ubuntu:ubuntu /tmp/$BACKUP_NAME"
scp ubuntu@51.210.6.193:/tmp/$BACKUP_NAME ./backups/
ssh ubuntu@51.210.6.193 "sudo rm /tmp/$BACKUP_NAME"
echo "✓ Backup salvato in: ./backups/$BACKUP_NAME"
echo ""

# 1. Build
echo "STEP 1: Building backend (self-contained)..."
cd BE/Src/MIT.Fwk.WebApi
dotnet publish -c Release -r linux-x64 --self-contained true -o ../../../publish/linux-x64-selfcontained
cd ../../..
echo "✓ Build completato"
echo ""

# 2. Package
echo "STEP 2: Creando pacchetto deployment..."
cd publish/linux-x64-selfcontained
PACKAGE="ofinder-api-$(date +%Y%m%d-%H%M%S).tar.gz"
tar -czf $PACKAGE *
echo "✓ Pacchetto creato: $PACKAGE"
echo ""

# 3. Upload
echo "STEP 3: Upload su server OVH..."
scp $PACKAGE ubuntu@51.210.6.193:/opt/mitfwk/
echo "✓ Upload completato"
echo ""

# 4. Deploy
echo "STEP 4: Deployment su server..."
ssh ubuntu@51.210.6.193 bash << ENDSSH
set -e
echo "  - Fermando servizio..."
sudo systemctl stop mitfwk-api

echo "  - Backup remoto..."
sudo cp -r /opt/mitfwk/app /opt/mitfwk/backups/app-backup-\$(date +%Y%m%d-%H%M%S)

echo "  - Pulizia directory..."
sudo rm -rf /opt/mitfwk/app/*

echo "  - Estrazione pacchetto..."
sudo tar -xzf /opt/mitfwk/$PACKAGE -C /opt/mitfwk/app

echo "  - Copia licenza..."
if [ ! -f /opt/mitfwk/app/license.lic ]; then
    sudo cp /opt/mitfwk/backups/license.lic /opt/mitfwk/app/license.lic
fi

echo "  - Impostazione permessi..."
sudo chown -R ubuntu:ubuntu /opt/mitfwk/app
sudo chmod +x /opt/mitfwk/app/MIT.Fwk.WebApi

echo "  - Riavvio servizio..."
sudo systemctl start mitfwk-api

echo "  - Attesa avvio..."
sleep 5

echo "  - Verifica stato..."
sudo systemctl status mitfwk-api --no-pager -l
ENDSSH

echo "✓ Deployment completato"
echo ""

# 5. Test
echo "STEP 5: Test finale..."
HTTP_CODE=\$(curl -s -o /dev/null -w "%{http_code}" https://ofinder.it/api)
if [ "\$HTTP_CODE" = "401" ] || [ "\$HTTP_CODE" = "200" ]; then
    echo "✓ API risponde correttamente (HTTP \$HTTP_CODE)"
else
    echo "✗ ATTENZIONE: API non risponde come previsto (HTTP \$HTTP_CODE)"
fi

echo ""
echo "========================================="
echo "✓ Deployment completato con successo!"
echo "========================================="
echo ""
echo "Pacchetto deployato: $PACKAGE"
echo "Backup locale: ./backups/$BACKUP_NAME"
echo "API endpoint: https://ofinder.it/api"
echo ""
```

**Uso dello script**:

**Windows PowerShell**:
```powershell
# Esegui deployment (dalla cartella BE)
.\deploy.ps1
```

**Linux/Mac/WSL/Git Bash**:
```bash
# Rendi eseguibile (solo la prima volta)
chmod +x deploy.sh

# Esegui deployment
./deploy.sh
```

## Note Importanti

1. **SEMPRE Self-Contained**: Usare SEMPRE `--self-contained true` nel build. Il server NON ha .NET Runtime installato.
2. **Backup Locale Prima di Tutto**: Scaricare SEMPRE un backup locale prima del deployment con nome formato `annomesegiornooraminuto`
3. **Licenza Obbligatoria**: Il file `license.lic` è obbligatorio per l'avvio dell'applicazione
4. **Downtime**: Il deployment causa un breve downtime (1-2 minuti)
5. **Test**: Testare in ambiente di sviluppo prima di deployare
6. **Rollback**: Tenere sempre pronta una strategia di rollback (backup locale + backup remoto)
7. **Monitoraggio**: Controllare i log dopo ogni deployment per verificare l'assenza di errori
8. **Dimensione Pacchetto**: Self-contained deployment è circa 67MB (vs 5MB non self-contained)

## Lezioni Apprese dal Deployment

### Problema 1: "You must install .NET to run this application"
**Causa**: Build fatto con `--self-contained false`
**Soluzione**: SEMPRE usare `--self-contained true`
**Perché**: Il server OVH non ha .NET 10 Runtime installato

### Problema 2: "No License: NoLicense" (Exit code 1)
**Causa**: File `license.lic` mancante nella directory app
**Soluzione**: Copiare `license.lic` dai backups
**Prevenzione**: Includere sempre nel deployment o copiarlo automaticamente

### Problema 3: Exit code 131 (SIGSEGV)
**Causa**: Runtime .NET non trovato o incompatibile
**Soluzione**: Usare self-contained deployment
**Verifica**: Controllare che il binario includa tutte le librerie necessarie

## Contatti e Risorse

- Server IP: 51.210.6.193
- Domain: ofinder.it
- Repository: https://github.com/scrabionau77/ofinder
