# ============================================================================
# MIT Framework - Database Migration Helper for IJsonApiDbContext
# ============================================================================
# This script automates the creation of Entity Framework migrations for
# all DbContexts that implement IJsonApiDbContext.
# Each DbContext will have its own migration table in the database.
#
# Usage:
#   .\Add-Migration.ps1 -Name "YourMigrationName"
#   .\Add-Migration.ps1 -Name "YourMigrationName" -Context "JsonApiDbContext"
#   .\Add-Migration.ps1 -Remove
#   .\Add-Migration.ps1 -Remove -Context "OtherDbContext"
#   .\Add-Migration.ps1 -Update
#   .\Add-Migration.ps1 -List
#
# Parameters:
#   -Name: The name of the migration (required unless using -Remove, -Update, or -List)
#   -Context: Specific DbContext name (optional - if not specified, applies to all contexts)
#   -Remove: Remove the last migration instead of adding a new one
#   -Update: Apply all pending migrations to the database
#   -List: Show information about all IJsonApiDbContext implementations
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Name,

    [Parameter(Mandatory=$false)]
    [string]$Context,

    [Parameter(Mandatory=$false)]
    [switch]$Remove,

    [Parameter(Mandatory=$false)]
    [switch]$Update,

    [Parameter(Mandatory=$false)]
    [switch]$List
)

$StartupProject = "Src\MIT.Fwk.WebApi\MIT.Fwk.WebApi.csproj"

# ============================================================================
# Function: Get-JsonApiDbContexts
# Discovers all DbContext classes that implement IJsonApiDbContext
# ============================================================================
function Get-JsonApiDbContexts {
    Write-Host "Discovering DbContexts that implement IJsonApiDbContext..." -ForegroundColor Yellow
    Write-Host ""

    $contexts = @()
    $srcPath = Join-Path $PSScriptRoot "Src"

    # Search for files containing DbContext implementations with IJsonApiDbContext
    $files = Get-ChildItem -Path $srcPath -Recurse -Filter "*.cs" |
        Select-String -Pattern ":\s*DbContext.*IJsonApiDbContext|IJsonApiDbContext.*DbContext" |
        Select-Object -ExpandProperty Path -Unique

    foreach ($file in $files) {
        # Read the file content
        $content = Get-Content $file -Raw

        # Extract DbContext class name
        # Use (?s) flag to make . match newlines (for primary constructors and multi-line declarations)
        if ($content -match "(?s)class\s+(\w+).*?IJsonApiDbContext") {
            $contextName = $Matches[1]

            # Extract namespace to determine project path
            if ($content -match "namespace\s+([\w\.]+)") {
                $namespace = $Matches[1]

                # Determine project path from file location
                $relativePath = $file.Substring($srcPath.Length + 1)
                $projectFolder = $relativePath.Split([IO.Path]::DirectorySeparatorChar)[0]
                $projectPath = "Src\$projectFolder"

                $contexts += [PSCustomObject]@{
                    Name = $contextName
                    ProjectPath = $projectPath
                    Namespace = $namespace
                    FilePath = $file
                }

                Write-Host "  Found: $contextName in $projectPath" -ForegroundColor Green
            }
        }
    }

    if ($contexts.Count -eq 0) {
        Write-Host "  No DbContexts implementing IJsonApiDbContext found!" -ForegroundColor Red
    }

    Write-Host ""
    return $contexts
}

function Show-Help {
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host " MIT Framework - IJsonApiDbContext Migration Helper" -ForegroundColor Cyan
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage Examples:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Show migration info for all contexts:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -List" -ForegroundColor White
    Write-Host ""
    Write-Host "  Add new migration to all contexts:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Name `"AddUserPreferences`"" -ForegroundColor White
    Write-Host ""
    Write-Host "  Add new migration to specific context:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Name `"AddProducts`" -Context `"OtherDbContext`"" -ForegroundColor White
    Write-Host ""
    Write-Host "  Remove last migration from all contexts:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Remove" -ForegroundColor White
    Write-Host ""
    Write-Host "  Remove last migration from specific context:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Remove -Context `"JsonApiDbContext`"" -ForegroundColor White
    Write-Host ""
    Write-Host "  Apply pending migrations to all databases:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Update" -ForegroundColor White
    Write-Host ""
    Write-Host "  Apply pending migrations to specific database:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Update -Context `"OtherDbContext`"" -ForegroundColor White
    Write-Host ""
    Write-Host "  Apply specific migration to specific database:" -ForegroundColor Gray
    Write-Host "    .\Add-Migration.ps1 -Update -Context `"OtherDbContext`" -Name `"AddProducts`"" -ForegroundColor White
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Show-MigrationInfo {
    param(
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$ContextInfo
    )

    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host " $($ContextInfo.Name) Information" -ForegroundColor Cyan
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Context: $($ContextInfo.Name)" -ForegroundColor Green
    Write-Host "Project: $($ContextInfo.ProjectPath)" -ForegroundColor Green
    Write-Host "Namespace: $($ContextInfo.Namespace)" -ForegroundColor Green
    Write-Host "Migration Table: __EFMigrationsHistory_$($ContextInfo.Name)" -ForegroundColor Green
    Write-Host ""

    # Check for existing migrations in Migrations folder
    $migrationsPath = Join-Path $PSScriptRoot "$($ContextInfo.ProjectPath)\Migrations"
    if (Test-Path $migrationsPath) {
        $migrationFiles = Get-ChildItem -Path $migrationsPath -Exclude "*ModelSnapshot.cs","*Designer.cs" | Sort-Object Name #-Filter "*.cs"
        if ($migrationFiles.Count -gt 0) {
            Write-Host "Existing migrations: $($migrationFiles.Count)" -ForegroundColor Green
            Write-Host ""
            Write-Host "Latest migrations:" -ForegroundColor Yellow
            $migrationFiles | Select-Object -Last 5 | ForEach-Object {
                Write-Host "  - $($_.BaseName)" -ForegroundColor Gray
            }
        } else {
            Write-Host "No migrations found yet" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Migrations folder not found: $migrationsPath" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
}

# Discover all DbContexts
$allContexts = Get-JsonApiDbContexts

if ($allContexts.Count -eq 0) {
    Write-Host "ERROR: No DbContexts implementing IJsonApiDbContext found!" -ForegroundColor Red
    exit 1
}

# Filter contexts based on -Context parameter
if ($Context) {
    $selectedContexts = $allContexts | Where-Object { $_.Name -eq $Context }
    if ($selectedContexts.Count -eq 0) {
        Write-Host "ERROR: DbContext '$Context' not found!" -ForegroundColor Red
        Write-Host "Available contexts:" -ForegroundColor Yellow
        $allContexts | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Gray
        }
        exit 1
    }
} else {
    $selectedContexts = $allContexts
}

# Handle -List parameter
if ($List) {
    foreach ($ctx in $selectedContexts) {
        Show-MigrationInfo -ContextInfo $ctx
    }
    exit 0
}

# Validate parameters
if (-not $Remove -and -not $Update -and -not $Name) {
    Write-Host "ERROR: -Name parameter is required when adding a migration" -ForegroundColor Red
    Show-Help
    exit 1
}

# Check if dotnet ef tool is installed
$efToolInstalled = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host " ERROR: dotnet-ef tool is not installed" -ForegroundColor Red
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install it using:" -ForegroundColor Yellow
    Write-Host "  dotnet tool install --global dotnet-ef" -ForegroundColor White
    Write-Host ""
    Write-Host "Or update it using:" -ForegroundColor Yellow
    Write-Host "  dotnet tool update --global dotnet-ef" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Verify startup project exists
$startupProject = Join-Path $PSScriptRoot $StartupProject
if (-not (Test-Path $startupProject)) {
    Write-Host "ERROR: Startup project not found: $startupProject" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host " MIT Framework - IJsonApiDbContext Migration Tool" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "Startup Project: $StartupProject" -ForegroundColor Green
Write-Host "Target Contexts: $($selectedContexts.Count)" -ForegroundColor Green
foreach ($ctx in $selectedContexts) {
    Write-Host "  - $($ctx.Name) ($($ctx.ProjectPath))" -ForegroundColor Gray
}
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

$hasErrors = $false

# Process each selected context
foreach ($ctx in $selectedContexts) {
    $contextName = $ctx.Name
    $projectPath = Join-Path $PSScriptRoot $ctx.ProjectPath
    $migrationTable = "__EFMigrationsHistory_$contextName"
    $migrationsOutputDir = "Migrations"

    # Verify project path exists
    if (-not (Test-Path $projectPath)) {
        Write-Host "ERROR: Project path not found: $projectPath" -ForegroundColor Red
        $hasErrors = $true
        continue
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " Processing: $contextName" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Project: $($ctx.ProjectPath)" -ForegroundColor Gray
    Write-Host "Migration Table: $migrationTable" -ForegroundColor Gray
    Write-Host "Migrations Folder: $migrationsOutputDir" -ForegroundColor Gray
    Write-Host ""

    Set-Location $projectPath

    # Execute the appropriate command
    if ($Remove) {
        Write-Host "Removing last migration from $contextName..." -ForegroundColor Yellow

        dotnet ef migrations remove `
            --context $contextName `
            --startup-project $startupProject

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Migration removed successfully from $contextName" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to remove migration from $contextName" -ForegroundColor Red
            $hasErrors = $true
        }
    }
    elseif ($Update -and $Name) {
        Write-Host "Updating migration '$Name' for $contextName..." -ForegroundColor Yellow

        dotnet ef database update $Name `
            --context $contextName `
            --startup-project $startupProject

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database updated to migration '$Name' for $contextName" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to update database to migration '$Name' for $contextName" -ForegroundColor Red
            $hasErrors = $true
        }
    }
    elseif ($Update) {
        Write-Host "Applying pending migrations to $contextName..." -ForegroundColor Yellow

        dotnet ef database update `
            --context $contextName `
            --startup-project $startupProject

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database updated successfully for $contextName" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to update database for $contextName" -ForegroundColor Red
            $hasErrors = $true
        }
    }
    else {
        Write-Host "Creating migration '$Name' for $contextName..." -ForegroundColor Yellow

        dotnet ef migrations add $Name `
            --context $contextName `
            --startup-project $startupProject `
            --output-dir $migrationsOutputDir

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Migration created successfully for $contextName" -ForegroundColor Green
            Write-Host "  Location: $projectPath\$migrationsOutputDir\" -ForegroundColor Gray
        } else {
            Write-Host "✗ Failed to create migration for $contextName" -ForegroundColor Red
            $hasErrors = $true
        }
    }
}

# Final summary
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
if ($hasErrors) {
    Write-Host " COMPLETED WITH ERRORS" -ForegroundColor Red
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Some operations failed. Please review the output above." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host " ALL OPERATIONS COMPLETED SUCCESSFULLY" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Green
    Write-Host ""

    if (-not $Remove -and -not $Update) {
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Review the generated migration files" -ForegroundColor Gray
        Write-Host "  2. Apply migrations: .\Add-Migration.ps1 -Update" -ForegroundColor Gray
        if ($Context) {
            Write-Host "  3. Or for specific context: .\Add-Migration.ps1 -Update -Context `"$Context`"" -ForegroundColor Gray
        }
        Write-Host "  4. Or enable auto-migration in customsettings.json: `"EnableAutoMigrations`": `"true`"" -ForegroundColor Gray
        Write-Host ""
    }
}

Set-Location $PSScriptRoot

Write-Host ""
