#!/bin/bash
# test-ofinder.sh - Esegue solo i test OFinder
# Utile per verifiche rapide senza deploy

set -e

echo "========================================="
echo "===   Test OFinder (solo test)       ==="
echo "========================================="
echo ""

echo "Esecuzione test OFinder..."
dotnet test Tests/MIT.Fwk.Tests.WebApi/MIT.Fwk.Tests.WebApi.csproj \
    --filter "FullyQualifiedName~OFinderTests" \
    --logger "console;verbosity=normal"

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================="
    echo "✓ Tutti i test OFinder sono passati!"
    echo "========================================="
    echo ""
    echo "Test eseguiti:"
    echo "  1. TestAllOFinderEntities_ShouldSucceed (9 entità)"
    echo "  2. Performer_CustomLogic_ShouldSucceed"
    echo "  3. PerformerReview_CustomLogic_ShouldSucceed"
    echo ""
else
    echo ""
    echo "========================================="
    echo "✗ Alcuni test sono falliti!"
    echo "========================================="
    echo ""
    echo "Verifica gli errori sopra e correggi prima del deployment."
    echo ""
    exit 1
fi
