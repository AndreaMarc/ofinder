# backup-server-configs.ps1
# Scarica tutte le configurazioni server in locale per sicurezza
# Autore: Claude Code
# Data: 2025-12-21

$ErrorActionPreference = "Stop"
$BACKUP_DIR = "C:\Projects\ofinder\server-backups\backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$SERVER = "ubuntu@51.210.6.193"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "=== Backup Configurazioni Server ===" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Crea directory backup locale
New-Item -ItemType Directory -Force -Path $BACKUP_DIR | Out-Null
Write-Host "Directory backup: $BACKUP_DIR" -ForegroundColor Green
Write-Host ""

# BACKUP 1: Configurazione nginx completa
Write-Host "1. Backup configurazione nginx..." -ForegroundColor Yellow
try {
    scp -r "${SERVER}:/etc/nginx" "$BACKUP_DIR/nginx-etc"
    Write-Host "   - Salvato in: nginx-etc/" -ForegroundColor Green
} catch {
    Write-Host "   - ERRORE: $($_.Exception.Message)" -ForegroundColor Red
}

# BACKUP 2: Servizio systemd mitfwk-api
Write-Host "2. Backup servizio systemd..." -ForegroundColor Yellow
try {
    ssh $SERVER "sudo cat /etc/systemd/system/mitfwk-api.service" > "$BACKUP_DIR/mitfwk-api.service"
    Write-Host "   - Salvato in: mitfwk-api.service" -ForegroundColor Green
} catch {
    Write-Host "   - ERRORE: $($_.Exception.Message)" -ForegroundColor Red
}

# BACKUP 3: Applicazione completa (con configurazioni e licenza)
Write-Host "3. Backup applicazione completa..." -ForegroundColor Yellow
try {
    ssh $SERVER "cd /opt/mitfwk && sudo tar -czf /tmp/app-backup.tar.gz app/" | Out-Null
    scp "${SERVER}:/tmp/app-backup.tar.gz" "$BACKUP_DIR/app-backup.tar.gz"
    ssh $SERVER "sudo rm /tmp/app-backup.tar.gz"
    Write-Host "   - Salvato in: app-backup.tar.gz" -ForegroundColor Green
} catch {
    Write-Host "   - ERRORE: $($_.Exception.Message)" -ForegroundColor Red
}

# BACKUP 4: Certificati SSL (se esistono)
Write-Host "4. Backup certificati SSL..." -ForegroundColor Yellow
try {
    $sslExists = ssh $SERVER "[ -d /etc/letsencrypt/live/ofinder.it ] && echo 'exists' || echo 'notfound'"
    if ($sslExists -match "exists") {
        ssh $SERVER "sudo tar -czf /tmp/ssl-backup.tar.gz /etc/letsencrypt" | Out-Null
        scp "${SERVER}:/tmp/ssl-backup.tar.gz" "$BACKUP_DIR/ssl-backup.tar.gz"
        ssh $SERVER "sudo rm /tmp/ssl-backup.tar.gz"
        Write-Host "   - Salvato in: ssl-backup.tar.gz" -ForegroundColor Green
    } else {
        Write-Host "   - SSL non configurato (OK, verrà configurato dopo)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   - ERRORE: $($_.Exception.Message)" -ForegroundColor Red
}

# BACKUP 5: Info configurazioni attuali (snapshot leggibile)
Write-Host "5. Snapshot configurazioni..." -ForegroundColor Yellow
try {
    $infoScript = @"
set -e
echo "=== CONFIGURAZIONI SERVER OFINDER ===" > /tmp/server-info.txt
echo "Data: `$(date)" >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- NGINX VERSION ---" >> /tmp/server-info.txt
nginx -v 2>&1 >> /tmp/server-info.txt || echo "Nginx non installato" >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- NGINX SITES ENABLED ---" >> /tmp/server-info.txt
ls -la /etc/nginx/sites-enabled/ 2>&1 >> /tmp/server-info.txt || echo "Nessun site enabled" >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- NGINX CONFIG TEST ---" >> /tmp/server-info.txt
sudo nginx -t 2>&1 >> /tmp/server-info.txt || echo "Nginx config non valida" >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- SYSTEMD SERVICES ---" >> /tmp/server-info.txt
systemctl list-units --type=service --state=running | grep -E '(nginx|mitfwk|mysql|mongod)' >> /tmp/server-info.txt || echo "Nessun servizio trovato" >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- SSL CERTIFICATES ---" >> /tmp/server-info.txt
if [ -d /etc/letsencrypt/live/ofinder.it ]; then
    sudo certbot certificates 2>&1 >> /tmp/server-info.txt
else
    echo "SSL non configurato" >> /tmp/server-info.txt
fi
echo "" >> /tmp/server-info.txt

echo "--- DISK SPACE ---" >> /tmp/server-info.txt
df -h >> /tmp/server-info.txt
echo "" >> /tmp/server-info.txt

echo "--- LISTENING PORTS ---" >> /tmp/server-info.txt
sudo netstat -tulpn | grep LISTEN >> /tmp/server-info.txt || sudo ss -tulpn | grep LISTEN >> /tmp/server-info.txt
"@

    ssh $SERVER $infoScript
    scp "${SERVER}:/tmp/server-info.txt" "$BACKUP_DIR/server-info.txt"
    ssh $SERVER "rm /tmp/server-info.txt"
    Write-Host "   - Salvato in: server-info.txt" -ForegroundColor Green
} catch {
    Write-Host "   - ERRORE: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Backup completato!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backup salvato in: $BACKUP_DIR" -ForegroundColor Yellow
Write-Host ""
Write-Host "Contenuto backup:" -ForegroundColor Yellow
Write-Host "  - nginx-etc/          : Configurazione nginx completa" -ForegroundColor White
Write-Host "  - mitfwk-api.service  : Servizio systemd backend" -ForegroundColor White
Write-Host "  - app-backup.tar.gz   : App completa (config + licenza)" -ForegroundColor White
Write-Host "  - ssl-backup.tar.gz   : Certificati SSL (se presenti)" -ForegroundColor White
Write-Host "  - server-info.txt     : Snapshot configurazioni leggibile" -ForegroundColor White
Write-Host ""
Write-Host "PROSSIMO STEP:" -ForegroundColor Cyan
Write-Host "  Leggi server-info.txt per vedere lo stato attuale" -ForegroundColor Yellow
Write-Host "  Poi procedi con build frontend" -ForegroundColor Yellow
Write-Host ""
