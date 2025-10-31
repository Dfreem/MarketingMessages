[CmdletBinding()]
param ()

function Ensure-DotnetEfTool {
    Write-Host "Checking EF Core CLI tool availability..."
    $result = dotnet tool restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore dotnet-ef tool. Output:`n$result"
        exit 1
    }
}

function Get-DbContexts {
    Write-Host "Retrieving available DbContexts..."
    $contexts = dotnet ef dbcontext list 2>$null
    if (-not $contexts) {
        Write-Error "No DbContext found in the project."
        exit 1
    }
    return $contexts
}

function Prompt-ForContext($contexts) {
    if ($contexts.Count -eq 1) {
        Write-Host "Single DbContext detected: $($contexts[0])"
        return $contexts[0]
    }
    else {
        Write-Host "Multiple DbContexts found. Please select one:"
        for ($i=0; $i -lt $contexts.Count; $i++) {
            Write-Host " [$($i+1)] $($contexts[$i])"
        }
        while ($true) {
            $selection = Read-Host "Enter the number of the DbContext to use"
            if ([int]::TryParse($selection, [ref]$null) -and
                $selection -ge 1 -and $selection -le $contexts.Count) {
                return $contexts[$selection - 1]
            }
            else {
                Write-Warning "Invalid selection. Try again."
            }
        }
    }
}

function Prompt-ForMigrationName {
    while ($true) {
        $name = Read-Host "Enter migration name (cannot be empty)"
        if (![string]::IsNullOrWhiteSpace($name)) {
            return $name.Trim()
        }
        Write-Warning "Migration name cannot be empty."
    }
}

function Prompt-ForSqlFilePath([string]$migrationName, [string]$contextName) {
    $defaultDir = "Migrations\" + $contextName
    if (-not (Test-Path $defaultDir)) {
        New-Item -ItemType Directory -Path $defaultDir | Out-Null
    }
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $defaultFileName = "$migrationName`_$timestamp.sql"
    $defaultPath = Join-Path $defaultDir $defaultFileName

    while ($true) {
        $path = Read-Host "Enter output file path for SQL script (default: $defaultPath)"
        if ([string]::IsNullOrWhiteSpace($path)) {
            return $defaultPath
        }
        else {
            try {
                $fullPath = Resolve-Path -Path $path -ErrorAction Stop
                # If path exists and is a directory, append default file name
                if ((Get-Item $fullPath).PSIsContainer) {
                    return Join-Path $fullPath $defaultFileName
                }
                else {
                    return $fullPath
                }
            }
            catch {
                # If path does not exist, try to check parent folder
                $parent = Split-Path -Path $path
                if (-not (Test-Path $parent)) {
                    Write-Warning "Parent directory '$parent' does not exist. Please enter a valid path."
                }
                else {
                    return (Resolve-Path -Path $parent).ProviderPath + "\" + (Split-Path -Leaf $path)
                }
            }
        }
    }
}

function Run-Migration($migrationName, $dbContext) {
    Write-Host "Adding migration '$migrationName' for context '$dbContext'..."
    dotnet ef migrations add $migrationName --context $dbContext
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Migration add failed."
        exit 1
    }
}

function Update-Database($dbContext) {
    Write-Host "Updating database for context '$dbContext'..."
    dotnet ef database update --context $dbContext
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Database update failed."
        exit 1
    }
}

function Generate-SqlScript($sqlFilePath, $dbContext) {
    Write-Host "Generating idempotent SQL script at '$sqlFilePath'..."
    dotnet ef migrations script --idempotent --context $dbContext -o $sqlFilePath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Generating SQL script failed."
        exit 1
    }
}

# --- Main script ---

Ensure-DotnetEfTool

$contexts = Get-DbContexts
$selectedContext = Prompt-ForContext $contexts

$migrationName = Prompt-ForMigrationName
$contextName = $selectedContext.Replace("Context", "").Split('.')[0]
$sqlFilePath = Prompt-ForSqlFilePath -migrationName $migrationName -contextName $contextName
Run-Migration -migrationName $migrationName -dbContext $selectedContext
Update-Database -dbContext $selectedContext
Generate-SqlScript -sqlFilePath $sqlFilePath -dbContext $selectedContext

Write-Host ""
Write-Host "✅ Migration process complete."
Write-Host "Migration Name: $migrationName"
Write-Host "DbContext: $selectedContext"
Write-Host "SQL script saved to: $sqlFilePath"
