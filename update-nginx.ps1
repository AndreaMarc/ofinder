# update-nginx.ps1 - Aggiorna configurazione nginx per servire frontend
# Autore: Claude Code
# Data: 2025-12-21

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "===  Aggiorna Nginx per Frontend     ===" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$SERVER = "ubuntu@51.210.6.193"
$NGINX_CONFIG_PATH = "/etc/nginx/sites-available/ofinder.it"
$FRONTEND_PATH = "/var/www/ofinder-frontend/html"

# STEP 1: Verifica frontend deployato
Write-Host "STEP 1: Verifica frontend deployato..." -ForegroundColor Yellow

$checkFrontend = ssh $SERVER "[ -f $FRONTEND_PATH/index.html ] && echo 'exists' || echo 'notfound'"

if ($checkFrontend -notmatch "exists") {
    Write-Host ""
    Write-Host "ERRORE: Frontend non deployato!" -ForegroundColor Red
    Write-Host "Esegui prima: .\FE\deploy-frontend.ps1" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "  Frontend trovato" -ForegroundColor Green
Write-Host ""

# STEP 2: Backup configurazione nginx corrente
Write-Host "STEP 2: Backup configurazione nginx..." -ForegroundColor Yellow

$backupScript = @"
set -e
BACKUP_NAME=ofinder.it-backup-`$(date +%Y%m%d-%H%M%S)
sudo cp $NGINX_CONFIG_PATH /opt/mitfwk/backups/`$BACKUP_NAME
echo "Backup salvato: /opt/mitfwk/backups/`$BACKUP_NAME"
"@

ssh $SERVER $backupScript

Write-Host "  Backup completato" -ForegroundColor Green
Write-Host ""

# STEP 3: Crea nuova configurazione nginx
Write-Host "STEP 3: Aggiornamento configurazione nginx..." -ForegroundColor Yellow

$nginxConfig = @"
server {
    listen 80;
    listen [::]:80;
    server_name ofinder.it www.ofinder.it;

    location /.well-known/acme-challenge/ {
        root /var/www/html;
    }

    location / {
        return 301 https://`$server_name`$request_uri;
    }
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name ofinder.it www.ofinder.it;

    ssl_certificate /etc/letsencrypt/live/ofinder.it/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/ofinder.it/privkey.pem;
    include /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem;

    # Security headers
    add_header Strict-Transport-Security "max-age=31536000" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;

    client_max_body_size 100M;

    access_log /var/log/nginx/ofinder_access.log;
    error_log /var/log/nginx/ofinder_error.log;

    # API endpoints - proxy to localhost:7002
    location /api {
        # CORS Headers - Allow all origins
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' '*' always;
        add_header 'Access-Control-Expose-Headers' 'Content-Length,Content-Range,Authorization' always;
        add_header 'Access-Control-Max-Age' 1728000 always;

        # Handle preflight OPTIONS requests
        if (`$request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' '*' always;
            add_header 'Access-Control-Max-Age' 1728000 always;
            add_header 'Content-Type' 'text/plain; charset=utf-8' always;
            add_header 'Content-Length' 0 always;
            return 204;
        }

        proxy_pass http://localhost:7002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade `$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host `$host;
        proxy_set_header X-Real-IP `$remote_addr;
        proxy_set_header X-Forwarded-For `$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto `$scheme;
        proxy_set_header X-Forwarded-Host `$server_name;
        proxy_cache_bypass `$http_upgrade;

        proxy_connect_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Swagger (if needed for testing)
    location /swagger {
        proxy_pass http://localhost:7002;
        proxy_http_version 1.1;
        proxy_set_header Host `$host;
        proxy_set_header X-Real-IP `$remote_addr;
        proxy_set_header X-Forwarded-For `$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto `$scheme;
    }

    # Uploads
    location /uploads/ {
        alias /opt/mitfwk/uploads/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Frontend Ember - Root location
    location / {
        root $FRONTEND_PATH;
        index index.html;
        try_files `$uri `$uri/ /index.html;

        # Cache per asset statici
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # No cache per index.html (forza reload app)
        location = /index.html {
            expires -1;
            add_header Cache-Control "no-store, must-revalidate";
        }
    }
}
"@

# Converti line endings Windows (CRLF) in Unix (LF)
$nginxConfigUnix = $nginxConfig -replace "`r`n", "`n"

# Scrivi config su file temporaneo e carica sul server
$tempFile = [System.IO.Path]::GetTempFileName()
$nginxConfigUnix | Out-File -FilePath $tempFile -Encoding UTF8 -NoNewline

scp $tempFile "${SERVER}:/tmp/ofinder.it.new"
Remove-Item $tempFile

Write-Host "  Configurazione caricata su server" -ForegroundColor Green
Write-Host ""

# STEP 4: Applica configurazione e testa
Write-Host "STEP 4: Applicazione configurazione..." -ForegroundColor Yellow

$applyScript = @"
set -e

echo "  Copia nuova configurazione..."
sudo cp /tmp/ofinder.it.new $NGINX_CONFIG_PATH
sudo rm /tmp/ofinder.it.new

echo "  Test configurazione nginx..."
sudo nginx -t

if [ `$? -eq 0 ]; then
    echo "  Configurazione valida"
    echo "  Reload nginx..."
    sudo systemctl reload nginx
    echo "  Nginx riavviato"
else
    echo "  ERRORE: Configurazione non valida!"
    echo "  Ripristino backup..."
    LATEST_BACKUP=`$(ls -t /opt/mitfwk/backups/ofinder.it-backup-* | head -1)
    sudo cp `$LATEST_BACKUP $NGINX_CONFIG_PATH
    exit 1
fi
"@

ssh $SERVER $applyScript

Write-Host "  Configurazione applicata" -ForegroundColor Green
Write-Host ""

# STEP 5: Verifica servizio
Write-Host "STEP 5: Verifica servizio nginx..." -ForegroundColor Yellow

$statusCheck = ssh $SERVER "sudo systemctl is-active nginx"

if ($statusCheck -match "active") {
    Write-Host "  Nginx attivo e funzionante" -ForegroundColor Green
} else {
    Write-Host "  ATTENZIONE: Nginx non attivo!" -ForegroundColor Red
    ssh $SERVER "sudo systemctl status nginx --no-pager"
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Nginx aggiornato con successo!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configurazione applicata:" -ForegroundColor Yellow
Write-Host "  Root:     $FRONTEND_PATH" -ForegroundColor White
Write-Host "  API:      /api -> http://localhost:7002" -ForegroundColor White
Write-Host "  Uploads:  /uploads -> /opt/mitfwk/uploads/" -ForegroundColor White
Write-Host "  Swagger:  /swagger -> http://localhost:7002" -ForegroundColor White
Write-Host ""
Write-Host "PROSSIMO STEP:" -ForegroundColor Cyan
Write-Host "  Test frontend: https://ofinder.it" -ForegroundColor Yellow
Write-Host "  Test API:      https://ofinder.it/api" -ForegroundColor Yellow
Write-Host ""
Write-Host "Rollback (se necessario):" -ForegroundColor Yellow
Write-Host "  ssh $SERVER 'BACKUP=`$(ls -t /opt/mitfwk/backups/ofinder.it-backup-* | head -1) && sudo cp `$BACKUP $NGINX_CONFIG_PATH && sudo nginx -t && sudo systemctl reload nginx'" -ForegroundColor Cyan
Write-Host ""
