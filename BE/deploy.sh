#!/bin/bash
# deploy.sh - Deployment automatico Ofinder su OVH
# Autore: Claude Code
# Data: 2025-12-14

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

# 1. Test OFinder
echo "STEP 1: Esecuzione test OFinder..."
dotnet test Tests/MIT.Fwk.Tests.WebApi/MIT.Fwk.Tests.WebApi.csproj --filter "FullyQualifiedName~OFinderTests" --logger "console;verbosity=minimal" --no-restore

if [ $? -ne 0 ]; then
    echo ""
    echo "ERRORE: I test OFinder sono falliti!"
    echo "Il deployment è stato annullato per evitare di deployare codice non funzionante."
    echo ""
    exit 1
fi

echo "✓ Test OFinder completati con successo"
echo ""

# 2. Build
echo "STEP 2: Building backend (self-contained)..."
cd Src/MIT.Fwk.WebApi
dotnet publish -c Release -r linux-x64 --self-contained true -o ../../publish/linux-x64-selfcontained
cd ../..
echo "✓ Build completato"
echo ""

# 3. Package
echo "STEP 3: Creando pacchetto deployment..."
cd publish/linux-x64-selfcontained
PACKAGE="ofinder-api-$(date +%Y%m%d-%H%M%S).tar.gz"
tar -czf $PACKAGE *
echo "✓ Pacchetto creato: $PACKAGE (dimensione: $(du -h $PACKAGE | cut -f1))"
echo ""

# 4. Upload
echo "STEP 4: Upload su server OVH (potrebbe richiedere qualche minuto)..."
scp $PACKAGE ubuntu@51.210.6.193:/opt/mitfwk/
echo "✓ Upload completato"
echo ""

# 5. Deploy
echo "STEP 5: Deployment su server..."
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

# 6. Test API
echo "STEP 6: Test API finale..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" https://ofinder.it/api)
if [ "$HTTP_CODE" = "401" ] || [ "$HTTP_CODE" = "200" ]; then
    echo "✓ API risponde correttamente (HTTP $HTTP_CODE)"
else
    echo "✗ ATTENZIONE: API non risponde come previsto (HTTP $HTTP_CODE)"
    echo "  Verifica i log: ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -n 100'"
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
echo "Per controllare i log sul server:"
echo "  ssh ubuntu@51.210.6.193 'sudo journalctl -u mitfwk-api -f'"
echo ""
