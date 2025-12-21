# build-frontend.ps1 - Build Ember.js Frontend per OFinder
# Autore: Claude Code
# Data: 2025-12-21

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "===   Build Frontend Ember (OFinder)  ===" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica directory corrente
$FE_DIR = "C:\Projects\ofinder\FE\WEB"
if (-not (Test-Path $FE_DIR)) {
    Write-Host "ERRORE: Directory frontend non trovata: $FE_DIR" -ForegroundColor Red
    exit 1
}

Set-Location $FE_DIR
Write-Host "Directory: $FE_DIR" -ForegroundColor Green
Write-Host ""

# STEP 1: Verifica node_modules
Write-Host "STEP 1: Verifica dipendenze npm..." -ForegroundColor Yellow
if (-not (Test-Path "node_modules")) {
    Write-Host "  node_modules non trovato, esecuzione npm install..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERRORE: npm install fallito!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  Dipendenze installate" -ForegroundColor Green
} else {
    Write-Host "  node_modules esistente" -ForegroundColor Green
}
Write-Host ""

# STEP 2: Pulizia dist/ precedente
Write-Host "STEP 2: Pulizia build precedente..." -ForegroundColor Yellow
if (Test-Path "dist") {
    Remove-Item -Recurse -Force "dist"
    Write-Host "  dist/ rimossa" -ForegroundColor Green
} else {
    Write-Host "  dist/ non esistente (nessuna pulizia necessaria)" -ForegroundColor Green
}
Write-Host ""

# STEP 3: Build Ember production
Write-Host "STEP 3: Build Ember (production)..." -ForegroundColor Yellow
Write-Host "  Comando: npm run build" -ForegroundColor Cyan
Write-Host ""

npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERRORE: Build Ember fallita!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "  Build completata" -ForegroundColor Green
Write-Host ""

# STEP 4: Verifica output
Write-Host "STEP 4: Verifica output..." -ForegroundColor Yellow

$errors = @()

if (-not (Test-Path "dist")) {
    $errors += "dist/ non creata"
}

if (-not (Test-Path "dist/index.html")) {
    $errors += "dist/index.html non trovato"
}

if (-not (Test-Path "dist/assets")) {
    $errors += "dist/assets/ non trovata"
}

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "ERRORE: Build incompleta!" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
    exit 1
}

# Statistiche output
$distSize = (Get-ChildItem -Recurse "dist" | Measure-Object -Property Length -Sum).Sum / 1MB
$fileCount = (Get-ChildItem -Recurse "dist" -File).Count

Write-Host "  dist/ creata correttamente" -ForegroundColor Green
Write-Host "  Dimensione totale: $([math]::Round($distSize, 2)) MB" -ForegroundColor Cyan
Write-Host "  File generati: $fileCount" -ForegroundColor Cyan
Write-Host ""

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Build completata con successo!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output: $FE_DIR\dist\" -ForegroundColor Yellow
Write-Host ""
Write-Host "PROSSIMO STEP:" -ForegroundColor Cyan
Write-Host "  Esegui .\deploy-frontend.ps1 per deployare su server" -ForegroundColor Yellow
Write-Host ""
