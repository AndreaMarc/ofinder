#!/bin/bash
# build-frontend.sh - Build Ember.js Frontend per OFinder (Mac/Linux)
# Autore: Claude Code
# Data: 2025-12-21

set -e

echo "=========================================="
echo "===   Build Frontend Ember (OFinder)  ==="
echo "=========================================="
echo ""

# Verifica directory corrente
FE_DIR="$(cd "$(dirname "$0")/WEB" && pwd)"
if [ ! -d "$FE_DIR" ]; then
    echo "ERRORE: Directory frontend non trovata: $FE_DIR"
    exit 1
fi

cd "$FE_DIR"
echo "Directory: $FE_DIR"
echo ""

# STEP 1: Verifica node_modules
echo "STEP 1: Verifica dipendenze npm..."
if [ ! -d "node_modules" ]; then
    echo "  node_modules non trovato, esecuzione npm install..."
    npm install
    if [ $? -ne 0 ]; then
        echo "  ERRORE: npm install fallito!"
        exit 1
    fi
    echo "  Dipendenze installate"
else
    echo "  node_modules esistente"
fi
echo ""

# STEP 2: Pulizia dist/ precedente
echo "STEP 2: Pulizia build precedente..."
if [ -d "dist" ]; then
    rm -rf dist
    echo "  dist/ rimossa"
else
    echo "  dist/ non esistente (nessuna pulizia necessaria)"
fi
echo ""

# STEP 3: Build Ember production
echo "STEP 3: Build Ember (production)..."
echo "  Comando: npm run build"
echo ""

npm run build

if [ $? -ne 0 ]; then
    echo ""
    echo "ERRORE: Build Ember fallita!"
    exit 1
fi

echo ""
echo "  Build completata"
echo ""

# STEP 4: Verifica output
echo "STEP 4: Verifica output..."

ERRORS=()

if [ ! -d "dist" ]; then
    ERRORS+=("dist/ non creata")
fi

if [ ! -f "dist/index.html" ]; then
    ERRORS+=("dist/index.html non trovato")
fi

if [ ! -d "dist/assets" ]; then
    ERRORS+=("dist/assets/ non trovata")
fi

if [ ${#ERRORS[@]} -gt 0 ]; then
    echo ""
    echo "ERRORE: Build incompleta!"
    for error in "${ERRORS[@]}"; do
        echo "  - $error"
    done
    exit 1
fi

# Statistiche output
DIST_SIZE=$(du -sh dist | cut -f1)
FILE_COUNT=$(find dist -type f | wc -l)

echo "  dist/ creata correttamente"
echo "  Dimensione totale: $DIST_SIZE"
echo "  File generati: $FILE_COUNT"
echo ""

echo "=========================================="
echo "Build completata con successo!"
echo "=========================================="
echo ""
echo "Output: $FE_DIR/dist/"
echo ""
echo "PROSSIMO STEP:"
echo "  Esegui ./deploy-frontend.sh per deployare su server"
echo ""
