$ErrorActionPreference = 'Stop'

$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$exePath = Join-Path $toolsDir 'TooltipAI.Service.exe'

$packageArgs = @{
  packageName    = $env:ChocolateyPackageName
  unzipLocation  = $toolsDir
  fileType       = 'exe'
  file           = $exePath
  silentArgs     = '/S'
  validExitCodes = @(0)
  softwareName   = 'Tooltip AI*'
}

Install-ChocolateyPackage @packageArgs

$serviceName = 'TooltipAI'
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    Start-Service -Name $serviceName
    Write-Host "Tooltip AI service started." -ForegroundColor Green
}

$shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Tooltip AI.lnk'
Install-ChocolateyShortcut -ShortcutFilePath $shortcutPath -TargetPath $exePath -Description 'AI-powered tooltip overlay'
