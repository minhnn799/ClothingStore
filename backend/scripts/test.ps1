$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localDotnet = Join-Path $env:USERPROFILE '.dotnet/dotnet.exe'
$dotnet = if (Test-Path $localDotnet) { $localDotnet } else { 'dotnet' }
$previous = $env:CLOTHESSTORE_TEST_CONNECTION
try {
    if (-not $env:CLOTHESSTORE_TEST_CONNECTION) {
        $env:CLOTHESSTORE_TEST_CONNECTION = $env:ConnectionStrings__DefaultConnection
        if (-not $env:CLOTHESSTORE_TEST_CONNECTION) {
            $configFile = Join-Path $projectRoot '01.API/appsettings.Development.json'
            if (-not (Test-Path $configFile)) { $configFile = Join-Path $projectRoot '01.API/appsettings.json' }
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            $env:CLOTHESSTORE_TEST_CONNECTION = $config.ConnectionStrings.DefaultConnection
        }
    }
    if (-not $env:CLOTHESSTORE_TEST_CONNECTION) { throw 'Configure CLOTHESSTORE_TEST_CONNECTION with PostgreSQL credentials and CREATEDB permission.' }
    & $dotnet test (Join-Path $projectRoot 'ClothingStore.slnx') @args
    $testExitCode = $LASTEXITCODE
} finally {
    $env:CLOTHESSTORE_TEST_CONNECTION = $previous
}
exit $testExitCode
