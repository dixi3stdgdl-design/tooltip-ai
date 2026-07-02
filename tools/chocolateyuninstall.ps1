$ErrorActionPreference = 'Stop'

$serviceName = 'TooltipAI'
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Stop-Service -Name $serviceName -Force
    sc.exe delete $serviceName | Out-Null
    Write-Host "Tooltip AI service removed." -ForegroundColor Yellow
}

$shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Tooltip AI.lnk'
if (Test-Path $shortcutPath) {
    Remove-Item $shortcutPath -Force
}

$appDataPath = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'TooltipAI'
if (Test-Path $appDataPath) {
    Remove-Item $appDataPath -Recurse -Force
}

Write-Host "Tooltip AI uninstalled successfully." -ForegroundColor Green
