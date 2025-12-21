# deploy-frontend.ps1 - Deploy Frontend Ember su server OVH
# Autore: Claude Code
# Data: 2025-12-21

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "===  Deploy Frontend Ember (OFinder)  ===" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Configurazione
$FE_DIR = "C:\Projects\ofinder\FE\WEB"
$SERVER = "ubuntu@51.210.6.193"
$TARGET_DIR = "/var/www/ofinder-frontend/html"
$PACKAGE_NAME = "frontend-ofinder-$(Get-Date -Format 'yyyyMMdd-HHmmss').tar.gz"

# Verifica build locale
if (-not (Test-Path "$FE_DIR\dist")) {
    Write-Host "ERRORE: dist/ non trovata!" -ForegroundColor Red
    Write-Host "Esegui prima: .\build-frontend.ps1" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path "$FE_DIR\dist\index.html")) {
    Write-Host "ERRORE: dist/index.html non trovato!" -ForegroundColor Red
    Write-Host "Build incompleta - esegui prima: .\build-frontend.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "Build locale verificata" -ForegroundColor Green
Write-Host ""

# STEP 1: Preparazione directory su server
Write-Host "STEP 1: Preparazione directory su server..." -ForegroundColor Yellow

$prepareScript = @"
set -e
# Crea directory target se non esiste
sudo mkdir -p $TARGET_DIR

# Crea directory backup se non esiste
sudo mkdir -p /opt/mitfwk/backups

# Backup frontend esistente (se presente)
if [ -f $TARGET_DIR/index.html ]; then
    BACKUP_NAME=frontend-backup-`$(date +%Y%m%d-%H%M%S).tar.gz
    echo "  Backup frontend esistente..."
    sudo tar -czf /opt/mitfwk/backups/`$BACKUP_NAME -C $TARGET_DIR .
    echo "  Backup salvato: /opt/mitfwk/backups/`$BACKUP_NAME"
else
    echo "  Nessun frontend esistente (primo deploy)"
fi

# Pulizia directory target
echo "  Pulizia directory target..."
sudo rm -rf $TARGET_DIR/*

echo "  Directory pronta"
"@

ssh $SERVER $prepareScript

Write-Host "  Directory preparata su server" -ForegroundColor Green
Write-Host ""

# STEP 2: Creazione pacchetto locale
Write-Host "STEP 2: Creazione pacchetto..." -ForegroundColor Yellow

Set-Location "$FE_DIR\dist"

# Rimuovi vecchi pacchetti nella dist/
$oldPackages = Get-ChildItem "frontend-ofinder-*.tar.gz" -ErrorAction SilentlyContinue
if ($oldPackages) {
    Remove-Item "frontend-ofinder-*.tar.gz" -Force
}

# Crea pacchetto
tar -czf $PACKAGE_NAME *

$packageSize = (Get-Item $PACKAGE_NAME).Length / 1MB
Write-Host "  Pacchetto creato: $PACKAGE_NAME" -ForegroundColor Green
Write-Host "  Dimensione: $([math]::Round($packageSize, 2)) MB" -ForegroundColor Cyan
Write-Host ""

# STEP 3: Upload su server
Write-Host "STEP 3: Upload su server..." -ForegroundColor Yellow

# Upload ottimizzato (compressione + keep-alive + cipher veloce)
scp -C -o Compression=yes -o ServerAliveInterval=30 -c aes128-gcm@openssh.com `
    $PACKAGE_NAME "${SERVER}:/tmp/"

Write-Host "  Upload completato" -ForegroundColor Green
Write-Host ""

# STEP 4: Estrazione e permessi su server
Write-Host "STEP 4: Estrazione e configurazione..." -ForegroundColor Yellow

$deployScript = @"
set -e
echo "  Estrazione pacchetto..."
sudo tar -xzf /tmp/$PACKAGE_NAME -C $TARGET_DIR

echo "  Impostazione permessi..."
sudo chown -R ubuntu:ubuntu $TARGET_DIR
sudo chmod -R 755 $TARGET_DIR

echo "  Pulizia pacchetto temporaneo..."
rm /tmp/$PACKAGE_NAME

echo "  Verifica files..."
ls -lh $TARGET_DIR | head -10

# Pulizia vecchi backup (mantieni solo ultimi 5)
cd /opt/mitfwk/backups
ls -t frontend-backup-*.tar.gz 2>/dev/null | tail -n +6 | xargs -r rm -f
BACKUP_COUNT=`$(ls frontend-backup-*.tar.gz 2>/dev/null | wc -l)
echo "  Backup frontend mantenuti: `$BACKUP_COUNT"
"@

ssh $SERVER $deployScript

Write-Host "  Estrazione completata" -ForegroundColor Green
Write-Host ""

# STEP 5: Verifica deployment
Write-Host "STEP 5: Verifica deployment..." -ForegroundColor Yellow

$verifyScript = @"
set -e
if [ ! -f $TARGET_DIR/index.html ]; then
    echo "ERRORE: index.html non trovato!"
    exit 1
fi

if [ ! -d $TARGET_DIR/assets ]; then
    echo "ERRORE: assets/ non trovata!"
    exit 1
fi

FILE_COUNT=`$(find $TARGET_DIR -type f | wc -l)
echo "  File deployati: `$FILE_COUNT"
echo "  Frontend deployato correttamente"
"@

ssh $SERVER $verifyScript

Write-Host "  Verifica completata" -ForegroundColor Green
Write-Host ""

# Pulizia locale
Set-Location $FE_DIR
Remove-Item "dist\$PACKAGE_NAME" -Force -ErrorAction SilentlyContinue

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Deploy completato con successo!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Frontend deployato in: $TARGET_DIR" -ForegroundColor Yellow
Write-Host "Backup remoti: /opt/mitfwk/backups/frontend-backup-*" -ForegroundColor Cyan
Write-Host ""
Write-Host "PROSSIMO STEP:" -ForegroundColor Cyan
Write-Host "  1. Aggiorna configurazione nginx per servire frontend" -ForegroundColor Yellow
Write-Host "  2. Test: https://ofinder.it" -ForegroundColor Yellow
Write-Host ""
Write-Host "Configurazione nginx richiesta:" -ForegroundColor Yellow
Write-Host "  location / {" -ForegroundColor White
Write-Host "      root $TARGET_DIR;" -ForegroundColor White
Write-Host "      index index.html;" -ForegroundColor White
Write-Host "      try_files `$uri `$uri/ /index.html;" -ForegroundColor White
Write-Host "  }" -ForegroundColor White
Write-Host ""
