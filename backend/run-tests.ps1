# PowerShell script to run tests with coverage
param(
    [string]$Configuration = "Debug",
    [string]$Framework = "net8.0",
    [switch]$Coverage,
    [switch]$Watch,
    [string]$Filter = ""
)

Write-Host "Running Prock Backend Tests" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Framework: $Framework" -ForegroundColor Yellow

# Base test command
$testCommand = "dotnet test backend.Tests/backend.Tests.csproj --configuration $Configuration --framework $Framework --verbosity normal"

# Add filter if specified
if ($Filter) {
    $testCommand += " --filter `"$Filter`""
    Write-Host "Filter: $Filter" -ForegroundColor Yellow
}

# Add coverage if requested
if ($Coverage) {
    Write-Host "Coverage: Enabled" -ForegroundColor Yellow
    $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory TestResults"
    
    # Run tests with coverage
    Invoke-Expression $testCommand
    
    # Generate coverage report
    if (Test-Path "TestResults") {
        Write-Host "Generating coverage report..." -ForegroundColor Green
        $coverageFile = Get-ChildItem -Path "TestResults" -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
        if ($coverageFile) {
            dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources
            reportgenerator -reports:$coverageFile.FullName -targetdir:TestResults/CoverageReport -reporttypes:"Html;TextSummary"
            Write-Host "Coverage report generated at: TestResults/CoverageReport/index.html" -ForegroundColor Green
        }
    }
} elseif ($Watch) {
    Write-Host "Watch mode: Enabled" -ForegroundColor Yellow
    $testCommand += " --watch"
    Invoke-Expression $testCommand
} else {
    # Run tests normally
    Invoke-Expression $testCommand
}

Write-Host "Test execution completed!" -ForegroundColor Green

