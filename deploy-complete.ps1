# deploy-complete.ps1 - Deploy Completo OFinder (Frontend + Nginx)
# Autore: Claude Code
# Data: 2025-12-21
#
# Questo script esegue il deployment completo di OFinder:
# 1. Backup configurazioni server
# 2. Build frontend Ember
# 3. Deploy frontend su server
# 4. Aggiornamento configurazione nginx
# 5. Test finale
#
# PREREQUISITI:
# - SSH configurato per ubuntu@51.210.6.193
# - Node.js e npm installati (per build frontend)
# - SSL già configurato su server (Let's Encrypt)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "===  Deploy Completo OFinder             ===" -ForegroundColor Cyan
Write-Host "===  (Frontend Ember + Nginx)            ===" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Questo script eseguirà:" -ForegroundColor Yellow
Write-Host "  1. Backup configurazioni server" -ForegroundColor White
Write-Host "  2. Build frontend Ember" -ForegroundColor White
Write-Host "  3. Deploy frontend su server OVH" -ForegroundColor White
Write-Host "  4. Aggiornamento configurazione nginx" -ForegroundColor White
Write-Host "  5. Test finale" -ForegroundColor White
Write-Host ""

# Conferma utente
$confirm = Read-Host "Continuare? (s/n)"
if ($confirm -ne "s" -and $confirm -ne "S") {
    Write-Host "Deployment annullato" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FASE 1: Backup Configurazioni Server" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\backup-server-configs.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: Backup fallito!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FASE 2: Build Frontend Ember" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\FE\build-frontend.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: Build frontend fallita!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FASE 3: Deploy Frontend su Server" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\FE\deploy-frontend.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: Deploy frontend fallito!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FASE 4: Aggiornamento Nginx" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\update-nginx.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: Aggiornamento nginx fallito!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Rollback disponibile nei backup: /opt/mitfwk/backups/" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "FASE 5: Test Finale" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Test frontend HTTPS..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

$frontendOk = $false
$apiOk = $false

# Test frontend
try {
    $response = Invoke-WebRequest -Uri "https://ofinder.it" -Method Get -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200 -and $response.Content -match "<!DOCTYPE html>") {
        Write-Host "  Frontend: OK (HTTP 200)" -ForegroundColor Green
        $frontendOk = $true
    }
} catch {
    Write-Host "  Frontend: ERRORE - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test API HTTPS..." -ForegroundColor Yellow

# Test API
try {
    $response = Invoke-WebRequest -Uri "https://ofinder.it/api" -Method Get -UseBasicParsing -TimeoutSec 10
    $statusCode = $response.StatusCode
    if ($statusCode -eq 200 -or $statusCode -eq 401) {
        Write-Host "  API: OK (HTTP $statusCode)" -ForegroundColor Green
        $apiOk = $true
    }
} catch {
    if ($_.Exception.Response) {
        $statusCode = [int]$_.Exception.Response.StatusCode
        if ($statusCode -eq 401 -or $statusCode -eq 200) {
            Write-Host "  API: OK (HTTP $statusCode)" -ForegroundColor Green
            $apiOk = $true
        } else {
            Write-Host "  API: ERRORE (HTTP $statusCode)" -ForegroundColor Red
        }
    } else {
        Write-Host "  API: ERRORE - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan

if ($frontendOk -and $apiOk) {
    Write-Host "DEPLOYMENT COMPLETATO CON SUCCESSO!" -ForegroundColor Green
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Applicazione OFinder è online!" -ForegroundColor Green
    Write-Host ""
    Write-Host "URL Applicazione: https://ofinder.it" -ForegroundColor Yellow
    Write-Host "URL API:          https://ofinder.it/api" -ForegroundColor Yellow
    Write-Host "URL Swagger:      https://ofinder.it/swagger" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "DEPLOYMENT COMPLETATO CON AVVISI" -ForegroundColor Yellow
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""

    if (-not $frontendOk) {
        Write-Host "Frontend: ERRORE - Richiede verifica" -ForegroundColor Red
    }

    if (-not $apiOk) {
        Write-Host "API: ERRORE - Richiede verifica" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Verifica manuale richiesta:" -ForegroundColor Yellow
    Write-Host "  Browser: https://ofinder.it" -ForegroundColor Cyan
    Write-Host "  Log nginx: ssh ubuntu@51.210.6.193 'sudo tail -f /var/log/nginx/ofinder_error.log'" -ForegroundColor Cyan
    Write-Host "  Log API: ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -f'" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Backup salvati in:" -ForegroundColor Yellow
$latestBackup = Get-ChildItem "C:\Projects\ofinder\server-backups\" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "  Locale: $($latestBackup.FullName)" -ForegroundColor Cyan
Write-Host "  Remoto: /opt/mitfwk/backups/" -ForegroundColor Cyan
Write-Host ""

Write-Host "Comandi utili:" -ForegroundColor Yellow
Write-Host "  Status nginx: ssh ubuntu@51.210.6.193 'sudo systemctl status nginx'" -ForegroundColor Cyan
Write-Host "  Status API:   ssh ubuntu@51.210.6.193 'sudo systemctl status mitfwk-api'" -ForegroundColor Cyan
Write-Host "  Reload nginx: ssh ubuntu@51.210.6.193 'sudo systemctl reload nginx'" -ForegroundColor Cyan
Write-Host ""
