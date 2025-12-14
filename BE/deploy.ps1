# deploy.ps1 - Deployment automatico Ofinder su OVH (Windows PowerShell)
# Autore: Claude Code
# Data: 2025-12-14

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "=== Ofinder Deployment Automatico ===" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# 0. Backup locale
Write-Host "STEP 0: Scaricando backup dal server..." -ForegroundColor Yellow
$BACKUP_NAME = "ofinder-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss').tar.gz"
New-Item -ItemType Directory -Force -Path ".\backups" | Out-Null

ssh ubuntu@51.210.6.193 "cd /opt/mitfwk && sudo tar -czf /tmp/$BACKUP_NAME app/ && sudo chown ubuntu:ubuntu /tmp/$BACKUP_NAME"
scp ubuntu@51.210.6.193:/tmp/$BACKUP_NAME .\backups\
ssh ubuntu@51.210.6.193 "sudo rm /tmp/$BACKUP_NAME"

Write-Host "Backup salvato in: .\backups\$BACKUP_NAME" -ForegroundColor Green
Write-Host ""

# 1. Build
Write-Host "STEP 1: Building backend (self-contained)..." -ForegroundColor Yellow
Set-Location Src\MIT.Fwk.WebApi
dotnet publish -c Release -r linux-x64 --self-contained true -o ..\..\publish\linux-x64-selfcontained
Set-Location ..\..
Write-Host "Build completato" -ForegroundColor Green
Write-Host ""

# 2. Package
Write-Host "STEP 2: Creando pacchetto deployment..." -ForegroundColor Yellow
Set-Location publish\linux-x64-selfcontained
$PACKAGE = "ofinder-api-$(Get-Date -Format 'yyyyMMdd-HHmmss').tar.gz"

# Usa tar di Windows 10/11 (disponibile nativamente)
tar -czf $PACKAGE *

$size = (Get-Item $PACKAGE).Length / 1MB
Write-Host "Pacchetto creato: $PACKAGE (dimensione: $([math]::Round($size, 2)) MB)" -ForegroundColor Green
Write-Host ""

# 3. Upload
Write-Host "STEP 3: Upload su server OVH (potrebbe richiedere qualche minuto)..." -ForegroundColor Yellow
scp $PACKAGE ubuntu@51.210.6.193:/opt/mitfwk/
Write-Host "Upload completato" -ForegroundColor Green
Write-Host ""

# 4. Deploy
Write-Host "STEP 4: Deployment su server..." -ForegroundColor Yellow

$deployScript = @"
set -e
echo "  - Fermando servizio..."
sudo systemctl stop mitfwk-api

echo "  - Backup remoto..."
sudo cp -r /opt/mitfwk/app /opt/mitfwk/backups/app-backup-`$(date +%Y%m%d-%H%M%S)

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
"@

ssh ubuntu@51.210.6.193 $deployScript

Write-Host "Deployment completato" -ForegroundColor Green
Write-Host ""

# 5. Test
Write-Host "STEP 5: Test finale..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://ofinder.it/api" -Method Get -UseBasicParsing
    $statusCode = $response.StatusCode
    Write-Host "API risponde correttamente (HTTP $statusCode)" -ForegroundColor Green
} catch {
    # In PowerShell 5.1, gli errori HTTP generano eccezioni
    if ($_.Exception.Response) {
        $statusCode = [int]$_.Exception.Response.StatusCode
        if ($statusCode -eq 401 -or $statusCode -eq 200) {
            Write-Host "API risponde correttamente (HTTP $statusCode)" -ForegroundColor Green
        } else {
            Write-Host "ATTENZIONE: API risponde con HTTP $statusCode" -ForegroundColor Yellow
            Write-Host "Verifica i log: ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -n 100'" -ForegroundColor Yellow
        }
    } else {
        Write-Host "ATTENZIONE: Impossibile raggiungere l'API" -ForegroundColor Red
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Verifica i log: ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -n 100'" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Deployment completato con successo!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pacchetto deployato: $PACKAGE"
Write-Host "Backup locale: .\backups\$BACKUP_NAME"
Write-Host "API endpoint: https://ofinder.it/api"
Write-Host ""
Write-Host "Per controllare i log sul server:" -ForegroundColor Yellow
Write-Host "  ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -f'"
Write-Host ""

# Torna alla directory BE
Set-Location ..\..
