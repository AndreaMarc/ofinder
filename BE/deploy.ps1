# deploy.ps1 - Deployment automatico Ofinder su OVH (Windows PowerShell)
# Autore: Claude Code
# Data: 2025-12-14

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "=== Ofinder Deployment Automatico ===" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# 0. Backup remoto (solo sul server, non scaricato localmente per velocita)
Write-Host "STEP 0: Verifica spazio e preparazione..." -ForegroundColor Yellow

# Nota: Il backup viene creato automaticamente durante il deployment (STEP 4)
# Non scarichiamo il backup locale per velocizzare il processo
# I backup sono disponibili su: /opt/mitfwk/backups/app-backup-YYYYMMDD-HHMMSS/

Write-Host "Preparazione completata" -ForegroundColor Green
Write-Host ""

# 1. Test OFinder
Write-Host "STEP 1: Esecuzione test OFinder..." -ForegroundColor Yellow
dotnet test Tests\MIT.Fwk.Tests.WebApi\MIT.Fwk.Tests.WebApi.csproj --filter "FullyQualifiedName~OFinderTests" --logger "console;verbosity=minimal" --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: I test OFinder sono falliti!" -ForegroundColor Red
    Write-Host "Il deployment è stato annullato per evitare di deployare codice non funzionante." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Test OFinder completati con successo" -ForegroundColor Green
Write-Host ""

# 2. Build
Write-Host "STEP 2: Building backend (self-contained)..." -ForegroundColor Yellow
Set-Location Src\MIT.Fwk.WebApi
dotnet publish -c Release -r linux-x64 --self-contained true -o ..\..\publish\linux-x64-selfcontained
Set-Location ..\..
Write-Host "Build completato" -ForegroundColor Green
Write-Host ""

# 3. Package
Write-Host "STEP 3: Creando pacchetto deployment..." -ForegroundColor Yellow
Set-Location publish\linux-x64-selfcontained

# IMPORTANTE: Rimuovi vecchi pacchetti per evitare inclusione ricorsiva!
# (Ogni tar include i .tar.gz precedenti, creando file da GB invece di MB)
$oldPackages = Get-ChildItem "ofinder-api-*.tar.gz" -ErrorAction SilentlyContinue
if ($oldPackages) {
    Write-Host "Rimuovendo $($oldPackages.Count) vecchi pacchetti..." -ForegroundColor Yellow
    Remove-Item "ofinder-api-*.tar.gz" -Force
}

$PACKAGE = "ofinder-api-$(Get-Date -Format 'yyyyMMdd-HHmmss').tar.gz"

# Crea pacchetto (esclude .tar.gz per sicurezza)
tar -czf $PACKAGE --exclude="*.tar.gz" *

$size = (Get-Item $PACKAGE).Length / 1MB
Write-Host "Pacchetto creato: $PACKAGE (dimensione: $([math]::Round($size, 2)) MB)" -ForegroundColor Green
Write-Host ""

# 4. Upload
Write-Host "STEP 4: Upload su server OVH (potrebbe richiedere qualche minuto)..." -ForegroundColor Yellow

# SCP ottimizzato: compressione + keep-alive + cipher veloce + limite bandwidth (opzionale)
# -C: abilita compressione
# -o Compression=yes: forza compressione
# -o ServerAliveInterval=30: invia keep-alive ogni 30 secondi (evita "stalled")
# -c aes128-gcm@openssh.com: cipher veloce (3x più veloce di default)
scp -C -o Compression=yes -o ServerAliveInterval=30 -o ServerAliveCountMax=3 -c aes128-gcm@openssh.com $PACKAGE ubuntu@51.210.6.193:/opt/mitfwk/

Write-Host "Upload completato" -ForegroundColor Green
Write-Host ""

# 5. Deploy
Write-Host "STEP 5: Deployment su server..." -ForegroundColor Yellow

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

echo "  - Attesa avvio (15 secondi)..."
sleep 15

echo "  - Verifica stato servizio..."
sudo systemctl is-active mitfwk-api || (echo "ERRORE: Servizio non attivo!" && sudo systemctl status mitfwk-api --no-pager -l && exit 1)

echo "  - Pulizia vecchi pacchetti..."
# Mantieni solo gli ultimi 3 pacchetti (per rollback) e cancella i più vecchi
cd /opt/mitfwk
ls -t ofinder-api-*.tar.gz 2>/dev/null | tail -n +4 | xargs -r rm -f
echo "    Pacchetti rimanenti: `$(ls ofinder-api-*.tar.gz 2>/dev/null | wc -l)"

# Mantieni solo gli ultimi 5 backup (per sicurezza)
cd /opt/mitfwk/backups
ls -td app-backup-* 2>/dev/null | tail -n +6 | xargs -r rm -rf
echo "    Backup rimanenti: `$(ls -d app-backup-* 2>/dev/null | wc -l)"
"@

# Convert Windows line endings (CRLF) to Unix (LF) before sending to Linux
$deployScriptUnix = $deployScript -replace "`r`n", "`n"

# SSH ottimizzato con keep-alive (deployment può richiedere tempo)
ssh -o ServerAliveInterval=30 -o ServerAliveCountMax=3 -c aes128-gcm@openssh.com ubuntu@51.210.6.193 $deployScriptUnix

Write-Host "Deployment completato" -ForegroundColor Green
Write-Host ""

# 6. Test API con retry
Write-Host "STEP 6: Test API (con retry)..." -ForegroundColor Yellow

$apiOk = $false
$maxRetries = 6
$retryDelay = 5

for ($i = 1; $i -le $maxRetries; $i++) {
    try {
        Write-Host "  Tentativo $i/$maxRetries..." -ForegroundColor Cyan
        $response = Invoke-WebRequest -Uri "https://ofinder.it/api" -Method Get -UseBasicParsing -TimeoutSec 10
        $statusCode = $response.StatusCode

        if ($statusCode -eq 200 -or $statusCode -eq 401) {
            Write-Host "  API risponde correttamente (HTTP $statusCode)" -ForegroundColor Green
            $apiOk = $true
            break
        }
    } catch {
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode

            # 401 o 200 = API funziona (401 = serve autenticazione, normale)
            if ($statusCode -eq 401 -or $statusCode -eq 200) {
                Write-Host "  API risponde correttamente (HTTP $statusCode)" -ForegroundColor Green
                $apiOk = $true
                break
            } elseif ($statusCode -eq 502 -or $statusCode -eq 503) {
                # 502/503 = API ancora in avvio, riprova
                Write-Host "  API in avvio (HTTP $statusCode), attendo $retryDelay secondi..." -ForegroundColor Yellow
                if ($i -lt $maxRetries) {
                    Start-Sleep -Seconds $retryDelay
                }
            } else {
                Write-Host "  Errore HTTP $statusCode" -ForegroundColor Red
                break
            }
        } else {
            Write-Host "  Errore connessione: $($_.Exception.Message)" -ForegroundColor Red
            if ($i -lt $maxRetries) {
                Start-Sleep -Seconds $retryDelay
            }
        }
    }
}

if (-not $apiOk) {
    Write-Host ""
    Write-Host "ATTENZIONE: API non risponde correttamente dopo $maxRetries tentativi!" -ForegroundColor Red
    Write-Host "Verifica i log: ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -n 100'" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan

if ($apiOk) {
    Write-Host "Deployment completato con successo!" -ForegroundColor Green
    Write-Host "API operativa e funzionante" -ForegroundColor Green
} else {
    Write-Host "Deployment completato con AVVISI" -ForegroundColor Yellow
    Write-Host "API non risponde correttamente - verifica necessaria!" -ForegroundColor Yellow
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pacchetto deployato: $PACKAGE"
Write-Host "Backup remoto: /opt/mitfwk/backups/app-backup-*" -ForegroundColor Cyan
Write-Host "API endpoint: https://ofinder.it/api"

if ($apiOk) {
    Write-Host "Stato API: OK" -ForegroundColor Green
} else {
    Write-Host "Stato API: ERRORE - Richiede verifica!" -ForegroundColor Red
}

Write-Host ""
Write-Host "Comandi utili:" -ForegroundColor Yellow
Write-Host "  Logs:    ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -f'"
Write-Host "  Status:  ssh ubuntu@51.210.6.193 'sudo systemctl status mitfwk-api'"
Write-Host "  Backups: ssh ubuntu@51.210.6.193 'ls -lh /opt/mitfwk/backups/'"

if (-not $apiOk) {
    Write-Host ""
    Write-Host "AZIONE RICHIESTA:" -ForegroundColor Red
    Write-Host "  1. Controlla i log per errori" -ForegroundColor Yellow
    Write-Host "  2. Verifica la licenza: ssh ubuntu@51.210.6.193 'ls -la /opt/mitfwk/app/license.lic'" -ForegroundColor Yellow
    Write-Host "  3. Se necessario, rollback: ssh ubuntu@51.210.6.193 'sudo systemctl stop mitfwk-api && sudo rm -rf /opt/mitfwk/app/* && sudo cp -r /opt/mitfwk/backups/app-backup-*/  /opt/mitfwk/app/ && sudo systemctl start mitfwk-api'" -ForegroundColor Yellow
}

Write-Host ""

# Torna alla directory BE
Set-Location ..\..

# Pulizia finale: rimuovi il pacchetto appena uploadato (è già sul server)
Write-Host "Pulizia locale..." -ForegroundColor Yellow
$localPackagePath = "publish\linux-x64-selfcontained\$PACKAGE"
if (Test-Path $localPackagePath) {
    Remove-Item $localPackagePath -Force
    Write-Host "Pacchetto locale rimosso (liberati ~70MB)" -ForegroundColor Green
}
