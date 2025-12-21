#!/bin/bash
# deploy-frontend.sh - Deploy Frontend Ember su server OVH (Mac/Linux)
# Autore: Claude Code
# Data: 2025-12-21

set -e

echo "=========================================="
echo "===  Deploy Frontend Ember (OFinder)  ==="
echo "=========================================="
echo ""

# Configurazione
FE_DIR="$(cd "$(dirname "$0")/WEB" && pwd)"
SERVER="ubuntu@51.210.6.193"
TARGET_DIR="/var/www/ofinder-frontend/html"
PACKAGE_NAME="frontend-ofinder-$(date +%Y%m%d-%H%M%S).tar.gz"

# Verifica build locale
if [ ! -d "$FE_DIR/dist" ]; then
    echo "ERRORE: dist/ non trovata!"
    echo "Esegui prima: ./build-frontend.sh"
    exit 1
fi

if [ ! -f "$FE_DIR/dist/index.html" ]; then
    echo "ERRORE: dist/index.html non trovato!"
    echo "Build incompleta - esegui prima: ./build-frontend.sh"
    exit 1
fi

echo "Build locale verificata"
echo ""

# STEP 1: Preparazione directory su server
echo "STEP 1: Preparazione directory su server..."

ssh $SERVER << 'EOF'
set -e
# Crea directory target se non esiste
sudo mkdir -p /var/www/ofinder-frontend/html

# Crea directory backup se non esiste
sudo mkdir -p /opt/mitfwk/backups

# Backup frontend esistente (se presente)
if [ -f /var/www/ofinder-frontend/html/index.html ]; then
    BACKUP_NAME=frontend-backup-$(date +%Y%m%d-%H%M%S).tar.gz
    echo "  Backup frontend esistente..."
    sudo tar -czf /opt/mitfwk/backups/$BACKUP_NAME -C /var/www/ofinder-frontend/html .
    echo "  Backup salvato: /opt/mitfwk/backups/$BACKUP_NAME"
else
    echo "  Nessun frontend esistente (primo deploy)"
fi

# Pulizia directory target
echo "  Pulizia directory target..."
sudo rm -rf /var/www/ofinder-frontend/html/*

echo "  Directory pronta"
EOF

echo "  Directory preparata su server"
echo ""

# STEP 2: Creazione pacchetto locale
echo "STEP 2: Creazione pacchetto..."

cd "$FE_DIR/dist"

# Rimuovi vecchi pacchetti nella dist/
rm -f frontend-ofinder-*.tar.gz 2>/dev/null || true

# Crea pacchetto
tar -czf $PACKAGE_NAME *

PACKAGE_SIZE=$(du -h $PACKAGE_NAME | cut -f1)
echo "  Pacchetto creato: $PACKAGE_NAME"
echo "  Dimensione: $PACKAGE_SIZE"
echo ""

# STEP 3: Upload su server
echo "STEP 3: Upload su server..."

# Upload ottimizzato (compressione + keep-alive + cipher veloce)
scp -C -o Compression=yes -o ServerAliveInterval=30 -c aes128-gcm@openssh.com \
    $PACKAGE_NAME $SERVER:/tmp/

echo "  Upload completato"
echo ""

# STEP 4: Estrazione e permessi su server
echo "STEP 4: Estrazione e configurazione..."

ssh $SERVER << EOF
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
ls -t frontend-backup-*.tar.gz 2>/dev/null | tail -n +6 | xargs -r rm -f || true
BACKUP_COUNT=\$(ls frontend-backup-*.tar.gz 2>/dev/null | wc -l)
echo "  Backup frontend mantenuti: \$BACKUP_COUNT"
EOF

echo "  Estrazione completata"
echo ""

# STEP 5: Verifica deployment
echo "STEP 5: Verifica deployment..."

ssh $SERVER << 'EOF'
set -e
if [ ! -f /var/www/ofinder-frontend/html/index.html ]; then
    echo "ERRORE: index.html non trovato!"
    exit 1
fi

if [ ! -d /var/www/ofinder-frontend/html/assets ]; then
    echo "ERRORE: assets/ non trovata!"
    exit 1
fi

FILE_COUNT=$(find /var/www/ofinder-frontend/html -type f | wc -l)
echo "  File deployati: $FILE_COUNT"
echo "  Frontend deployato correttamente"
EOF

echo "  Verifica completata"
echo ""

# Pulizia locale
cd "$FE_DIR/dist"
rm -f $PACKAGE_NAME

echo "=========================================="
echo "Deploy completato con successo!"
echo "=========================================="
echo ""
echo "Frontend deployato in: $TARGET_DIR"
echo "Backup remoti: /opt/mitfwk/backups/frontend-backup-*"
echo ""
echo "PROSSIMO STEP:"
echo "  1. Aggiorna configurazione nginx per servire frontend"
echo "  2. Test: https://ofinder.it"
echo ""
echo "Configurazione nginx richiesta:"
echo "  location / {"
echo "      root $TARGET_DIR;"
echo "      index index.html;"
echo "      try_files \$uri \$uri/ /index.html;"
echo "  }"
echo ""
