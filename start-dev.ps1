# Dev watchdog: keeps the ECO API (and optionally the Angular dev server) alive.
# Usage:
#   powershell -ExecutionPolicy Bypass -File start-dev.ps1          -> API only
#   powershell -ExecutionPolicy Bypass -File start-dev.ps1 -WithUi  -> API + ng serve
#
# Leave the window open while developing. Press Ctrl+C to stop.

param([switch]$WithUi)

$ErrorActionPreference = 'SilentlyContinue'

function Test-Port($port) {
    (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) -ne $null
}

function Start-Api {
    Write-Host "$(Get-Date -Format HH:mm:ss) starting ECO.Api..." -ForegroundColor Cyan
    Start-Process -FilePath 'dotnet' `
        -ArgumentList 'run', '--no-build', '--project', "$PSScriptRoot\ECO.Api", '--urls', 'https://localhost:7085' `
        -WindowStyle Hidden
}

function Start-Ui {
    Write-Host "$(Get-Date -Format HH:mm:ss) starting ng serve..." -ForegroundColor Cyan
    Start-Process -FilePath 'cmd.exe' `
        -ArgumentList '/c', "cd /d `"$PSScriptRoot\Client`" && set NODE_OPTIONS=--max-old-space-size=6144&& npx ng serve --port 4200" `
        -WindowStyle Hidden
}

if (-not (Test-Path "$PSScriptRoot\ECO.Api")) {
    Write-Host "Run this script from the solution root (H:\All-Project\API\ECO)" -ForegroundColor Red
    exit 1
}

Start-Api
if ($WithUi) { Start-Ui }

Write-Host "Watchdog running. API: https://localhost:7085 - checking every 5s." -ForegroundColor Green

while ($true) {
    Start-Sleep -Seconds 5
    if (-not (Test-Port 7085)) {
        Write-Host "$(Get-Date -Format HH:mm:ss) API is down - restarting." -ForegroundColor Yellow
        Start-Api
    }
}
