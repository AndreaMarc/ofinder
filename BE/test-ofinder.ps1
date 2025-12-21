# test-ofinder.ps1 - Esegue solo i test OFinder
# Utile per verifiche rapide senza deploy

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "===   Test OFinder (solo test)       ===" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Esecuzione test OFinder..." -ForegroundColor Yellow
dotnet test Tests\MIT.Fwk.Tests.WebApi\MIT.Fwk.Tests.WebApi.csproj `
    --filter "FullyQualifiedName~OFinderTests" `
    --logger "console;verbosity=normal"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "✓ Tutti i test OFinder sono passati!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Test eseguiti:" -ForegroundColor Cyan
    Write-Host "  1. TestAllOFinderEntities_ShouldSucceed (9 entità)" -ForegroundColor White
    Write-Host "  2. Performer_CustomLogic_ShouldSucceed" -ForegroundColor White
    Write-Host "  3. PerformerReview_CustomLogic_ShouldSucceed" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "✗ Alcuni test sono falliti!" -ForegroundColor Red
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Verifica gli errori sopra e correggi prima del deployment." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
