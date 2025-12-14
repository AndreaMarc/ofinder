# Backend Ofinder

API Backend per Ofinder - Framework .NET 10.0

## Quick Start - Deployment

### Windows
```powershell
cd BE
.\deploy.ps1
```

### Linux/Mac/WSL
```bash
cd BE
chmod +x deploy.sh  # Solo la prima volta
./deploy.sh
```

## Documentazione Completa

- **[DEPLOYMENT-GUIDE.md](DEPLOYMENT-GUIDE.md)** - Guida completa al deployment su OVH

## Struttura Progetto

```
BE/
├── Src/                          # Codice sorgente
│   ├── MIT.Fwk.WebApi/          # API principale
│   ├── MIT.Fwk.Core/            # Libreria core
│   ├── MIT.Fwk.Infrastructure/  # Infrastruttura
│   ├── MIT.Fwk.Examples/        # Moduli custom
│   ├── MIT.Fwk.Scheduler/       # Jobs schedulati
│   └── MIT.Fwk.Licensing/       # Sistema licenze
├── publish/                      # Output build
│   └── linux-x64-selfcontained/ # Build per deployment
├── backups/                      # Backup locali
├── deploy.ps1                    # Script deployment Windows
├── deploy.sh                     # Script deployment Linux/Mac
└── DEPLOYMENT-GUIDE.md          # Guida completa
```

## Build Locale

```bash
cd Src/MIT.Fwk.WebApi
dotnet build
```

## Test Locale

```bash
cd Src/MIT.Fwk.WebApi
dotnet run
```

API disponibile su: http://localhost:7002

## Deployment

Lo script automatico esegue:
1. Backup locale dal server
2. Build self-contained per Linux
3. Creazione pacchetto tar.gz
4. Upload su server OVH
5. Deployment con rollback automatico
6. Test finale

## Server di Produzione

- **Server**: 51.210.6.193 (OVH)
- **Dominio**: ofinder.it
- **API Endpoint**: https://ofinder.it/api
- **Porta Interna**: 7002

## Note Importanti

- **SEMPRE** usare `--self-contained true` per il build
- **SEMPRE** fare backup locale prima del deployment
- File `license.lic` obbligatorio per l'avvio
- Deployment causa ~2 minuti di downtime

## Troubleshooting

Vedi [DEPLOYMENT-GUIDE.md](DEPLOYMENT-GUIDE.md) sezione "Troubleshooting" e "Lezioni Apprese".
